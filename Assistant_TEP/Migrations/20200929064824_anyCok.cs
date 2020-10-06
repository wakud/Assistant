using Microsoft.EntityFrameworkCore.Migrations;

namespace Assistant_TEP.Migrations
{
    public partial class anyCok : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AnyCok",
                table: "User",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnyCok",
                table: "User");
        }
    }
}
