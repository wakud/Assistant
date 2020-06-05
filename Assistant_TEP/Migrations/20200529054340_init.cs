using Microsoft.EntityFrameworkCore.Migrations;

namespace Assistant_TEP.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DbTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Code = table.Column<string>(maxLength: 4, nullable: true),
                    Nach = table.Column<string>(maxLength: 50, nullable: true),
                    Address = table.Column<string>(maxLength: 100, nullable: true),
                    Buh = table.Column<string>(maxLength: 50, nullable: true),
                    Tel = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportParamTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    TypeC = table.Column<string>(nullable: true),
                    TypeHtml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportParamTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypeReports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(maxLength: 50, nullable: true),
                    Login = table.Column<string>(maxLength: 10, nullable: false),
                    Password = table.Column<string>(nullable: false),
                    CokId = table.Column<int>(nullable: true),
                    IsAdmin = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Organization_CokId",
                        column: x => x.CokId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    FileScript = table.Column<string>(nullable: true),
                    DbTypeId = table.Column<int>(nullable: false),
                    TypeReportId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_DbTypes_DbTypeId",
                        column: x => x.DbTypeId,
                        principalTable: "DbTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_TypeReports_TypeReportId",
                        column: x => x.TypeReportId,
                        principalTable: "TypeReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportParams",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ReportId = table.Column<int>(nullable: false),
                    ParamTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportParams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportParams_ReportParamTypes_ParamTypeId",
                        column: x => x.ParamTypeId,
                        principalTable: "ReportParamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportParams_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportParams_ParamTypeId",
                table: "ReportParams",
                column: "ParamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportParams_ReportId",
                table: "ReportParams",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_DbTypeId",
                table: "Reports",
                column: "DbTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TypeReportId",
                table: "Reports",
                column: "TypeReportId");

            migrationBuilder.CreateIndex(
                name: "IX_User_CokId",
                table: "User",
                column: "CokId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportParams");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ReportParamTypes");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DropTable(
                name: "DbTypes");

            migrationBuilder.DropTable(
                name: "TypeReports");
        }
    }
}
