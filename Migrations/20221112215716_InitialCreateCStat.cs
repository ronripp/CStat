using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CStat.Migrations
{
    public partial class InitialCreateCStat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ronripp_CStat");

            migrationBuilder.CreateTable(
                name: "Address",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Street = table.Column<string>(maxLength: 30, nullable: false),
                    Town = table.Column<string>(maxLength: 30, nullable: false),
                    State = table.Column<string>(maxLength: 20, nullable: false),
                    ZipCode = table.Column<string>(maxLength: 9, nullable: false),
                    Phone = table.Column<string>(maxLength: 20, nullable: true),
                    Fax = table.Column<string>(maxLength: 20, nullable: true),
                    Country = table.Column<string>(fixedLength: true, maxLength: 3, nullable: false),
                    WebSite = table.Column<string>(maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.id);
                },
                comment: "Address");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(fixedLength: true, maxLength: 30, nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Manufacturer",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(fixedLength: true, maxLength: 10, nullable: true),
                    Address_id = table.Column<int>(nullable: true),
                    Contract_Link = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "ronripp_CStat",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "ronripp_CStat",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "ronripp_CStat",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "ronripp_CStat",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "ronripp_CStat",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "ronripp_CStat",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Income_Type = table.Column<int>(nullable: true),
                    Expense_Type = table.Column<int>(nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(10, 2)", nullable: true),
                    CCA_Person_id = table.Column<int>(nullable: true),
                    Business_id = table.Column<int>(nullable: true),
                    Church_id = table.Column<int>(nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CCA_Account_id = table.Column<int>(nullable: true),
                    payment_type = table.Column<int>(nullable: true),
                    memo = table.Column<string>(maxLength: 50, nullable: true),
                    invoice_id = table.Column<string>(fixedLength: true, maxLength: 10, nullable: true),
                    payment_number = table.Column<string>(fixedLength: true, maxLength: 30, nullable: true),
                    link = table.Column<string>(maxLength: 700, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Business",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Address_id = table.Column<int>(nullable: true),
                    Type = table.Column<int>(nullable: true),
                    Terms = table.Column<string>(maxLength: 255, nullable: true),
                    Fees = table.Column<string>(maxLength: 255, nullable: true),
                    Status = table.Column<int>(nullable: true),
                    Status_Details = table.Column<string>(maxLength: 255, nullable: true),
                    POC_id = table.Column<int>(nullable: true),
                    Contract_Link = table.Column<string>(maxLength: 255, nullable: true),
                    User_Link = table.Column<string>(fixedLength: true, maxLength: 255, nullable: true),
                    API_Link = table.Column<string>(fixedLength: true, maxLength: 255, nullable: true),
                    Acct_id = table.Column<string>(maxLength: 50, nullable: true),
                    User_Name = table.Column<string>(maxLength: 30, nullable: true),
                    Password = table.Column<string>(maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Business", x => x.id);
                    table.ForeignKey(
                        name: "FK_Business_Address",
                        column: x => x.Address_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(nullable: false),
                    Account_Num = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Terms = table.Column<string>(maxLength: 255, nullable: true),
                    Business_id = table.Column<int>(nullable: false),
                    Contract_Link = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.id);
                    table.ForeignKey(
                        name: "FK_Account_Business",
                        column: x => x.Business_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(fixedLength: true, maxLength: 80, nullable: false),
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
                        principalSchema: "ronripp_CStat",
                        principalTable: "Business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_Manufacturer",
                        column: x => x.Mfg_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Manufacturer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionItems",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                        principalSchema: "ronripp_CStat",
                        principalTable: "Item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionItems_Transaction",
                        column: x => x.id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Church",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Address_id = table.Column<int>(nullable: true),
                    Affiliation = table.Column<string>(maxLength: 50, nullable: false),
                    Membership_Status = table.Column<int>(nullable: false),
                    Status_Details = table.Column<string>(maxLength: 255, nullable: true),
                    Senior_Minister_id = table.Column<int>(nullable: true),
                    Youth_Minister_id = table.Column<int>(nullable: true),
                    Trustee1_id = table.Column<int>(nullable: true),
                    Trustee2_id = table.Column<int>(nullable: true),
                    Trustee3_id = table.Column<int>(nullable: true),
                    Alternate1_id = table.Column<int>(nullable: true),
                    Alternate2_id = table.Column<int>(nullable: true),
                    Alternate3_id = table.Column<int>(nullable: true),
                    Elder1_id = table.Column<int>(nullable: true),
                    Elder2_id = table.Column<int>(nullable: true),
                    Elder3_id = table.Column<int>(nullable: true),
                    Elder4_id = table.Column<int>(nullable: true),
                    Elder5_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Church", x => x.id);
                    table.ForeignKey(
                        name: "FK_Church_Address",
                        column: x => x.Address_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Start_Time = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    End_Time = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 255, nullable: true),
                    Church_id = table.Column<int>(nullable: true),
                    Cost_Child = table.Column<decimal>(type: "decimal(10, 2)", nullable: true),
                    Cost_Adult = table.Column<decimal>(type: "decimal(10, 2)", nullable: true),
                    Cost_Family = table.Column<decimal>(type: "decimal(10, 2)", nullable: true),
                    Cost_Cabin = table.Column<decimal>(type: "decimal(10, 2)", nullable: true),
                    Cost_Lodge = table.Column<decimal>(type: "decimal(10, 2)", nullable: true),
                    Cost_Tent = table.Column<decimal>(type: "decimal(10, 2)", nullable: true),
                    Contract_Link = table.Column<string>(maxLength: 255, nullable: true),
                    staff = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.id);
                    table.ForeignKey(
                        name: "FK_Event_Church",
                        column: x => x.Church_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Church",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(maxLength: 30, nullable: false),
                    LastName = table.Column<string>(maxLength: 30, nullable: false),
                    Alias = table.Column<string>(maxLength: 30, nullable: true),
                    DOB = table.Column<DateTime>(type: "date", nullable: true),
                    Gender = table.Column<byte>(nullable: true),
                    Status = table.Column<long>(nullable: true),
                    SSNum = table.Column<string>(maxLength: 10, nullable: true),
                    Address_id = table.Column<int>(nullable: true),
                    PG1_Person_id = table.Column<int>(nullable: true),
                    PG2_Person_id = table.Column<int>(nullable: true),
                    Church_id = table.Column<int>(nullable: true),
                    SkillSets = table.Column<long>(nullable: false),
                    CellPhone = table.Column<string>(maxLength: 20, nullable: true),
                    EMail = table.Column<string>(maxLength: 50, nullable: true),
                    ContactPref = table.Column<string>(maxLength: 50, nullable: true),
                    Notes = table.Column<string>(maxLength: 255, nullable: true),
                    Roles = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.id);
                    table.ForeignKey(
                        name: "FK_Person_Address",
                        column: x => x.Address_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Address",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Person_Church",
                        column: x => x.Church_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Church",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Person_Person1",
                        column: x => x.PG1_Person_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Person_Person2",
                        column: x => x.PG2_Person_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Person");

            migrationBuilder.CreateTable(
                name: "Incident",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    Date_Reported = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Description = table.Column<string>(maxLength: 255, nullable: true),
                    Person1_id = table.Column<int>(nullable: true),
                    Person2_id = table.Column<int>(nullable: true),
                    Persion3_id = table.Column<int>(nullable: true),
                    Person4_id = table.Column<int>(nullable: true),
                    Person5_id = table.Column<int>(nullable: true),
                    Report_Link = table.Column<string>(maxLength: 255, nullable: true),
                    Status = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incident", x => x.id);
                    table.ForeignKey(
                        name: "FK_Incident_Person2",
                        column: x => x.Persion3_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incident_Person",
                        column: x => x.Person1_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incident_Person1",
                        column: x => x.Person2_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incident_Person3",
                        column: x => x.Person4_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incident_Person4",
                        column: x => x.Person5_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItem",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Item_id = table.Column<int>(nullable: false),
                    Inventory_id = table.Column<int>(nullable: false),
                    Current_Stock = table.Column<float>(nullable: true),
                    Reorder_Threshold = table.Column<float>(nullable: true),
                    Units = table.Column<int>(nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Order_Id = table.Column<int>(nullable: true),
                    Person_Id = table.Column<int>(nullable: true),
                    State = table.Column<int>(nullable: true),
                    Units_per_day = table.Column<double>(nullable: true),
                    Zone = table.Column<int>(nullable: true),
                    buy1_id = table.Column<int>(nullable: true),
                    buy2_id = table.Column<int>(nullable: true),
                    buy3_id = table.Column<int>(nullable: true),
                    Expected_Cost = table.Column<decimal>(type: "decimal(13, 2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItem", x => x.id);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Transaction1",
                        column: x => x.buy1_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Transaction2",
                        column: x => x.buy2_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Transaction3",
                        column: x => x.buy3_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Inventory",
                        column: x => x.Inventory_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Inventory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Item",
                        column: x => x.Item_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Transaction",
                        column: x => x.Order_Id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryItem_Person",
                        column: x => x.Person_Id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Medical",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Person_id = table.Column<int>(nullable: false),
                    Event_id = table.Column<int>(nullable: true),
                    Form_Link = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medical", x => x.id);
                    table.ForeignKey(
                        name: "FK_Medical_Event",
                        column: x => x.Event_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Medical_Person",
                        column: x => x.Person_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(nullable: false),
                    time = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    result = table.Column<int>(nullable: true),
                    Person_id = table.Column<int>(nullable: true),
                    Church_id = table.Column<int>(nullable: true),
                    Business_id = table.Column<int>(nullable: true),
                    Record_Link = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_Operations_Business",
                        column: x => x.Business_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Church",
                        column: x => x.Church_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Church",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Operations_Person",
                        column: x => x.Person_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Position",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<int>(nullable: false),
                    Person_id = table.Column<int>(nullable: false),
                    Event_id = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Start_Date = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    End_Date = table.Column<DateTime>(nullable: true),
                    Pay = table.Column<decimal>(type: "decimal(10, 2)", nullable: true),
                    Pay_Terms = table.Column<int>(nullable: false),
                    Responsibility_Link = table.Column<string>(maxLength: 255, nullable: true),
                    Comments = table.Column<string>(maxLength: 255, nullable: true),
                    Roles = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => x.id);
                    table.ForeignKey(
                        name: "FK_Position_Person",
                        column: x => x.Person_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Registration",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Event_id = table.Column<int>(nullable: true),
                    Person_id = table.Column<int>(nullable: true),
                    Form_Link = table.Column<string>(maxLength: 255, nullable: false),
                    Current_Grade = table.Column<int>(nullable: true),
                    T_Shirt_Size = table.Column<string>(fixedLength: true, maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registration", x => x.id);
                    table.ForeignKey(
                        name: "FK_Registration_Event",
                        column: x => x.Event_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registration_Person",
                        column: x => x.Person_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Task",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(maxLength: 255, nullable: false),
                    priority = table.Column<int>(nullable: false),
                    Blocking1_id = table.Column<int>(nullable: true),
                    Blocking2_id = table.Column<int>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Person_id = table.Column<int>(nullable: true),
                    Due_Date = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Creation_Date = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    Start_Date = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Actual_Done_Date = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Church_id = table.Column<int>(nullable: true),
                    Plan_Link = table.Column<string>(maxLength: 255, nullable: true),
                    Required_Skills = table.Column<string>(maxLength: 255, nullable: false),
                    Committed_Cost = table.Column<decimal>(type: "money", nullable: true),
                    Committed_Man_Hours = table.Column<double>(nullable: true),
                    Estimated_Done_Date = table.Column<DateTime>(nullable: true),
                    Estimated_Man_Hours = table.Column<double>(nullable: true),
                    Roles = table.Column<long>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Total_Cost = table.Column<decimal>(type: "money", nullable: true),
                    Worker1_id = table.Column<int>(nullable: true),
                    Worker2_id = table.Column<int>(nullable: true),
                    Worker3_id = table.Column<int>(nullable: true),
                    ParentTask_id = table.Column<int>(nullable: true),
                    Event_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Task", x => x.id);
                    table.ForeignKey(
                        name: "FK_Task_Task1",
                        column: x => x.Blocking1_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Task_Task",
                        column: x => x.Blocking2_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Task_Church",
                        column: x => x.Church_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Church",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Task_Event",
                        column: x => x.Event_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Task_Task2",
                        column: x => x.ParentTask_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Task_Person",
                        column: x => x.Person_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Task_Person1",
                        column: x => x.Worker1_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Task_Person2",
                        column: x => x.Worker2_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Task_Person3",
                        column: x => x.Worker3_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attendance",
                schema: "ronripp_CStat",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Person_id = table.Column<int>(nullable: false),
                    Role_Type = table.Column<int>(nullable: false),
                    Start_Time = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    End_Time = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Event_id = table.Column<int>(nullable: true),
                    Registration_id = table.Column<int>(nullable: true),
                    Medical_id = table.Column<int>(nullable: true),
                    Transaction_id = table.Column<int>(nullable: true),
                    Detail_Note = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.id);
                    table.ForeignKey(
                        name: "FK_Attendance_Event",
                        column: x => x.Event_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendance_Medical",
                        column: x => x.Medical_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Medical",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendance_Person",
                        column: x => x.Person_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendance_Registration",
                        column: x => x.Registration_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Registration",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendance_Transaction",
                        column: x => x.Transaction_id,
                        principalSchema: "ronripp_CStat",
                        principalTable: "Transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Business_id",
                schema: "ronripp_CStat",
                table: "Account",
                column: "Business_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "ronripp_CStat",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "ronripp_CStat",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "([NormalizedName] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "ronripp_CStat",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "ronripp_CStat",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "ronripp_CStat",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "ronripp_CStat",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "ronripp_CStat",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "([NormalizedUserName] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Event_id",
                schema: "ronripp_CStat",
                table: "Attendance",
                column: "Event_id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Medical_id",
                schema: "ronripp_CStat",
                table: "Attendance",
                column: "Medical_id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Person_id",
                schema: "ronripp_CStat",
                table: "Attendance",
                column: "Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Registration_id",
                schema: "ronripp_CStat",
                table: "Attendance",
                column: "Registration_id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Transaction_id",
                schema: "ronripp_CStat",
                table: "Attendance",
                column: "Transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_Business_Address_id",
                schema: "ronripp_CStat",
                table: "Business",
                column: "Address_id");

            migrationBuilder.CreateIndex(
                name: "IX_Business_POC_id",
                schema: "ronripp_CStat",
                table: "Business",
                column: "POC_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Address_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Address_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Alternate1_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Alternate1_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Alternate2_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Alternate2_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Alternate3_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Alternate3_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Elder1_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder1_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Elder2_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder2_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Elder3_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder3_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Elder4_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder4_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Elder5_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder5_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Senior_Minister_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Senior_Minister_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Trustee1_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Trustee1_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Trustee2_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Trustee2_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Trustee3_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Trustee3_id");

            migrationBuilder.CreateIndex(
                name: "IX_Church_Youth_Minister_id",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Youth_Minister_id");

            migrationBuilder.CreateIndex(
                name: "IX_Event_Church_id",
                schema: "ronripp_CStat",
                table: "Event",
                column: "Church_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_Persion3_id",
                schema: "ronripp_CStat",
                table: "Incident",
                column: "Persion3_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_Person1_id",
                schema: "ronripp_CStat",
                table: "Incident",
                column: "Person1_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_Person2_id",
                schema: "ronripp_CStat",
                table: "Incident",
                column: "Person2_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_Person4_id",
                schema: "ronripp_CStat",
                table: "Incident",
                column: "Person4_id");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_Person5_id",
                schema: "ronripp_CStat",
                table: "Incident",
                column: "Person5_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_buy1_id",
                schema: "ronripp_CStat",
                table: "InventoryItem",
                column: "buy1_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_buy2_id",
                schema: "ronripp_CStat",
                table: "InventoryItem",
                column: "buy2_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_buy3_id",
                schema: "ronripp_CStat",
                table: "InventoryItem",
                column: "buy3_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_Inventory_id",
                schema: "ronripp_CStat",
                table: "InventoryItem",
                column: "Inventory_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_Item_id",
                schema: "ronripp_CStat",
                table: "InventoryItem",
                column: "Item_id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_Order_Id",
                schema: "ronripp_CStat",
                table: "InventoryItem",
                column: "Order_Id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_Person_Id",
                schema: "ronripp_CStat",
                table: "InventoryItem",
                column: "Person_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Mfg_id",
                schema: "ronripp_CStat",
                table: "Item",
                column: "Mfg_id");

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Event_id",
                schema: "ronripp_CStat",
                table: "Medical",
                column: "Event_id");

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Person_id",
                schema: "ronripp_CStat",
                table: "Medical",
                column: "Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_Business_id",
                schema: "ronripp_CStat",
                table: "Operations",
                column: "Business_id");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_Church_id",
                schema: "ronripp_CStat",
                table: "Operations",
                column: "Church_id");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_Person_id",
                schema: "ronripp_CStat",
                table: "Operations",
                column: "Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Address_id",
                schema: "ronripp_CStat",
                table: "Person",
                column: "Address_id");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Church_id",
                schema: "ronripp_CStat",
                table: "Person",
                column: "Church_id");

            migrationBuilder.CreateIndex(
                name: "IX_Person_PG1_Person_id",
                schema: "ronripp_CStat",
                table: "Person",
                column: "PG1_Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Person_PG2_Person_id",
                schema: "ronripp_CStat",
                table: "Person",
                column: "PG2_Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Position_Person_id",
                schema: "ronripp_CStat",
                table: "Position",
                column: "Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_Event_id",
                schema: "ronripp_CStat",
                table: "Registration",
                column: "Event_id");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_Person_id",
                schema: "ronripp_CStat",
                table: "Registration",
                column: "Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Blocking1_id",
                schema: "ronripp_CStat",
                table: "Task",
                column: "Blocking1_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Blocking2_id",
                schema: "ronripp_CStat",
                table: "Task",
                column: "Blocking2_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Church_id",
                schema: "ronripp_CStat",
                table: "Task",
                column: "Church_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Event_id",
                schema: "ronripp_CStat",
                table: "Task",
                column: "Event_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_ParentTask_id",
                schema: "ronripp_CStat",
                table: "Task",
                column: "ParentTask_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Person_id",
                schema: "ronripp_CStat",
                table: "Task",
                column: "Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Worker1_id",
                schema: "ronripp_CStat",
                table: "Task",
                column: "Worker1_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Worker2_id",
                schema: "ronripp_CStat",
                table: "Task",
                column: "Worker2_id");

            migrationBuilder.CreateIndex(
                name: "IX_Task_Worker3_id",
                schema: "ronripp_CStat",
                table: "Task",
                column: "Worker3_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Business_id",
                schema: "ronripp_CStat",
                table: "Transaction",
                column: "Business_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CCA_Account_id",
                schema: "ronripp_CStat",
                table: "Transaction",
                column: "CCA_Account_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CCA_Person_id",
                schema: "ronripp_CStat",
                table: "Transaction",
                column: "CCA_Person_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Church_id",
                schema: "ronripp_CStat",
                table: "Transaction",
                column: "Church_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Business",
                schema: "ronripp_CStat",
                table: "Transaction",
                column: "Business_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Business",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Person",
                schema: "ronripp_CStat",
                table: "Transaction",
                column: "CCA_Person_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Church",
                schema: "ronripp_CStat",
                table: "Transaction",
                column: "Church_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Church",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account",
                schema: "ronripp_CStat",
                table: "Transaction",
                column: "CCA_Account_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Account",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Business_Person",
                schema: "ronripp_CStat",
                table: "Business",
                column: "POC_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person5",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Alternate1_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person6",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Alternate2_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person7",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Alternate3_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person8",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder1_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person9",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder2_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person10",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder3_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person11",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder4_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person12",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Elder5_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Senior_Minister_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person2",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Trustee1_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person3",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Trustee2_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person4",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Trustee3_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Church_Person1",
                schema: "ronripp_CStat",
                table: "Church",
                column: "Youth_Minister_id",
                principalSchema: "ronripp_CStat",
                principalTable: "Person",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person5",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person6",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person7",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person8",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person9",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person10",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person11",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person12",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person2",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person3",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person4",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropForeignKey(
                name: "FK_Church_Person1",
                schema: "ronripp_CStat",
                table: "Church");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Attendance",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Incident",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "InventoryItem",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Operations",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Position",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Task",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "TransactionItems",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Medical",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Registration",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Inventory",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Item",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Transaction",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Event",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Manufacturer",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Account",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Business",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Person",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Church",
                schema: "ronripp_CStat");

            migrationBuilder.DropTable(
                name: "Address",
                schema: "ronripp_CStat");
        }
    }
}
