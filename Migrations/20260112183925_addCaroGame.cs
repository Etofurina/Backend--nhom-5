using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI2.Migrations
{
    /// <inheritdoc />
    public partial class addCaroGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaroGames",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerX = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlayerO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlayerXType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlayerOType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlayerXLevel = table.Column<int>(type: "int", nullable: true),
                    PlayerOLevel = table.Column<int>(type: "int", nullable: true),
                    Board = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Winner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaroGames", x => x.GameId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaroGames");
        }
    }
}
