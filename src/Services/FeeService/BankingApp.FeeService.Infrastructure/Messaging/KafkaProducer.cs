using BankingApp.FeeService.Application.Interfaces;
using KafkaFlow.Producers;

namespace BankingApp.FeeService.Infrastructure.Messaging;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducerAccessor _producerAccessor;

    public KafkaProducer(IProducerAccessor producerAccessor)
    {
        _producerAccessor = producerAccessor;
    }

    public async Task PublishAsync<T>(string topic, T message) where T : class
    {
        var producer = _producerAccessor.GetProducer("fee-producer");
        await producer.ProduceAsync(topic, Guid.NewGuid().ToString(), message);
    }
}