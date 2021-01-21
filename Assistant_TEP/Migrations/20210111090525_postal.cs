using Microsoft.EntityFrameworkCore.Migrations;

namespace Assistant_TEP.Migrations
{
    public partial class postal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Postal",
                table: "Organization",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostalCode",
                table: "Abonents",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Postal",
                table: "Organization");

            migrationBuilder.AlterColumn<int>(
                name: "PostalCode",
                table: "Abonents",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
