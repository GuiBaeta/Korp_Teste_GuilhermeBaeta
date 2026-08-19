using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invoice_number_sequences",
                columns: table => new
                {
                    Year = table.Column<int>(type: "integer", nullable: false),
                    LastNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_number_sequences", x => x.Year);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invoices_Number",
                table: "invoices",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_invoices_Number",
                table: "invoices");

            migrationBuilder.DropTable(
                name: "invoice_number_sequences");
        }
    }
}
