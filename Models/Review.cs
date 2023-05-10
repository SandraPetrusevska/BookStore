using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Review
    {
        public int Id { get; set; }

       //[Required]
        [StringLength(450)]
        public string AppUser { get; set; } 

        [Required]
        [StringLength(500)]
        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Rating")]
        public int? Rating { get; set; }

        [Required]
        [Display(Name = "Book")]
        public int BookId { get; set; }
        public Book? Book { get; set; }
    }
}
