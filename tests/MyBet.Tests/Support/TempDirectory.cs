namespace MyBet.Tests.Support
{
    public sealed class TempDirectory : IDisposable
    {
        public string FullPath { get; } =
            Path.Combine(Path.GetTempPath(), "mybet-tests-" + Guid.NewGuid().ToString("N"));

        public TempDirectory() => Directory.CreateDirectory(FullPath);

        public string AddFile(string name, byte[]? content = null)
        {
            var path = Path.Combine(FullPath, name);
            File.WriteAllBytes(path, content ?? new byte[] { 1, 2, 3 });
            return path;
        }

        public void Dispose() => Directory.Delete(FullPath, recursive: true);
    }
}
