namespace BankingApp.TransferService.Application.Interfaces;

public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, T message) where T : class;
}