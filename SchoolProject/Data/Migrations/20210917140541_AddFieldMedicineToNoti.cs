using Microsoft.EntityFrameworkCore.Migrations;

namespace SchoolProject.Data.Migrations
{
    public partial class AddFieldMedicineToNoti : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MedicineId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_MedicineId",
                table: "Notifications",
                column: "MedicineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Medicines_MedicineId",
                table: "Notifications",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Medicines_MedicineId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_MedicineId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "MedicineId",
                table: "Notifications");
        }
    }
}
