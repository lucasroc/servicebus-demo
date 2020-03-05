using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using RentCarManager.Model;

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
            var message = new Message(GetJsonBytes(data))
            {
                ContentType = "application/json",
                Label = label,
                MessageId = Guid.NewGuid().ToString()
            };

            var messageSender = new MessageSender(_connectionString, _queueName);

            await messageSender.SendAsync(message);

            return $"Mensagem enviada: {message.Label}. Mensagem Id: {message.MessageId}";
        }

        public async Task<RentResponse> ReceiveMessagesAsync(string label, CancellationToken cancellationToken)
        {
            var messageReceiver = new MessageReceiver(_connectionString, _queueName, ReceiveMode.PeekLock);

            var doneReceiving = new TaskCompletionSource<bool>();
            // close the receiver and factory when the CancellationToken fires 
            cancellationToken.Register(
                async () =>
                {
                    await messageReceiver.CloseAsync();
                    doneReceiving.SetResult(true);
                }
            );

            var messageBody = new RentResponse { ResponseMessage = "Nenhuma mensagem disponível" };

            // register the RegisterMessageHandler callback
            messageReceiver.RegisterMessageHandler(async (message, cancellationTokenRegister) =>
            {
                var isInvalidMessageLabel = string.IsNullOrEmpty(message.Label) || !message.Label.Equals(label,
                    StringComparison.InvariantCultureIgnoreCase);

                if (isInvalidMessageLabel)
                {
                    var reason = "Não é a mensagem que estou esperando";

                    messageBody.ResponseMessage = reason;

                    await messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken, reason);
                }

                messageBody = messageBody ?? GetJsonMessageBody<RentResponse>(message.Body);

                await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
            }, new MessageHandlerOptions((o) =>
            {
                messageBody.ResponseMessage = $"Erro. Mensagem: {o.Exception?.Message}, Entidade: {o.ExceptionReceivedContext.EntityPath}";
                return Task.CompletedTask;
            })
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            }
            );

            await doneReceiving.Task;

            return messageBody;
        }

        private byte[] GetJsonBytes(object data) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        private T GetJsonMessageBody<T>(byte[] messageBody) =>
            JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(messageBody));
    }
}
