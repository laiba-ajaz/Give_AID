using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Give_AID.Migrations
{
    /// <inheritdoc />
    public partial class AddNGOAndProgramme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NGOs",
                columns: table => new
                {
                    NGOID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NGOName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NGOs", x => x.NGOID);
                });

            migrationBuilder.CreateTable(
                name: "Programmes",
                columns: table => new
                {
                    ProgrammeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    NGOID = table.Column<int>(type: "int", nullable: false),
                    DonationCausesID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programmes", x => x.ProgrammeId);
                    table.ForeignKey(
                        name: "FK_Programmes_DonationCauses_DonationCausesID",
                        column: x => x.DonationCausesID,
                        principalTable: "DonationCauses",
                        principalColumn: "DonationCausesID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Programmes_NGOs_NGOID",
                        column: x => x.NGOID,
                        principalTable: "NGOs",
                        principalColumn: "NGOID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Programmes_DonationCausesID",
                table: "Programmes",
                column: "DonationCausesID");

            migrationBuilder.CreateIndex(
                name: "IX_Programmes_NGOID",
                table: "Programmes",
                column: "NGOID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Programmes");

            migrationBuilder.DropTable(
                name: "NGOs");
        }
    }
}
