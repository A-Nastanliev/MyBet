using Microsoft.EntityFrameworkCore;
using MyBet.Domain.Entities;
using MyBet.Domain.Enums;
using MyBet.Persistence;
using MyBet.Persistence.Repositories;
using MyBet.Tests.Support;

namespace MyBet.Tests.Persistence
{
    public class SlipRepositoryTests : IDisposable
    {
        private readonly SqliteTestDatabase _db = new();
        private readonly MyBetDbContext _context;
        private readonly SlipRepository _repository;
        private readonly UnitOfWork _unitOfWork;

        public SlipRepositoryTests()
        {
            _context = _db.NewContext();
            _repository = new SlipRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _db.Dispose();
        }

        private async Task<IReadOnlyList<Slip>> ReadAllAsync()
        {
            using var other = _db.NewContext();
            return await new SlipRepository(other).GetAllAsync();
        }

        [Fact]
        public async Task Slip_is_stored_only_when_changes_are_saved()
        {
            _repository.Add(TestSlips.Saved("1001"));
            Assert.Empty(await ReadAllAsync());

            await _unitOfWork.SaveChangesAsync();

            Assert.Single(await ReadAllAsync());
        }

        [Fact]
        public async Task All_fields_survive_a_round_trip()
        {
            _repository.Add(new Slip
            {
                SlipId = "FbOAhzuOw148hoXv1eE8xQ==",
                Stake = 2.31m,
                TotalOdds = 14.16m,
                Payout = 35.75m,
                BonusPercent = 10m,
                Status = SlipStatus.CashedOut
            });
            await _unitOfWork.SaveChangesAsync();

            var saved = Assert.Single(await ReadAllAsync());

            Assert.True(saved.Id > 0);
            Assert.Equal("FbOAhzuOw148hoXv1eE8xQ==", saved.SlipId);
            Assert.Equal(2.31m, saved.Stake);
            Assert.Equal(14.16m, saved.TotalOdds);
            Assert.Equal(35.75m, saved.Payout);
            Assert.Equal(10m, saved.BonusPercent);
            Assert.Equal(SlipStatus.CashedOut, saved.Status);
        }

        [Fact]
        public async Task Missing_bonus_stays_null()
        {
            var slip = TestSlips.Saved("1001");
            slip.BonusPercent = null;
            _repository.Add(slip);
            await _unitOfWork.SaveChangesAsync();

            Assert.Null(Assert.Single(await ReadAllAsync()).BonusPercent);
        }

        [Fact]
        public async Task Status_is_stored_as_text()
        {
            var slip = TestSlips.Saved("1001");
            slip.Status = SlipStatus.CashedOut;
            _repository.Add(slip);
            await _unitOfWork.SaveChangesAsync();

            using var other = _db.NewContext();
            var raw = await other.Database
                .SqlQueryRaw<string>("SELECT \"Status\" AS \"Value\" FROM \"Slips\"")
                .ToListAsync();

            Assert.Equal("CashedOut", Assert.Single(raw));
        }

        [Fact]
        public async Task Slip_reference_is_compared_exactly_including_case()
        {
            _repository.Add(TestSlips.Saved("AbC=="));
            await _unitOfWork.SaveChangesAsync();

            Assert.True(await _repository.ExistsAsync("AbC=="));
            Assert.False(await _repository.ExistsAsync("abc=="));
            Assert.False(await _repository.ExistsAsync("ABC=="));
        }

        [Fact]
        public async Task References_that_differ_only_in_case_are_two_slips()
        {
            _repository.Add(TestSlips.Saved("AbC=="));
            _repository.Add(TestSlips.Saved("abc=="));
            await _unitOfWork.SaveChangesAsync();

            Assert.Equal(2, (await ReadAllAsync()).Count);
        }

        [Fact]
        public async Task Same_reference_twice_is_rejected_by_the_database()
        {
            _repository.Add(TestSlips.Saved("1001"));
            await _unitOfWork.SaveChangesAsync();

            _repository.Add(TestSlips.Saved("1001"));

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => _unitOfWork.SaveChangesAsync());
        }

        [Fact]
        public async Task Slips_are_returned_in_the_order_they_were_saved()
        {
            _repository.Add(TestSlips.Saved("1003"));
            _repository.Add(TestSlips.Saved("1001"));
            _repository.Add(TestSlips.Saved("1002"));
            await _unitOfWork.SaveChangesAsync();

            var ids = (await ReadAllAsync()).Select(s => s.SlipId);

            Assert.Equal(new[] { "1003", "1001", "1002" }, ids);
        }
    }
}
