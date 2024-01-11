namespace DesignTech_PLM_Entegrasyon_App.MVC.Services.Rabbitmq
{
    public interface IMessageProducer
    {
        public void SendingMessage<T>(T message);
            }
}
