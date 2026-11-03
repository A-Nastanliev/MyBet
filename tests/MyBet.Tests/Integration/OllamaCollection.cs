namespace MyBet.Tests.Integration
{
    [CollectionDefinition(Name)]
    public sealed class OllamaCollection : ICollectionFixture<OllamaFixture>
    {
        public const string Name = "Ollama";
    }
}