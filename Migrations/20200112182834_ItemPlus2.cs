using Microsoft.EntityFrameworkCore.Migrations;

namespace CStat.Migrations
{
    public partial class ItemPlus2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Current_Stock",
                table: "InventoryItem",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Reorder_Threshold",
                table: "InventoryItem",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Units",
                table: "InventoryItem",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Current_Stock",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "Reorder_Threshold",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "Units",
                table: "InventoryItem");
        }
    }
}
