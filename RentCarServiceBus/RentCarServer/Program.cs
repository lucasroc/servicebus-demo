using System;
using System.Threading;
using System.Threading.Tasks;
using RentCarManager;

namespace RentCarServer
{
    class Program
    {
        const string QueueName = "bootcamp-queue";

        public async Task Run(string queueName)
        {
            Console.ReadKey();
            Console.WriteLine("Reading queue messages...");

            var queues = new Queues(queueName);

            queues.ReceiveMessagesAsync("Reservas");

            await Task.WhenAll(
                 Task.WhenAny(                    
                    Task.Delay(TimeSpan.FromSeconds(5))),                
                Run(QueueName));
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("RentCarServer running!");

                var app = new Program();
                app.Run(QueueName);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.ToString()}");
            }
        }
    }
}
