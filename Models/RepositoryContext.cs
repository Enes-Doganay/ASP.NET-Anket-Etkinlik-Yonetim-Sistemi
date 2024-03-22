using System.Reflection;
using Basics.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
public class RepositoryContext : IdentityDbContext<ApplicationUser>
{
    public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
    {

    }
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<EventPoll> EventPolls { get; set; }
    public DbSet<Event> Events { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}