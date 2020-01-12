using Microsoft.EntityFrameworkCore.Migrations;

namespace CStat.Migrations
{
    public partial class ItemsPlus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Person",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_Person_id",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Person_id",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Source_Check_Num",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Status_Type",
                table: "Business");

            migrationBuilder.RenameColumn(
                name: "Status Details",
                table: "Business",
                newName: "Status_Details");

            migrationBuilder.AlterColumn<int>(
                name: "CCA_Account_id",
                table: "Transaction",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CCA_Person_id",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_id",
                table: "Transaction",
                fixedLength: true,
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_number",
                table: "Transaction",
                fixedLength: true,
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_type",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Roles",
                table: "Position",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Roles",
                table: "Person",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "API_Link",
                table: "Business",
                fixedLength: true,
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Business",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User_Link",
                table: "Business",
                fixedLength: true,
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(fixedLength: true, maxLength: 30, nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    name = table.Column<string>(fixedLength: true, maxLength: 30, nullable: false),
                    UPC = table.Column<string>(fixedLength: true, maxLength: 12, nullable: true),
                    Mfg_id = table.Column<int>(nullable: true),
                    Size = table.Column<float>(nullable: false),
                    Units = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.id);
                    table.ForeignKey(
                        name: "FK_Item_Business",
                        column: x => x.Mfg_id,
                        principalTable: "Business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Manufacturer",
                columns: table => new
                {
                    Name = table.Column<string>(fixedLength: true, maxLength: 10, nullable: true),
                    Address_id = table.Column<int>(nullable: true),
                    Contract_Link = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "InventoryItem",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    Item_id = table.Column<int>(nullable: false),
                    Inventory_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItem", x => x.id);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Inventory",
                        column: x => x.Inventory_id,
                        principalTable: "Inventory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Item",
                        column: x => x.Item_id,
                        principalTable: "Item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionItems",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    Transaction_id = table.Column<int>(nullable: false),
                    item_id = table.Column<int>(nullable: false),
                    Cost = table.Column<decimal>(type: "money", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionItems", x => x.id);
                    table.ForeignKey(
                        name: "FK_TransactionItems_Item",
                        column: x => x.id,
                        principalTable: "Item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionItems_Transaction",
                        column: x => x.id,
                        principalTable: "Transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CCA_Person_id",
                table: "Transaction",
                column: "CCA_Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_Inventory_id",
                table: "InventoryItem",
                column: "Inventory_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_Item_id",
                table: "InventoryItem",
                column: "Item_id");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Mfg_id",
                table: "Item",
                column: "Mfg_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Person",
                table: "Transaction",
                column: "CCA_Person_id",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Person",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "InventoryItem");

            migrationBuilder.DropTable(
                name: "Manufacturer");

            migrationBuilder.DropTable(
                name: "TransactionItems");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_CCA_Person_id",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "CCA_Person_id",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "invoice_id",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "payment_number",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "payment_type",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Roles",
                table: "Position");

            migrationBuilder.DropColumn(
                name: "Roles",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "API_Link",
                table: "Business");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Business");

            migrationBuilder.DropColumn(
                name: "User_Link",
                table: "Business");

            migrationBuilder.RenameColumn(
                name: "Status_Details",
                table: "Business",
                newName: "Status Details");

            migrationBuilder.AlterColumn<int>(
                name: "CCA_Account_id",
                table: "Transaction",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Person_id",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source_Check_Num",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status_Type",
                table: "Business",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Person_id",
                table: "Transaction",
                column: "Person_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Person",
                table: "Transaction",
                column: "Person_id",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
