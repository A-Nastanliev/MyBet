using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;

namespace MyBet.Tests.Fakes
{
    public class FakeSlipExtractor : ISlipExtractor
    {
        private readonly Queue<Func<ExtractedSlip?>> _responses = new();

        public int CallCount { get; private set; }

        public void Enqueue(ExtractedSlip? slip) => _responses.Enqueue(() => slip);

        public void EnqueueFailure(Exception exception) => _responses.Enqueue(() => throw exception);

        public Task<ExtractedSlip?> ExtractAsync(byte[] imageBytes, CancellationToken ct = default)
        {
            CallCount++;
            return Task.FromResult(_responses.Dequeue()());
        }
    }
}
