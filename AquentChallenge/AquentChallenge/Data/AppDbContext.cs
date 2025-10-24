using AquentChallenge.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AquentChallenge.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Person> People => Set<Person>();
    }
}
