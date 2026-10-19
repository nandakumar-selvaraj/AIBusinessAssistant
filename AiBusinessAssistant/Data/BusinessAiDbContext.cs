using AiBusinessAssistant.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiBusinessAssistant.Data
{
    public class BusinessAiDbContext: DbContext
    {
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<OrderWorkflow> OrderWorkflows => Set<OrderWorkflow>();
        public DbSet<OrderWorkflowItem> OrderWorkflowItems => Set<OrderWorkflowItem>();

        public BusinessAiDbContext(DbContextOptions<BusinessAiDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Configure cascade deletes or restrict rules if needed (Optional)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderWorkflow>()
                .HasOne<ChatConversation>()
                .WithOne()
                .HasForeignKey<OrderWorkflow>(workflow => workflow.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderWorkflowItem>()
                .HasKey(item => new { item.ConversationId, item.ProductId });
            modelBuilder.Entity<OrderWorkflowItem>()
                .HasOne<OrderWorkflow>()
                .WithMany(workflow => workflow.Items)
                .HasForeignKey(item => item.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
