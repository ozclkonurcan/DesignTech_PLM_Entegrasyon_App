using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQServiceLibraryFor.MVC
{
    //public class RabbitMQService
    //{
    //    private readonly IConfiguration _configuration;

    //    public RabbitMQService(IConfiguration configuration)
    //    {
    //        _configuration = configuration;
    //    }

    //    public void SendMessage(string message)
    //    {
    //        // RabbitMQ ile mesaj gönderme kodları burada olmalı
    //    }

    //    public string ReceiveMessage()
    //    {
    //        // RabbitMQ ile mesaj alıp cevap gönderme kodları burada olmalı
    //        return "Cevap Mesajı";
    //    }
    //}



    // RabbitMQService.cs
    public class RabbitMQService
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;

        public RabbitMQService(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
        }

        public void SendMessage(string message)
        {
            using (var channel = _connection.CreateModel())
            {
                channel.QueueDeclare(queue: _configuration["RabbitMQ:QueueName"], durable: false, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: _configuration["RabbitMQ:QueueName"], basicProperties: null, body: body);
            }
        }

        public string ReceiveMessage()
        {
            using (var channel = _connection.CreateModel())
            {
                channel.QueueDeclare(queue: _configuration["RabbitMQ:QueueName"], durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);

                string receivedMessage = null;
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    receivedMessage = Encoding.UTF8.GetString(body);
                };

                channel.BasicConsume(queue: _configuration["RabbitMQ:QueueName"], autoAck: true, consumer: consumer);

                return receivedMessage;
            }
        }
    }
}
