using System;
using System.Threading;
using System.Threading.Tasks;
using RentCarManager;

namespace RentCarServer
{
    class Program
    {
        const string QueueName = "bootcamp-queue";
        const string TopicName = "bootcamp-topic";
        const string SubscriptionName = "bootcamp-subs";

        public async Task RunQueue(string queueName)
        {
            Console.ReadKey();
            Console.WriteLine("Reading queue messages...");

            var queues = new Queues(queueName);

            queues.ReceiveMessagesAsync("Reservas");

            await Task.WhenAll(
                 Task.WhenAny(                    
                    Task.Delay(TimeSpan.FromSeconds(5))),                
                RunQueue(queueName));
        }

        public async Task RunTopic(string topicName, string subscription)
        {
            Console.ReadKey();
            Console.WriteLine("Reading subscription messages...");

            var topics = new Topics(topicName);

            topics.ReceiveMessages(subscription);

            await Task.WhenAll(
                 Task.WhenAny(
                    Task.Delay(TimeSpan.FromSeconds(5))),
                RunTopic(topicName, subscription));
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("RentCarServer running!");

                var app = new Program();
                //app.RunQueue(QueueName);
                app.RunTopic(TopicName, SubscriptionName);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.ToString()}");
            }
        }
    }
}
