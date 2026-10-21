using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiBusinessAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingOrderCompletionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelectedOrderId",
                table: "OrderWorkflows",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedOrderId",
                table: "OrderWorkflows");
        }
    }
}
