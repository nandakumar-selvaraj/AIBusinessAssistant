using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiBusinessAssistant.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderWorkflows",
                columns: table => new
                {
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedOrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrderNumber = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderWorkflows", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_OrderWorkflows_ChatConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ChatConversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderWorkflowItems",
                columns: table => new
                {
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderWorkflowItems", x => new { x.ConversationId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_OrderWorkflowItems_OrderWorkflows_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "OrderWorkflows",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderWorkflowItems");

            migrationBuilder.DropTable(
                name: "OrderWorkflows");
        }
    }
}
