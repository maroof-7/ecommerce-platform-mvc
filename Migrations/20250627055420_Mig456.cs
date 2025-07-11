using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DummyProject.Migrations
{
    /// <inheritdoc />
    public partial class Mig456 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Buyers_BuyerId1",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Buyers");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BuyerId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BuyerId1",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BuyerId1",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Buyers",
                columns: table => new
                {
                    BuyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyers", x => x.BuyerId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BuyerId1",
                table: "Orders",
                column: "BuyerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Buyers_BuyerId1",
                table: "Orders",
                column: "BuyerId1",
                principalTable: "Buyers",
                principalColumn: "BuyerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
