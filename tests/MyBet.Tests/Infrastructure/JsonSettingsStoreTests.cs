using MyBet.Infrastructure.Settings;
using MyBet.Tests.Support;

namespace MyBet.Tests.Infrastructure
{
    public class JsonSettingsStoreTests
    {
        [Fact]
        public async Task Missing_file_means_no_folder()
        {
            using var dir = new TempDirectory();
            var store = new JsonSettingsStore(Path.Combine(dir.FullPath, "settings.json"));

            Assert.Null(await store.GetFolderAsync());
        }

        [Fact]
        public async Task Folder_round_trips_and_the_directory_is_created()
        {
            using var dir = new TempDirectory();
            var path = Path.Combine(dir.FullPath, "nested", "settings.json");

            await new JsonSettingsStore(path).SetFolderAsync(@"D:\Slips");

            Assert.Equal(@"D:\Slips", await new JsonSettingsStore(path).GetFolderAsync());
        }

        [Fact]
        public async Task Corrupt_file_is_treated_as_empty()
        {
            using var dir = new TempDirectory();
            var path = dir.AddFile("settings.json", "{ not json"u8.ToArray());

            Assert.Null(await new JsonSettingsStore(path).GetFolderAsync());
        }
    }
}