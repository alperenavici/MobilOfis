using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobilOfis.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HRApprovalDate",
                table: "Leaves",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HRApprovalId",
                table: "Leaves",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManagerApprovalDate",
                table: "Leaves",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HRApprovalDate",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "HRApprovalId",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "ManagerApprovalDate",
                table: "Leaves");
        }
    }
}
