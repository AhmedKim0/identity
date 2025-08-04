using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.DAL.Migrations
{
    /// <inheritdoc />
    public partial class OTPv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oTPCodes_AspNetUsers_UserId",
                table: "oTPCodes");

            migrationBuilder.DropColumn(
                name: "AttemptsCount",
                table: "oTPCodes");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "oTPCodes");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "oTPCodes");

            migrationBuilder.DropColumn(
                name: "RequestIp",
                table: "oTPCodes");

            migrationBuilder.DropColumn(
                name: "SentTo",
                table: "oTPCodes");

            migrationBuilder.DropColumn(
                name: "UsedAtUtc",
                table: "oTPCodes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "oTPCodes",
                newName: "EmailVerificationId");

            migrationBuilder.RenameColumn(
                name: "IsUsed",
                table: "oTPCodes",
                newName: "IsExpired");

            migrationBuilder.RenameColumn(
                name: "ExpiresAtUtc",
                table: "oTPCodes",
                newName: "ExpireAt");

            migrationBuilder.RenameIndex(
                name: "IX_oTPCodes_UserId",
                table: "oTPCodes",
                newName: "IX_oTPCodes_EmailVerificationId");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "oTPCodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "oTPCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "emailVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    BlockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emailVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "oTPTries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTPCodeId = table.Column<int>(type: "int", nullable: false),
                    TryAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oTPTries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_oTPTries_oTPCodes_OTPCodeId",
                        column: x => x.OTPCodeId,
                        principalTable: "oTPCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_oTPTries_OTPCodeId",
                table: "oTPTries",
                column: "OTPCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_oTPCodes_emailVerifications_EmailVerificationId",
                table: "oTPCodes",
                column: "EmailVerificationId",
                principalTable: "emailVerifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oTPCodes_emailVerifications_EmailVerificationId",
                table: "oTPCodes");

            migrationBuilder.DropTable(
                name: "emailVerifications");

            migrationBuilder.DropTable(
                name: "oTPTries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "oTPCodes");

            migrationBuilder.RenameColumn(
                name: "IsExpired",
                table: "oTPCodes",
                newName: "IsUsed");

            migrationBuilder.RenameColumn(
                name: "ExpireAt",
                table: "oTPCodes",
                newName: "ExpiresAtUtc");

            migrationBuilder.RenameColumn(
                name: "EmailVerificationId",
                table: "oTPCodes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_oTPCodes_EmailVerificationId",
                table: "oTPCodes",
                newName: "IX_oTPCodes_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "oTPCodes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "AttemptsCount",
                table: "oTPCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "oTPCodes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "oTPCodes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequestIp",
                table: "oTPCodes",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SentTo",
                table: "oTPCodes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAtUtc",
                table: "oTPCodes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_oTPCodes_AspNetUsers_UserId",
                table: "oTPCodes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
