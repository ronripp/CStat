using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CStat.Migrations
{
    public partial class NewTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position_Tile2",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Position_Title1",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Position_Title3",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Task_Status",
                table: "Task");

            migrationBuilder.RenameColumn(
                name: "Completion_Date",
                table: "Task",
                newName: "Actual_Done_Date");

            migrationBuilder.AddColumn<string>(
                name: "link",
                table: "Transaction",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Committed_Cost",
                table: "Task",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Committed_Man_Hours",
                table: "Task",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Estimated_Done_Date",
                table: "Task",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Estimated_Man_Hours",
                table: "Task",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Roles",
                table: "Task",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Task",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Total_Cost",
                table: "Task",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Worker1_id",
                table: "Task",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Worker2_id",
                table: "Task",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Worker3_id",
                table: "Task",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "InventoryItem",
                type: "datetime2(0)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order_Id",
                table: "InventoryItem",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Person_Id",
                table: "InventoryItem",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "InventoryItem",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Units_per_day",
                table: "InventoryItem",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Zone",
                table: "InventoryItem",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Task_Worker1_id",
                table: "Task",
                column: "Worker1_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Worker2_id",
                table: "Task",
                column: "Worker2_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Worker3_id",
                table: "Task",
                column: "Worker3_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_Order_Id",
                table: "InventoryItem",
                column: "Order_Id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_Person_Id",
                table: "InventoryItem",
                column: "Person_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItem_Transaction",
                table: "InventoryItem",
                column: "Order_Id",
                principalTable: "Transaction",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItem_Person",
                table: "InventoryItem",
                column: "Person_Id",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Person1",
                table: "Task",
                column: "Worker1_id",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Person2",
                table: "Task",
                column: "Worker2_id",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Person3",
                table: "Task",
                column: "Worker3_id",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItem_Transaction",
                table: "InventoryItem");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItem_Person",
                table: "InventoryItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Person1",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Person2",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Person3",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_Worker1_id",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_Worker2_id",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_Worker3_id",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItem_Order_Id",
                table: "InventoryItem");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItem_Person_Id",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "link",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Committed_Cost",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Committed_Man_Hours",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Estimated_Done_Date",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Estimated_Man_Hours",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Roles",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Total_Cost",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Worker1_id",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Worker2_id",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Worker3_id",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "Order_Id",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "Person_Id",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "State",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "Units_per_day",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "Zone",
                table: "InventoryItem");

            migrationBuilder.RenameColumn(
                name: "Actual_Done_Date",
                table: "Task",
                newName: "Completion_Date");

            migrationBuilder.AddColumn<int>(
                name: "Position_Tile2",
                table: "Task",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Position_Title1",
                table: "Task",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Position_Title3",
                table: "Task",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Task_Status",
                table: "Task",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
