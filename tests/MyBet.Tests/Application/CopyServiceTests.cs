using MyBet.Application.Copy;
using MyBet.Tests.Fakes;
using MyBet.Tests.Support;

namespace MyBet.Tests.Application
{
    public class CopyServiceTests : IDisposable
    {
        private readonly TempDirectory _home = new();
        private readonly TempDirectory _source = new();
        private readonly CopyService _service;

        public CopyServiceTests()
        {
            _service = new CopyService(new FakeSettingsStore { Folder = _home.FullPath });
        }

        public void Dispose()
        {
            _home.Dispose();
            _source.Dispose();
        }

        private string InHome(string name) => Path.Combine(_home.FullPath, name);

        [Fact]
        public async Task One_image_is_copied_into_the_home_folder()
        {
            var file = _source.AddFile("a.png", new byte[] { 1 });

            var result = await _service.CopyAsync(file);

            Assert.Equal(CopyStatus.Done, result.Status);
            Assert.Equal(CopyOutcome.Copied, Assert.Single(result.Files).Outcome);
            Assert.Equal(new byte[] { 1 }, await File.ReadAllBytesAsync(InHome("a.png")));
            Assert.True(File.Exists(file));
        }

        [Fact]
        public async Task Every_image_in_a_folder_is_copied_and_other_files_are_ignored()
        {
            _source.AddFile("a.png", new byte[] { 1 });
            _source.AddFile("b.jpg", new byte[] { 2 });
            _source.AddFile("c.jpeg", new byte[] { 3 });
            _source.AddFile("d.webp", new byte[] { 4 });
            _source.AddFile("notes.txt", new byte[] { 5 });

            var result = await _service.CopyAsync(_source.FullPath);

            Assert.Equal(4, result.CountOf(CopyOutcome.Copied));
            Assert.False(File.Exists(InHome("notes.txt")));
        }

        [Fact]
        public async Task Subfolders_are_not_searched()
        {
            _source.AddFile("a.png", new byte[] { 1 });
            var sub = Path.Combine(_source.FullPath, "sub");
            Directory.CreateDirectory(sub);
            File.WriteAllBytes(Path.Combine(sub, "deep.png"), new byte[] { 2 });

            var result = await _service.CopyAsync(_source.FullPath);

            Assert.Equal("a.png", Path.GetFileName(Assert.Single(result.Files).Source));
            Assert.False(File.Exists(InHome("deep.png")));
        }

        [Fact]
        public async Task Folder_path_with_a_trailing_separator_works()
        {
            _source.AddFile("a.png", new byte[] { 1 });

            var result = await _service.CopyAsync(_source.FullPath + Path.DirectorySeparatorChar);

            Assert.Equal(CopyOutcome.Copied, Assert.Single(result.Files).Outcome);
        }

        [Fact]
        public async Task Extension_is_matched_without_regard_to_case()
        {
            _source.AddFile("A.PNG", new byte[] { 1 });

            var result = await _service.CopyAsync(_source.FullPath);

            Assert.Equal(CopyOutcome.Copied, Assert.Single(result.Files).Outcome);
        }

        [Fact]
        public async Task File_with_content_already_in_the_home_folder_is_skipped_even_under_another_name()
        {
            _home.AddFile("old.png", new byte[] { 1 });
            _source.AddFile("new-name.png", new byte[] { 1 });

            var result = await _service.CopyAsync(_source.FullPath);

            Assert.Equal(CopyOutcome.SkippedSameContent, Assert.Single(result.Files).Outcome);
            Assert.False(File.Exists(InHome("new-name.png")));
        }

        [Fact]
        public async Task Same_name_with_different_content_is_renamed_and_the_original_is_kept()
        {
            _home.AddFile("a.png", new byte[] { 1 });
            _source.AddFile("a.png", new byte[] { 2 });

            var result = await _service.CopyAsync(_source.FullPath);

            var file = Assert.Single(result.Files);
            Assert.Equal(CopyOutcome.Renamed, file.Outcome);
            Assert.Equal(InHome("a (2).png"), file.Destination);
            Assert.Equal(new byte[] { 1 }, await File.ReadAllBytesAsync(InHome("a.png")));
            Assert.Equal(new byte[] { 2 }, await File.ReadAllBytesAsync(InHome("a (2).png")));
        }

        [Fact]
        public async Task Rename_goes_to_the_next_free_number()
        {
            _home.AddFile("a.png", new byte[] { 1 });
            _home.AddFile("a (2).png", new byte[] { 2 });
            _source.AddFile("a.png", new byte[] { 3 });

            var result = await _service.CopyAsync(_source.FullPath);

            Assert.Equal(InHome("a (3).png"), Assert.Single(result.Files).Destination);
        }

        [Fact]
        public async Task Identical_files_in_the_source_are_copied_once()
        {
            _source.AddFile("a.png", new byte[] { 1 });
            _source.AddFile("b.png", new byte[] { 1 });

            var result = await _service.CopyAsync(_source.FullPath);

            Assert.Equal(1, result.CountOf(CopyOutcome.Copied));
            Assert.Equal(1, result.CountOf(CopyOutcome.SkippedSameContent));
        }

        [Fact]
        public async Task Running_it_twice_copies_nothing_the_second_time()
        {
            _source.AddFile("a.png", new byte[] { 1 });
            _source.AddFile("b.png", new byte[] { 2 });
            await _service.CopyAsync(_source.FullPath);

            var result = await _service.CopyAsync(_source.FullPath);

            Assert.Equal(2, result.CountOf(CopyOutcome.SkippedSameContent));
            Assert.Equal(0, result.CountOf(CopyOutcome.Copied));
        }

        [Fact]
        public async Task Folder_without_images_is_done_with_no_files()
        {
            _source.AddFile("notes.txt");

            var result = await _service.CopyAsync(_source.FullPath);

            Assert.Equal(CopyStatus.Done, result.Status);
            Assert.Empty(result.Files);
        }

        [Fact]
        public async Task A_file_that_is_not_an_image_is_rejected()
        {
            var file = _source.AddFile("notes.txt");

            var result = await _service.CopyAsync(file);

            Assert.Equal(CopyStatus.NotAnImage, result.Status);
            Assert.Empty(Directory.GetFiles(_home.FullPath));
        }

        [Fact]
        public async Task Missing_source_is_reported()
        {
            var result = await _service.CopyAsync(Path.Combine(_source.FullPath, "nope.png"));

            Assert.Equal(CopyStatus.SourceMissing, result.Status);
        }

        [Fact]
        public async Task No_home_folder_set_is_reported_and_nothing_is_copied()
        {
            var service = new CopyService(new FakeSettingsStore());
            var file = _source.AddFile("a.png");

            var result = await service.CopyAsync(file);

            Assert.Equal(CopyStatus.NoHomeFolder, result.Status);
            Assert.Empty(Directory.GetFiles(_home.FullPath));
        }

        [Fact]
        public async Task Home_folder_that_no_longer_exists_is_reported()
        {
            var gone = Path.Combine(_home.FullPath, "gone");
            var service = new CopyService(new FakeSettingsStore { Folder = gone });
            var file = _source.AddFile("a.png");

            var result = await service.CopyAsync(file);

            Assert.Equal(CopyStatus.HomeFolderMissing, result.Status);
            Assert.Equal(gone, result.Location);
        }
    }
}