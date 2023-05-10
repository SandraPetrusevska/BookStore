using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using BookStore.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Identity;
using BookStore.Areas.Identity.Data;

namespace BookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookStoreContext _context;
        readonly IBufferedFileUploadService _bufferedFileUploadService;
        private readonly UserManager<BookStoreUser> _userManager;

        public BooksController(BookStoreContext context, IBufferedFileUploadService bufferedFileUploadService, UserManager<BookStoreUser> userManager)
        {
            _context = context;
            _bufferedFileUploadService = bufferedFileUploadService;
            _userManager = userManager;
        }

        // GET: Books
        public async Task<IActionResult> Index(string bookGenre, string searchString)
        {
            IQueryable<Book> books = _context.Book.AsQueryable();
            IQueryable<string> genreQuery = _context.Genre.OrderBy(b => b.Id).Select(b => b.GenreName).Distinct();

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(bookGenre))
            {
                books = books.Where(x => x.BookGenres.Any (g => g.Genre.GenreName == bookGenre));
            }

            books = books.Include(m => m.Author)
                         .Include(m => m.Reviews);

            var bookGenreVM = new BookGenreViewModel
            {
                Genres = new SelectList(await genreQuery.ToListAsync()),
                Books = await books.ToListAsync()
            };
            return View(bookGenreVM);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            ViewBag.Bought= false;
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;
            if (userId != null)
            {
                var userBooks = await _context.UserBooks.Where(b => b.AppUser == userId && b.BookId == id).ToListAsync();
                if(userBooks.Any())
                {
                    ViewBag.Bought = true;
                }
            }

            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "Admin")]

        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Author, "Id", "FullName");

            var genres = _context.Genre.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.GenreName
            }).ToList();

            ViewData["BookGenres"] = genres;
            return View();
        }

        // POST: Books/Create
        [Authorize(Roles = "Admin")]

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( [Bind("Id,Title,YearPublished,NumPages,Description,Publisher,FrontPage,DownloadUrl,AuthorId")] Book book, int[] selectedGenres, IFormFile file, IFormFile filePdf)
        {
            ModelState.Remove("file");
            ModelState.Remove("filePdf");

            if (ModelState.IsValid)
            {
                try
                {
                    book.FrontPage = await _bufferedFileUploadService.UploadFile(file);
                    book.DownloadUrl = await _bufferedFileUploadService.UploadFile(filePdf);

                    if (!string.IsNullOrEmpty(book.FrontPage) || !string.IsNullOrEmpty(book.DownloadUrl))
                    {
                        ViewBag.Message = "File Upload Successful";
                    }
                    else
                    {
                        ViewBag.Message = "File Upload Failed";
                    }
                }
                catch (Exception ex)
                {
                    //Log ex
                    ViewBag.Message = "File Upload Failed";
                }

                _context.Add(book);
                await _context.SaveChangesAsync();
                if (selectedGenres != null)
                {
                    foreach (var gId in selectedGenres)
                    {
                        var genreBook = new BookGenre
                        {
                            GenreId = gId,
                            BookId = book.Id
                        };
                        _context.Add(genreBook);
                    }

                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Author, "Id", "FullName", book.AuthorId);


            return View(book);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Author, "Id", "FullName", book.AuthorId);

            var bookG = _context.BookGenre
                .Where(b => b.BookId == book.Id)
                .Select(b => b.GenreId)
                .ToList();

            var genres = _context.Genre.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.GenreName,
                Selected = bookG.Contains(g.Id)
            }).ToList();

            ViewData["BookGenres"] = genres;

            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,YearPublished,NumPages,Description,Publisher,FrontPage,DownloadUrl,AuthorId")] Book book, int[] selectedGenres, IFormFile fileEdit, IFormFile fileEditPdf)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            ModelState.Remove("fileEdit");
            ModelState.Remove("fileEditPdf");

            if (ModelState.IsValid)
            {
                var oldBook = await _context.Book.FindAsync(book.Id);

                if (book == null)
                {
                    // Book not found
                    return null;
                }

                if (fileEdit?.Length>0)
                {
                    try
                    {
                        book.FrontPage = await _bufferedFileUploadService.UploadFile(fileEdit);
                        if (!string.IsNullOrEmpty(book.FrontPage))
                        {
                            ViewBag.Message = "File Upload Successful";
                        }
                        else
                        {
                            ViewBag.Message = "File Upload Failed";
                        }
                    }
                    catch (Exception ex)
                    {
                        //Log ex
                        ViewBag.Message = "File Upload Failed";
                    }
                }
                else
                {
                    book.FrontPage = oldBook.FrontPage;
                }

                if (fileEditPdf?.Length > 0)
                {
                    try
                    {
                        book.DownloadUrl = await _bufferedFileUploadService.UploadFile(fileEditPdf);
                        if (!string.IsNullOrEmpty(book.DownloadUrl))
                        {
                            ViewBag.Message = "File Upload Successful";
                        }
                        else
                        {
                            ViewBag.Message = "File Upload Failed";
                        }
                    }
                    catch (Exception ex)
                    {
                        //Log ex
                        ViewBag.Message = "File Upload Failed";
                    }
                }
                else
                {
                    book.DownloadUrl = oldBook.DownloadUrl;
                }

                try
                {
                    // If the entity is in Added state, remove it from the context
                    _context.Entry(oldBook).State = EntityState.Detached;
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    if (selectedGenres != null)
                    {
                        var old = _context.BookGenre.Where(b => b.BookId == book.Id);
                        _context.BookGenre.RemoveRange(old);
                        foreach (var gId in selectedGenres)
                        {
                            var genreBook = new BookGenre
                            {
                                GenreId = gId,
                                BookId = book.Id
                            };
                            _context.Update(genreBook);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Author, "Id", "FirstName", book.AuthorId);
            var genres = _context.Genre.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.GenreName
            }).ToList();

            ViewData["BookGenres"] = genres;
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [Authorize(Roles = "Admin")]

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Book == null)
            {
                return Problem("Entity set 'BookStoreContext.Book'  is null.");
            }
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
          return (_context.Book?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        [Authorize(Roles = "User")]
        public async Task<IActionResult> Buy(int id)
        {
            if (id == null || _context.Book == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;

            var userBook = new UserBooks
            {
                AppUser = userId,
                BookId = id
            };

            _context.UserBooks.Add(userBook);
            await _context.SaveChangesAsync();

            //ViewBag.BoughtBookId = id;

            return RedirectToAction("MyBooks");
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyBooks(string bookGenre, string searchString)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userId = user?.Id;

            IQueryable<Book> books = _context.Book.AsQueryable();

            IQueryable<string> genreQuery = _context.Genre.OrderBy(b => b.Id).Select(b => b.GenreName).Distinct();

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(bookGenre))
            {
                books = books.Where(x => x.BookGenres.Any(g => g.Genre.GenreName == bookGenre));
            }

            books = books.Include(m => m.Author)
                         .Include(m => m.Reviews)
                         .Where(b => b.UserBooks.Any(u => u.AppUser == userId));

            var bookGenreVM = new BookGenreViewModel
            {
                Genres = new SelectList(await genreQuery.ToListAsync()),
                Books = await books.ToListAsync()
            };

            return View(bookGenreVM);
        }
    }
}
