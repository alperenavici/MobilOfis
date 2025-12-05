using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobilOfis.Data.Migrations
{
    /// <inheritdoc />
    public partial class eventType3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerImageUrl",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerImageUrl",
                table: "Events",
                type: "text",
                nullable: true);
        }
    }
}
