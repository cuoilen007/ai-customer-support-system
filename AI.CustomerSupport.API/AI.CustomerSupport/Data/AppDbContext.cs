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
    }
}
