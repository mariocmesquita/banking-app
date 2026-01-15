using BankingApp.TransferService.Application.Interfaces;
using KafkaFlow.Producers;

namespace BankingApp.TransferService.Infrastructure.Messaging;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducerAccessor _producerAccessor;

    public KafkaProducer(IProducerAccessor producerAccessor)
    {
        _producerAccessor = producerAccessor;
    }

    public async Task PublishAsync<T>(string topic, T message) where T : class
    {
        var producer = _producerAccessor.GetProducer("transfer-producer");
        await producer.ProduceAsync(topic, Guid.NewGuid().ToString(), message);
    }
}