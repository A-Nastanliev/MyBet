using System.Text.Json;
using MyBet.Application.Abstractions;

namespace MyBet.Infrastructure.Settings
{
    public class JsonSettingsStore : ISettingsStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private readonly string _path;

        public JsonSettingsStore(string path) => _path = path;

        public async Task<string?> GetFolderAsync(CancellationToken ct = default) =>
            (await LoadAsync(ct)).Folder;

        public async Task SetFolderAsync(string folder, CancellationToken ct = default)
        {
            var settings = await LoadAsync(ct);
            settings.Folder = folder;
            await SaveAsync(settings, ct);
        }

        private async Task<UserSettings> LoadAsync(CancellationToken ct)
        {
            if (!File.Exists(_path)) return new UserSettings();

            try
            {
                await using var stream = File.OpenRead(_path);
                return await JsonSerializer.DeserializeAsync<UserSettings>(stream, JsonOptions, ct)
                       ?? new UserSettings();
            }
            catch (JsonException)
            {
                return new UserSettings();
            }
        }

        private async Task SaveAsync(UserSettings settings, CancellationToken ct)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

            var temp = _path + ".tmp";
            await using (var stream = File.Create(temp))
                await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, ct);

            File.Move(temp, _path, overwrite: true);
        }
    }
}