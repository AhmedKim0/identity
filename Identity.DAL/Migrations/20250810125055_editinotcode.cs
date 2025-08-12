using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.DAL.Migrations
{
    /// <inheritdoc />
    public partial class editinotcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "oTPCodes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "oTPCodes");
        }
    }
}
