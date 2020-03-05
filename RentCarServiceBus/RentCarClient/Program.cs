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

        public async Task Run(string queueName, string name, string firstName)
        {
            var queues = new Queues(queueName);

            var request =
                new RentRequest
                {
                    Name = name,
                    FirstName = firstName,
                    Description = "Solicitando uma reserva..."
                };

            var sendTask = queues.SendMessagesAsync(request, "Reservas");

            await Task.WhenAll(
                 Task.WhenAny(
                     Task.Run(() => Console.WriteLine(sendTask.Result))
                 )
                 .ContinueWith((t) => sendTask)
                );
        }

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("RentCarClient running!");
                Console.ReadKey();

                var name = string.Empty;
                var firstName = string.Empty;

                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-name":
                            var nameArg = args[i + 1];
                            Console.WriteLine($"Nome: {nameArg}");
                            name = nameArg;
                            break;
                        case "-firstName":
                            var firstNameArg = args[i + 1];
                            Console.WriteLine($"Primeiro Nome: {firstNameArg}");
                            firstName = firstNameArg;
                            break;
                        default:
                            break;
                    }
                }

                var hasNoParameters = string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(firstName);
                if (hasNoParameters)
                {
                    Console.WriteLine("Não existe a opção informada. -name para Nome e -firstName para Primeiro Nome");
                    Console.ReadKey();
                    return;
                }

                var app = new Program();
                await app.Run(QueueName, name, firstName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.ToString()}");
            }
        }
    }
}
