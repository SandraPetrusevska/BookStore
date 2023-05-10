using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookStore.Models;
using BookStore.Areas.Identity.Data;

namespace BookStore.Data
{
    public class BookStoreContext : IdentityDbContext<BookStoreUser>
    {
        public BookStoreContext (DbContextOptions<BookStoreContext> options)
            : base(options)
        {
        }

        public DbSet<Author> Author { get; set; } = default!;

        public DbSet<Book>? Book { get; set; }

        public DbSet<BookGenre>? BookGenre { get; set; }

        public DbSet<Genre>? Genre { get; set; }

        public DbSet<Review>? Review { get; set; }

        public DbSet<UserBooks>? UserBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           /* modelBuilder.Entity<Book>()
                .HasOne<Author>(p => p.Author)
                .WithMany(p => p.Books)
                .HasForeignKey(p => p.AuthorId)
                .HasPrincipalKey(p => p.Id);

            modelBuilder.Entity<BookGenre>()
                .HasOne<Book>(p => p.Book)
                .WithMany(p => p.BookGenres)
                .HasForeignKey(p => p.BookId)
                .HasPrincipalKey(p => p.Id);

            modelBuilder.Entity<BookGenre>()
                .HasOne<Genre>(p => p.Genre)
                .WithMany(p => p.BooksGenre)
                .HasForeignKey(p => p.GenreId)
                .HasPrincipalKey(p => p.Id);

            modelBuilder.Entity<Review>()
                .HasOne<Book>(p => p.Book)
                .WithMany(p => p.Reviews)
                .HasForeignKey(p => p.BookId)
                .HasPrincipalKey(p => p.Id);

            modelBuilder.Entity<UserBooks>()
                .HasOne<Book>(p => p.Book)
                .WithMany(p => p.UserBooks)
                .HasForeignKey(p => p.BookId)
                .HasPrincipalKey(p => p.Id); */

        }

    }
}
