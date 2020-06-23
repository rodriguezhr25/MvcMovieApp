using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using MvcMovie.Data;
using MvcMovie.Models;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;

        public MoviesController(MvcMovieContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string movieGenre, string searchString, string sortOrder , string currentFilter, string genreFilter)
        {

            

            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString == null)
            {
                searchString = currentFilter;
            }
            if (movieGenre == null)
            {
                movieGenre = genreFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["GenreFilter"] = movieGenre;
            var movies = from m in _context.Movie
                         join g in _context.Genre on m.Genre equals g.ID.ToString()
                         select new Movie{ Id = m.Id, Title = m.Title , ReleaseDate = m.ReleaseDate , Genre = g.GenreName , Price = m.Price , Rating  = m.Rating , ProfileImage  = m.ProfileImage };

            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from m in _context.Genre
                                            orderby m.GenreName
                                            select m.GenreName;

            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title.Contains(searchString)) ;
            }

            if (!string.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre == movieGenre);
            }

            
            switch (sortOrder)
            {
             
                case "Date":
                    movies = movies.OrderBy(s => s.ReleaseDate);
                    break;
                case "date_desc":
                    movies = movies.OrderByDescending(s => s.ReleaseDate);
                    break;
            
            }

            var movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Movies = await movies.ToListAsync()
            };
            
           // return View(await movies.AsNoTracking().ToListAsync());
            return View(movieGenreVM);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

        
            var movies = from m in _context.Movie
                         join g in _context.Genre on m.Genre equals g.ID.ToString()
                         select new Movie { Id = m.Id, Title = m.Title, ReleaseDate = m.ReleaseDate, Genre = g.GenreName, Price = m.Price, Rating = m.Rating, ImagePath = m.ImagePath };

            var movie = await movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
           
            var genres = _context.Genre.ToList();

            var genreList = new SelectList(genres, "ID", "GenreName");

            ViewData["Genres"] = genreList;
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price,Rating,ImagePath, ProfileImage")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UploadedFile(movie);
                movie.ImagePath = uniqueFileName;
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            var genres = _context.Genre.ToList();

            var genreList = new SelectList(genres, "ID", "GenreName");

            ViewData["Genres"] = genreList;
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price,Rating,ImagePath,ProfileImage")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadedFile(movie);
                    movie.ImagePath = uniqueFileName;
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
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
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = from m in _context.Movie
                         join g in _context.Genre on m.Genre equals g.ID.ToString()
                         select new Movie { Id = m.Id, Title = m.Title, ReleaseDate = m.ReleaseDate, Genre = g.GenreName, Price = m.Price, Rating = m.Rating, ProfileImage = m.ProfileImage };

            var movie = await movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private string UploadedFile(Movie movie)
        {
            string uniqueFileName = null;

            var file = movie.ProfileImage as IFormFile;
            //var allowedExtensions = new[] { ".jpg", ".png" };
            if (movie.ProfileImage != null)
            {
                var extension = Path.GetExtension(file.FileName);
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + movie.ProfileImage.FileName + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    movie.ProfileImage.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
