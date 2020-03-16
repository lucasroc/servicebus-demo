using System;
using System.Threading;
using System.Threading.Tasks;
using RentCarManager;
using RentCarManager.Model;

namespace RentCarClient
{
    class Program
    {
        const string QueueName = "bootcamp-queue";
        const string TopicName = "bootcamp-topic";
        const string SubscriptionName = "bootcamp-subs2";

        static int RequestIndex = 0;

        public async Task RunQueue(string queueName)
        {
            Console.ReadKey();
            Console.WriteLine("Sending messages...");

            var queues = new Queues(queueName);

            var requestsList = GetRentRequests();

            var sendTask = queues.SendMessagesAsync(requestsList[RequestIndex], "Reservas");

            //Gerência o index
            RequestIndex++;
            RequestIndex = RequestIndex < requestsList.Length ? RequestIndex : 0;

            await Task.WhenAll(
                     Task.WhenAny(
                         Task.Run(() => Console.WriteLine(sendTask.Result))
                     )
                     .ContinueWith((t) => sendTask),
                    RunQueue(queueName)
                );
        }

        public async Task RunTopic(string topicName)
        {
            Console.ReadKey();
            Console.WriteLine("Sending messages...");

            var topics = new Topics(topicName);

            var requestsList = GetRentRequests();

            var sendTask = topics.SendMessagesAsync(requestsList[RequestIndex], "Reservas", SubscriptionName);

            //Gerência o index
            RequestIndex++;
            RequestIndex = RequestIndex < requestsList.Length ? RequestIndex : 0;

            await Task.WhenAll(
                     Task.WhenAny(
                         Task.Run(() => sendTask)
                     ),
                    RunTopic(topicName)
                );
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("RentCarClient running!");
                Console.ReadKey();
                
                var app = new Program();
                app.RunQueue(QueueName);
                //app.RunTopic(TopicName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.ToString()}");
            }
        }

        private RentRequest[] GetRentRequests()
        {
            return new RentRequest[] {
                new RentRequest
                {
                    Name = "Rocco",
                    FirstName = "Lucas",
                    Description = "Solicitando uma reserva..."
                },
                new RentRequest
                {
                    Name = "Ferreira",
                    FirstName = "João",
                    Description = "Solicitando uma reserva..."
                },
                new RentRequest
                {
                    Name = "Matos",
                    FirstName = "André",
                    Description = "Solicitando uma reserva..."
                }
            };
        }
    }
}
