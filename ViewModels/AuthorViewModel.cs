using BookStore.Models;

namespace BookStore.ViewModels
{
    public class AuthorViewModel
    {
        public IList<Author> Authors { get; set; }
        public string SearchFirstName { get; set; }
        public string SearchLastName { get; set; }
        public string SearchNationality { get; set; }
    }
}
