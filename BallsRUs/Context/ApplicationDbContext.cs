﻿using BallsRUs.Data;
using BallsRUs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BallsRUs.Context
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasMany(e => e.Categories)
                .WithMany(e => e.Products);

            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            modelBuilder.Entity<Product>()
                .HasMany(product => product.Categories)
                .WithMany(category => category.Products)
                .UsingEntity<ProductCategory>(
                    j => j.HasOne(pc => pc.Category).WithMany().HasForeignKey(pc => pc.CategoryId),
                    j => j.HasOne(pc => pc.Product).WithMany().HasForeignKey(pc => pc.ProductId));

            base.OnModelCreating(modelBuilder);
            modelBuilder.Seed();
        }
    }
}