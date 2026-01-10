using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nethesap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to Payments table
            migrationBuilder.AddColumn<int>(
                name: "RefundStatus",
                table: "Payments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalSaleId",
                table: "Payments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRefund",
                table: "Payments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Add new columns to PaymentItems table
            migrationBuilder.AddColumn<bool>(
                name: "IsRefunded",
                table: "PaymentItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RefundedQuantity",
                table: "PaymentItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove columns from Payments table
            migrationBuilder.DropColumn(
                name: "RefundStatus",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OriginalSaleId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsRefund",
                table: "Payments");

            // Remove columns from PaymentItems table
            migrationBuilder.DropColumn(
                name: "IsRefunded",
                table: "PaymentItems");

            migrationBuilder.DropColumn(
                name: "RefundedQuantity",
                table: "PaymentItems");
        }
    }
}
