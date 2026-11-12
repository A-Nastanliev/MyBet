namespace MyBet.Application.Import
{
    public static class ImageExtensions
    {
        private static readonly string[] Supported = { ".png", ".jpg", ".jpeg", ".webp" };

        public static bool IsImage(string path) =>
            Supported.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    }
}