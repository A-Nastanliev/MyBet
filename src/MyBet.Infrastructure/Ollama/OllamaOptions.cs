namespace MyBet.Infrastructure.Ollama
{
    public class OllamaOptions
    {
        public const string SectionName = "Ollama";

        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string Model { get; set; } = "qwen2.5vl:7b";
        public int NumCtx { get; set; } = 4096;
        public int TimeoutMinutes { get; set; } = 5;
        public bool ManageLifecycle { get; set; } = true;
        public string? ExecutablePath { get; set; }

        public int StartupTimeoutSeconds { get; set; } = 30;
        public string KeepAlive { get; set; } = "30m";
    }
}
