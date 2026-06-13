using AI.CustomerSupport.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
        DbContextOptions<AppDbContext> options): base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Conversation> Conversations => Set<Conversation>();

        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<SupportPolicy> SupportPolicies => Set<SupportPolicy>();
        public DbSet<ChatEvaluation> ChatEvaluations => Set<ChatEvaluation>();
        public DbSet<ModelTrainingExample> ModelTrainingExamples => Set<ModelTrainingExample>();
        public DbSet<TrainingRun> TrainingRuns => Set<TrainingRun>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ChatEvaluation>()
                .HasOne(x => x.UserMessage)
                .WithMany()
                .HasForeignKey(x => x.UserMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatEvaluation>()
                .HasOne(x => x.AssistantMessage)
                .WithMany()
                .HasForeignKey(x => x.AssistantMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ModelTrainingExample>()
                .HasIndex(x => x.ChatEvaluationId)
                .IsUnique()
                .HasFilter("[ChatEvaluationId] IS NOT NULL");

            modelBuilder.Entity<ModelTrainingExample>()
                .HasOne(x => x.ChatEvaluation)
                .WithMany()
                .HasForeignKey(x => x.ChatEvaluationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
