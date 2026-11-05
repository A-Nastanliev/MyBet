using MyBet.Domain.Enums;
using MyBet.Infrastructure.Ollama.Dtos;
using MyBet.Infrastructure.Ollama.Mapping;

namespace MyBet.Tests.Infrastructure
{
    public class SlipMapperTests
    {
        [Fact]
        public void Nulls_become_safe_defaults()
        {
            var slip = SlipMapper.ToDomain(new SlipDto { SlipId = " 123 " });

            Assert.Equal("123", slip.SlipId);
            Assert.Equal(0m, slip.TotalOdds);
            Assert.Empty(slip.Legs);
            Assert.Null(slip.Stake);
        }

        [Fact]
        public void Missing_type_is_derived_from_leg_count()
        {
            var dto = new SlipDto
            {
                Legs = new() { new LegDto { Odds = 1.5m }, new LegDto { Odds = 2m } }
            };

            Assert.Equal("Двойни", SlipMapper.ToDomain(dto).Type);
        }

        [Fact]
        public void Given_type_is_kept()
        {
            var dto = new SlipDto { Type = "Двойни", Legs = new() { new LegDto() } };

            Assert.Equal("Двойни", SlipMapper.ToDomain(dto).Type);
        }

        [Fact]
        public void Leg_with_sub_selections_is_marked_bet_builder()
        {
            var dto = new SlipDto
            {
                Legs = new()
            {
                new LegDto
                {
                    Market = "Създай Залог",
                    SubSelections = new() { new SubSelectionDto { Market = "Голове", Selection = "Над 2.5" } }
                }
            }
            };

            var leg = Assert.Single(SlipMapper.ToDomain(dto).Legs);
            Assert.True(leg.IsBetBuilder);
            Assert.Single(leg.SubSelections);
        }

        [Fact]
        public void Single_leg_without_type_is_derived_as_single()
        {
            var dto = new SlipDto { Legs = new() { new LegDto { Odds = 1.8m } } };

            Assert.Equal("Сингъл", SlipMapper.ToDomain(dto).Type);
        }

        [Fact]
        public void Unparseable_date_becomes_null()
        {
            Assert.Null(SlipMapper.ToDomain(new SlipDto { PlacedAt = "not a date" }).PlacedAt);
        }

        [Fact]
        public void Bulgarian_placed_at_text_is_parsed()
        {
            var slip = SlipMapper.ToDomain(new SlipDto { PlacedAt = "18:19 ч. 18.10.2026 г." });

            Assert.Equal(new DateTime(2026, 10, 18, 18, 19, 0), slip.PlacedAt);
        }

        [Fact]
        public void Placed_at_with_only_a_date_keeps_the_date()
        {
            var slip = SlipMapper.ToDomain(new SlipDto { PlacedAt = "25.10.2026" });

            Assert.Equal(new DateTime(2026, 10, 25), slip.PlacedAt);
        }

        [Fact]
        public void Time_without_a_date_is_ignored()
        {
            Assert.Null(SlipMapper.ToDomain(new SlipDto { PlacedAt = "19:35" }).PlacedAt);
        }

        [Fact]
        public void Bet_builder_leg_always_has_the_bet_builder_market_and_no_selection()
        {
            var dto = new SlipDto
            {
                Legs = new()
        {
            new LegDto
            {
                Market = "Създай Залог",
                Selection = "Създай Залог",
                SubSelections = new() { new SubSelectionDto { Market = "Голове", Selection = "Над 2.5" } }
            }
        }
            };

            var leg = Assert.Single(SlipMapper.ToDomain(dto).Legs);

            Assert.True(leg.IsBetBuilder);
            Assert.Equal("Създай Залог", leg.Market);
            Assert.Equal("", leg.Selection);
        }

        [Fact]
        public void Bet_builder_is_recognised_when_the_model_only_put_it_in_selection()
        {
            var dto = new SlipDto { Legs = new() { new LegDto { Market = "", Selection = "Създай Залог" } } };

            var leg = Assert.Single(SlipMapper.ToDomain(dto).Legs);

            Assert.True(leg.IsBetBuilder);
            Assert.Equal("Създай Залог", leg.Market);
            Assert.Equal("", leg.Selection);
        }

        [Theory]
        [InlineData("", LegResult.Pending)]
        [InlineData("Печалба", LegResult.Won)]
        [InlineData("Печалба,Изплатени", LegResult.EarlyPayout)]
        [InlineData("Загуба", LegResult.Lost)]
        public void Badges_decide_the_leg_result(string badges, LegResult expected)
        {
            var leg = new LegDto { Badges = badges.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() };
            var slip = SlipMapper.ToDomain(new SlipDto { Legs = new() { leg } });

            Assert.Equal(expected, slip.Legs[0].Result);
        }

        [Fact]
        public void Kesaut_label_makes_the_slip_cashed_out()
        {
            var slip = SlipMapper.ToDomain(new SlipDto { PayoutLabel = "Кешаут", Legs = new() { new LegDto() } });

            Assert.Equal(SlipStatus.CashedOut, slip.Status);
        }

        [Fact]
        public void Lost_badge_makes_the_slip_lost()
        {
            var dto = new SlipDto
            {
                PayoutLabel = "Печалба",
                Legs = new()
                {
                    new LegDto { Badges = new() { "Печалба" } },
                    new LegDto { Badges = new() { "Загуба" } },
                    new LegDto()
                }
            };

            Assert.Equal(SlipStatus.Lost, SlipMapper.ToDomain(dto).Status);
        }

        [Fact]
        public void Vs_placeholder_is_not_a_score()
        {
            var slip = SlipMapper.ToDomain(new SlipDto { Legs = new() { new LegDto { Score = "vs." } } });

            Assert.Null(slip.Legs[0].Score);
        }
    }
}