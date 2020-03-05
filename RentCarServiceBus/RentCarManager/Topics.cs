using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

using RentCarManager.Model;
using RentCarManager.Helpers;

namespace RentCarManager
{
    public class Topics
    {
        private readonly string _connectionString;
        private readonly string _topicName;

        public Topics(string topicName)
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ServiceBusNamespaceConnection"].ConnectionString;
            _topicName = topicName;
        }

        public async Task SendMessagesAsync<T>(T data, string label, string subscription)
        {
            var message = new Message(MessageConverters.GetJsonBytes(data))
            {
                Label = label,
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                To = subscription
            };

            var topicClient = new TopicClient(_connectionString, _topicName);

            await topicClient.SendAsync(message);

            Console.WriteLine($"Mensagem enviada: {message.Label} | Tópico: {_topicName} | Mensagem Id: {message.MessageId}");
        }

        public void ReceiveMessages(string subscription)
        {
            var entityPath = EntityNameHelper.FormatSubscriptionPath(_topicName, subscription);
            var receiver = new MessageReceiver(_connectionString, entityPath, ReceiveMode.PeekLock, RetryPolicy.Default, 100);

            var messageBody = new RentResponse { ResponseMessage = "Nenhuma mensagem disponível" };

            receiver.RegisterMessageHandler(async (message, cancellToken) =>
            {
                messageBody = MessageConverters.GetJsonMessageBody<RentResponse>(message.Body);
                messageBody.MessageId = message.MessageId;

                if (cancellToken.IsCancellationRequested)
                    await receiver.AbandonAsync(message.SystemProperties.LockToken);
                else
                    await receiver.CompleteAsync(message.SystemProperties.LockToken);

                Console.WriteLine($"{subscription} | {messageBody}");

            }, new MessageHandlerOptions((o) =>
            {
                Console.WriteLine($"Erro. Mensagem: {o.Exception?.Message}, Entidade: {o.ExceptionReceivedContext.EntityPath}");
                return Task.CompletedTask;
            })
            {
                AutoComplete = false, //Indica que deve ser terminado pelo callback
                MaxConcurrentCalls = 1 //Indica a quantidade de processos concorrentes para processar a mensagem
            });
        }
    }
}
