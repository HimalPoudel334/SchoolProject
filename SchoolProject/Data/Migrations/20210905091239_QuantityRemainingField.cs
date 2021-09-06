using Microsoft.EntityFrameworkCore.Migrations;

namespace SchoolProject.Data.Migrations
{
    public partial class QuantityRemainingField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantityRemaining",
                table: "Donations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityRemaining",
                table: "Donations");
        }
    }
}
