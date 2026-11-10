namespace MyBet.Tests.Fakes
{
    public class RecordingProgress<T> : IProgress<T>
    {
        public List<T> Reports { get; } = new();

        public void Report(T value) => Reports.Add(value);
    }
}