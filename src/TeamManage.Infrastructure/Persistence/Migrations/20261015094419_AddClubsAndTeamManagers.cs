using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamManage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClubsAndTeamManagers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Roles changed (Manager/Player -> ClubManager/TeamManager/Player) and teams now
            // require a club, so existing data is reset.
            migrationBuilder.Sql("DELETE FROM MatchPlayers");
            migrationBuilder.Sql("DELETE FROM Matches");
            migrationBuilder.Sql("DELETE FROM TeamPlayers");
            migrationBuilder.Sql("DELETE FROM Positions");
            migrationBuilder.Sql("DELETE FROM Teams");
            migrationBuilder.Sql("DELETE FROM RefreshTokens");
            migrationBuilder.Sql("DELETE FROM Users");
            migrationBuilder.AlterColumn<Guid>(
                name: "ManagerId",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ClubId",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Clubs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClubManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clubs_Users_ClubManagerId",
                        column: x => x.ClubManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ClubId",
                table: "Teams",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Clubs_ClubManagerId",
                table: "Clubs",
                column: "ClubManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Clubs_ClubId",
                table: "Teams",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Clubs_ClubId",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "Clubs");

            migrationBuilder.DropIndex(
                name: "IX_Teams_ClubId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ClubId",
                table: "Teams");

            migrationBuilder.AlterColumn<Guid>(
                name: "ManagerId",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
