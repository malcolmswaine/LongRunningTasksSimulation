using System.Text.Json;
using System.Text;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using System.Threading.Channels;
using Oi.Lib.Shared.Types;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Oi.Lib.Shared
{
    public class MessageBusPublisher : IMessageBusPublisher, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusPublisher(IConfiguration configuration)
        {
            _configuration = configuration;

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                _connection.ConnectionShutdown += RabbitMQConnectionShutdown;

                Console.WriteLine($"Connected to RabbitMQ");
            }
            catch (Exception x)
            {
                Console.WriteLine($"Error connecting to rabbit {x.ToString()}");
            }
        }


        public void SendMessage(MessageTypeEnum messageType, string message, string sigrConnId)
        {
            var serialized = JsonConvert.SerializeObject(new GenericMessage() { MessageType = messageType, Payload = message, SigrConnId = sigrConnId });
            var body = Encoding.UTF8.GetBytes(serialized);
            _channel.BasicPublish(
                exchange: "trigger",
                routingKey: "",
                basicProperties: null,
                body);
            Console.WriteLine($"sent message {message}");
        }


        private void RabbitMQConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine($"RabbitMQ Connection shutdown");
        }

        public void Dispose()
        {
            Console.WriteLine($"Disposing of Rabbit connection");
            if (_channel.IsOpen)
            {
                //_channel.Close();
                //_connection.Close();
            }
        }
    }

    public interface IMessageBusPublisher
    {
        void SendMessage(MessageTypeEnum messageType, string message, string sigrConnId);
    }
}
