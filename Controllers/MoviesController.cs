using Azure.AI.OpenAI;
using Fall2025_Project3_pslumpkins.Data;
using Fall2025_Project3_pslumpkins.Models;
using Fall2025_Project3_pslumpkins.Models.ViewModels;
using Fall2025_Project3_pslumpkins.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VaderSharp2;

namespace Fall2025_Project3_pslumpkins.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id, [FromServices] AzureOpenAIClient ai, [FromServices] IConfiguration cfg)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.ActorMovies).ThenInclude(am => am.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            var deployment = cfg["AzureOpenAI:Deployment"]!;

            var reviews = await AiHelper.GetList(
                ai, deployment,
                "You are a concise movie critic. One short sentence per item. No profanity.",
                $"Audience-style reviews for '{movie.Title}' ({movie.Year}).",
                10);

            var analyzer = new SentimentIntensityAnalyzer();
            var scored = reviews
                .Select(r =>
                {
                    var clean = CleanForVader(r);
                    var compound = string.IsNullOrWhiteSpace(clean)
                        ? 0.0
                        : analyzer.PolarityScores(clean).Compound;
                    return (Text: clean, Compound: compound);
                })
                .ToList();

            var avg = scored.Count == 0 ? 0.0 : Math.Round(scored.Average(x => x.Compound), 3);

            var vm = new MovieDetailsVM
            {
                Movie = movie,
                Actors = movie.ActorMovies.Select(am => am.Actor!).OrderBy(a => a.Name).ToList(),
                Reviews = scored,
                AverageCompound = avg
            };

            return View(vm);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,MovieImdb,Genre,Year")] Movie movie, IFormFile? posterFile)
        {
            if (!ModelState.IsValid) return View(movie);

            if (posterFile is { Length: > 0})
            {
                using var ms = new MemoryStream();
                await posterFile.CopyToAsync(ms);
                movie.Poster = ms.ToArray();
            }

            _context.Add(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,MovieImdb,Genre,Year")] Movie incoming, IFormFile? posterFile)
        {
            if (id != incoming.Id) return NotFound();
            if (!ModelState.IsValid) return View(incoming);

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            movie.Title = incoming.Title;
            movie.MovieImdb = incoming.MovieImdb;
            movie.Genre = incoming.Genre;
            movie.Year = incoming.Year;

            if (posterFile != null && posterFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await posterFile.CopyToAsync(ms);
                movie.Poster = ms.ToArray();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.ActorMovies)
                .ThenInclude(am => am.Actor)
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
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //added this because I was getting a weird 0 value for the vadersharp sentiment analysis
        private static string CleanForVader(string? s) =>
            WebUtility.HtmlDecode(s ?? string.Empty)
                .Replace('\u00A0', ' ')   
                .Replace("\u200B", "")    
                .Replace("\uFEFF", "")    
                .Trim();
        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
