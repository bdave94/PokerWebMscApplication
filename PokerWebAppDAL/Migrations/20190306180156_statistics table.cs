using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerWebAppDAL.Migrations
{
    public partial class statisticstable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    StatisticsID = table.Column<Guid>(nullable: false),
                    TotalGamesPlayed = table.Column<int>(nullable: false),
                    TotalWins = table.Column<int>(nullable: false),
                    TotalMoneyEarned = table.Column<int>(nullable: false),
                    BestWeeklyRank = table.Column<int>(nullable: false),
                    BestWeeklyRankDate = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.StatisticsID);
                    table.ForeignKey(
                        name: "FK_Statistics_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_UserId",
                table: "Statistics",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistics");
        }
    }
}
