using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using RentCarManager.Model;
using RentCarManager.Helpers;

namespace RentCarManager
{
    public class Queues
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public Queues(string queueName)
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ServiceBusNamespaceConnection"].ConnectionString;
            _queueName = queueName;
        }

        public async Task<string> SendMessagesAsync<T>(T data, string label)
        {
            var message = new Message(MessageConverters.GetJsonBytes(data))
            {
                ContentType = "application/json",
                Label = label,
                MessageId = Guid.NewGuid().ToString()
            };

            var senderClient = new QueueClient(_connectionString, _queueName);

            await senderClient.SendAsync(message);

            return $"Mensagem enviada: {message.Label}. Mensagem Id: {message.MessageId}";
        }

        public void ReceiveMessagesAsync(string label)
        {
            var receiverClient = new QueueClient(_connectionString, _queueName, ReceiveMode.PeekLock);

            var messageBody = new RentResponse { ResponseMessage = "Nenhuma mensagem disponível" };

            // register the RegisterMessageHandler callback
            receiverClient.RegisterMessageHandler(async (message, cancellationTokenRegister) =>
            {
                var isInvalidMessageLabel = string.IsNullOrEmpty(message.Label) || !message.Label.Equals(label,
                    StringComparison.InvariantCultureIgnoreCase);

                if (isInvalidMessageLabel)
                {
                    var reason = "Não é a mensagem que estou esperando";

                    messageBody.ResponseMessage = reason;

                    await receiverClient.DeadLetterAsync(message.SystemProperties.LockToken, reason);
                }

                messageBody = MessageConverters.GetJsonMessageBody<RentResponse>(message.Body);
                messageBody.MessageId = message.MessageId;

                if (cancellationTokenRegister.IsCancellationRequested)
                    await receiverClient.AbandonAsync(message.SystemProperties.LockToken);
                else
                    await receiverClient.CompleteAsync(message.SystemProperties.LockToken);

                Console.WriteLine(messageBody);

            }, new MessageHandlerOptions((o) =>
            {
                Console.WriteLine($"Erro. Mensagem: {o.Exception?.Message}, Entidade: {o.ExceptionReceivedContext.EntityPath}");
                return Task.CompletedTask;
            })
            {
                AutoComplete = false, //Indica que deve ser terminado pelo callback
                MaxConcurrentCalls = 1 //Indica a quantidade de processos concorrentes para processar a mensagem
            }
            );            
        }        
    }
}
