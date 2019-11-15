using Microsoft.EntityFrameworkCore.Migrations;

namespace DTShop.PaymentService.Data.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<long>(nullable: false),
                    OrderId = table.Column<int>(nullable: false),
                    Username = table.Column<string>(nullable: false),
                    TotalCost = table.Column<decimal>(nullable: false),
                    IsPassed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
