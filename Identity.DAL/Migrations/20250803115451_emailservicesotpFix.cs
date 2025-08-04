using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.DAL.Migrations
{
    /// <inheritdoc />
    public partial class emailservicesotpFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oTPCodes_emailVerifications_EmailVerificationId",
                table: "oTPCodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_emailVerifications",
                table: "emailVerifications");

            migrationBuilder.RenameTable(
                name: "emailVerifications",
                newName: "EmailVerification");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUTC",
                table: "oTPCodes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "oTPCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailVerification",
                table: "EmailVerification",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "emailBodies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emailBodies", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_oTPCodes_EmailVerification_EmailVerificationId",
                table: "oTPCodes",
                column: "EmailVerificationId",
                principalTable: "EmailVerification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oTPCodes_EmailVerification_EmailVerificationId",
                table: "oTPCodes");

            migrationBuilder.DropTable(
                name: "emailBodies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailVerification",
                table: "EmailVerification");

            migrationBuilder.DropColumn(
                name: "CreatedAtUTC",
                table: "oTPCodes");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "oTPCodes");

            migrationBuilder.RenameTable(
                name: "EmailVerification",
                newName: "emailVerifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_emailVerifications",
                table: "emailVerifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_oTPCodes_emailVerifications_EmailVerificationId",
                table: "oTPCodes",
                column: "EmailVerificationId",
                principalTable: "emailVerifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
