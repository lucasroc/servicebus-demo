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
            var cts = new CancellationTokenSource();

            var queues = new Queues(queueName);

            var receiveTask = queues.ReceiveMessagesAsync("Reservas", cts.Token);

            await Task.WhenAll(
                 Task.WhenAny(
                    Task.Run(() => Console.WriteLine($"Nome: {receiveTask.Result}")),
                    Task.Delay(TimeSpan.FromSeconds(10)))
                    .ContinueWith((t) => cts.Cancel()),
                receiveTask);
        }

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("RentCarServer running!");

                var app = new Program();
                await app.Run(QueueName);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.ToString()}");
            }
        }
    }
}
