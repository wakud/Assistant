using Microsoft.EntityFrameworkCore.Migrations;

namespace Assistant_TEP.Migrations
{
    public partial class abon_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Abonents");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Abonents",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Abonents");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Abonents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
