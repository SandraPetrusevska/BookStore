using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace BookStore.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Year Published")]
        public int? YearPublished { get; set; }

        [Display(Name = "Number of Pages")]
        public int? NumPages { get; set; }

        [StringLength(int.MaxValue)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(50)]
        [Display(Name = "Publisher")]
        public string? Publisher { get; set; }

        [StringLength(int.MaxValue)]
        [Display(Name = "Front Page")]
        public string? FrontPage { get; set; }

        [StringLength(int.MaxValue)]
        [Display(Name = "Download Url")]
        public string? DownloadUrl { get; set; }


        [Required]
        [Display(Name = "Author")]
        public int AuthorId { get; set; }
        public Author? Author { get; set; } 

        public ICollection<BookGenre>? BookGenres { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<UserBooks>? UserBooks { get; set; }

    }
}
