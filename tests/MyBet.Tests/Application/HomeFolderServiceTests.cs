using MyBet.Application.HomeFolder;
using MyBet.Tests.Fakes;
using MyBet.Tests.Support;

namespace MyBet.Tests.Application
{
    public class HomeFolderServiceTests
    {
        private readonly FakeSettingsStore _settings = new();
        private readonly FakeDatabaseManager _database = new();

        private HomeFolderService Create() => new(_settings, _database);

        [Fact]
        public async Task First_folder_is_saved_without_wiping()
        {
            using var dir = new TempDirectory();

            var result = await Create().SetAsync(dir.FullPath);

            Assert.Equal(HomeFolderOutcome.Set, result.Outcome);
            Assert.Equal(dir.FullPath, _settings.Folder);
            Assert.Equal(0, _database.ResetCount);
        }

        [Fact]
        public async Task Same_folder_does_nothing_even_with_a_trailing_separator()
        {
            using var dir = new TempDirectory();
            _settings.Folder = dir.FullPath;

            var result = await Create().SetAsync(dir.FullPath + Path.DirectorySeparatorChar);

            Assert.Equal(HomeFolderOutcome.Unchanged, result.Outcome);
            Assert.Equal(0, _database.ResetCount);
        }

        [Fact]
        public async Task Different_folder_asks_for_confirmation_and_changes_nothing()
        {
            using var first = new TempDirectory();
            using var second = new TempDirectory();
            _settings.Folder = first.FullPath;

            var result = await Create().SetAsync(second.FullPath);

            Assert.Equal(HomeFolderOutcome.ConfirmationRequired, result.Outcome);
            Assert.Equal(first.FullPath, _settings.Folder);
            Assert.Equal(0, _database.ResetCount);
        }

        [Fact]
        public async Task Different_folder_wipes_the_database_once_confirmed()
        {
            using var first = new TempDirectory();
            using var second = new TempDirectory();
            _settings.Folder = first.FullPath;

            var result = await Create().SetAsync(second.FullPath, wipeConfirmed: true);

            Assert.Equal(HomeFolderOutcome.Wiped, result.Outcome);
            Assert.Equal(second.FullPath, _settings.Folder);
            Assert.Equal(1, _database.ResetCount);
        }

        [Fact]
        public async Task Missing_folder_changes_nothing()
        {
            using var dir = new TempDirectory();
            _settings.Folder = dir.FullPath;
            var missing = Path.Combine(dir.FullPath, "does-not-exist");

            var result = await Create().SetAsync(missing, wipeConfirmed: true);

            Assert.Equal(HomeFolderOutcome.NotFound, result.Outcome);
            Assert.Equal(dir.FullPath, _settings.Folder);
            Assert.Equal(0, _database.ResetCount);
        }
    }
}