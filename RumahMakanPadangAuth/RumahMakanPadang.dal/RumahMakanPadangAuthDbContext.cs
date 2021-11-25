using Microsoft.EntityFrameworkCore;
using RumahMakanPadangAuth.dal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RumahMakanPadangAuth.dal
{
    public class RumahMakanPadangAuthDbContext : DbContext
    {
        public RumahMakanPadangAuthDbContext(DbContextOptions<RumahMakanPadangAuthDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
