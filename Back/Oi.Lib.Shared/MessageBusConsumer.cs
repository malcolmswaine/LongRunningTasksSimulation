using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Oi.Lib.Shared.Types;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Oi.Lib.Shared
{
    public class MessageBusConsumer : BackgroundService
    {
        private readonly IConfiguration config;
        private readonly List<IEventProcessor> eventProcessors = [];
        private IConnection connection;
        private IModel channel;
        private string queueName;

        public MessageBusConsumer(IConfiguration config) 
        {
            this.config = config;

            InitRabbit();
        }

        private void InitRabbit()
        {
            Console.WriteLine("Initializing rabbit queue listener");
            var factory = new ConnectionFactory()
            {
                HostName = config["RabbitMQHost"],
                Port = int.Parse(config["RabbitMQPort"])
            };

            try
            {
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                    exchange: "trigger",
                    routingKey: "");

                Console.WriteLine("Successfully bound to queue on exchange trigger");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error binding to rabbit queue {e.ToString()}");
            }

            connection.ConnectionShutdown += RabbitMQConnectionShutdown;
        }

        public void AddEventProcessor(IEventProcessor eventProcessor)
        {
            eventProcessors.Add(eventProcessor);
        }

        public void RemoveEventProcessor(IEventProcessor eventProcessor)
        {
            var foundEventProcessor = eventProcessors.FirstOrDefault(eventProcessor);
            if (foundEventProcessor != null)
            {
                eventProcessors.Remove(foundEventProcessor);
            }
            else
            { 
                Console.WriteLine($"event processor could not be found");
            }
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (moduleHandle, ea) =>
            {
                Console.WriteLine($"Event Received");

                var body = ea.Body;
                var payload = Encoding.UTF8.GetString(body.ToArray());

                var message = JsonConvert.DeserializeObject<GenericMessage>(payload);

                eventProcessors.ForEach(eventProcessor =>
                {
                    eventProcessor.ProcessEvent(message);
                });                
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        private void RabbitMQConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine($"RabbitMQ Connection shutdown");
        }

        public void Dispose()
        {
            Console.WriteLine($"Disposing of Rabbit connection");
            if (channel.IsOpen)
            {
                //channel.Close();
                //connection.Close();
            }
        }

    }
}
