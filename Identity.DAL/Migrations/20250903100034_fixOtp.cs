using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.DAL.Migrations
{
    /// <inheritdoc />
    public partial class fixOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "oTPCodes");

            migrationBuilder.AddColumn<int>(
                name: "otpPurpose",
                table: "oTPCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "otpPurpose",
                table: "oTPCodes");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "oTPCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
