using Microsoft.EntityFrameworkCore.Migrations;

namespace Assistant_TEP.Migrations
{
    public partial class paramsource : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParamSource",
                table: "ReportParams",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParamSource",
                table: "ReportParams");
        }
    }
}
