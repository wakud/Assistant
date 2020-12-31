using Microsoft.EntityFrameworkCore.Migrations;

namespace Assistant_TEP.Migrations
{
    public partial class abon_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Apartment",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeCok",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "House",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Housing",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Oblast",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rajon",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondName",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypVul",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfCityAbbr",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfCityFull",
                table: "Abonents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeStreet",
                table: "Abonents",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apartment",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "CodeCok",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "House",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "Housing",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "Oblast",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "Rajon",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "SecondName",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "TypVul",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "TypeOfCityAbbr",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "TypeOfCityFull",
                table: "Abonents");

            migrationBuilder.DropColumn(
                name: "TypeStreet",
                table: "Abonents");
        }
    }
}
