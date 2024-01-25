using Newtonsoft.Json;

using System.Text;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Services.Rabbitmq
{
    //public class MessageProducer : IMessageProducer
    //{
    //    private readonly IConfiguration _configuration;

    //    public MessageProducer(IConfiguration configuration)
    //    {
    //        _configuration = configuration;
    //    }

    //    public void SendingMessage<T>(T message)
    //    {
    //        var factory = new ConnectionFactory()
    //        {

    //            HostName = _configuration["RabbitMQ:HostName"],
    //            Port = int.Parse(_configuration["RabbitMQ:Port"]),
    //            UserName = _configuration["RabbitMQ:UserName"],
    //            Password = _configuration["RabbitMQ:Password"],
    //            VirtualHost = _configuration["RabbitMQ:VirtualHost"]
    //        };

    //        var conn = factory.CreateConnection();

    //        using var channel = conn.CreateModel();

    //        channel.QueueDeclare("hebelehübele",durable:true,exclusive:true);

    //        var json = JsonConvert.SerializeObject(message);

    //        var body = Encoding.UTF8.GetBytes(json);

    //        channel.BasicPublish("", "hebelehübele", body: body);


    //        var consumer = new EventingBasicConsumer(channel);

    //        consumer.Received += (model, eventArgs) =>
    //        {
    //            var body = eventArgs.Body.ToArray();
    //            var message = Encoding.UTF8.GetString(body);

    //        };
    //    }
    //}
}
