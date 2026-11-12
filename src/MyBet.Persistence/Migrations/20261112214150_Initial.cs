using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBet.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewQueue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    SlipId = table.Column<string>(type: "TEXT", nullable: false),
                    Stake = table.Column<decimal>(type: "TEXT", nullable: true),
                    TotalOdds = table.Column<decimal>(type: "TEXT", nullable: true),
                    Payout = table.Column<decimal>(type: "TEXT", nullable: true),
                    BonusPercent = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScannedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ScannedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Outcome = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScannedFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Slips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SlipId = table.Column<string>(type: "TEXT", nullable: false, collation: "BINARY"),
                    Stake = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalOdds = table.Column<decimal>(type: "TEXT", nullable: false),
                    Payout = table.Column<decimal>(type: "TEXT", nullable: false),
                    BonusPercent = table.Column<decimal>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slips", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewQueue_FileHash",
                table: "ReviewQueue",
                column: "FileHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScannedFiles_Hash",
                table: "ScannedFiles",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Slips_SlipId",
                table: "Slips",
                column: "SlipId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewQueue");

            migrationBuilder.DropTable(
                name: "ScannedFiles");

            migrationBuilder.DropTable(
                name: "Slips");
        }
    }
}
