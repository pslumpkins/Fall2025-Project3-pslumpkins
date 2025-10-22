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
using System.Threading.Tasks;
using VaderSharp2;


namespace Fall2025_Project3_pslumpkins.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id, [FromServices] AzureOpenAIClient ai, [FromServices] IConfiguration cfg)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors
                .Include(a => a.ActorMovies).ThenInclude(am => am.Movie)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (actor == null) return NotFound();

            var deployment = cfg["AzureOpenAI:Deployment"]!;

            var tweets = await AiHelper.GetList(
                ai, deployment,
                "Generate tweet-like fan comments (<140 chars), one per item. No hashtags or @mentions. Be civil.",
                $"Fan comments about actor {actor.Name} (career, performances, persona).",
                20);

            var analyzer = new SentimentIntensityAnalyzer();
            var scored = tweets
                .Select(t =>
                {
                    var clean = CleanForVader(t);
                    var compound = string.IsNullOrWhiteSpace(clean)
                        ? 0.0
                        : analyzer.PolarityScores(clean).Compound;
                    return (Tweet: clean, Compound: compound);
                })
                .ToList();

            var avg = scored.Count == 0 ? 0.0 : Math.Round(scored.Average(x => x.Compound), 3);

            var vm = new ActorDetailsVM
            {
                Actor = actor,
                Movies = actor.ActorMovies.Select(am => am.Movie!)
                                          .OrderBy(m => m.Title)
                                          .ToList(),
                Tweets = scored,              
                AverageCompound = avg
            };

            return View(vm);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ActorImdb,Gender,Age")] Actor actor, IFormFile? photoFile)
        {
            if (!ModelState.IsValid) return View(actor);
            if (photoFile != null && photoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await photoFile.CopyToAsync(ms);
                actor.Photo = ms.ToArray();
            }
            _context.Add(actor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FindAsync(id);
            
            if (actor == null) return NotFound();
            
            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ActorImdb,Gender,Age")] Actor incoming, IFormFile? photoFile)
        {
            if (id != incoming.Id) return NotFound();
            if (!ModelState.IsValid) return View(incoming);

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();


            actor.Name = incoming.Name;
            actor.ActorImdb = incoming.ActorImdb;
            actor.Gender = incoming.Gender;
            actor.Age = incoming.Age;


            if (photoFile != null && photoFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await photoFile.CopyToAsync(ms);
                actor.Photo = ms.ToArray();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(a => a.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private static string CleanForVader(string? s) =>
            WebUtility.HtmlDecode(s ?? string.Empty)
                .Replace('\u00A0', ' ')
                .Replace("\u200B", "")
                .Replace("\uFEFF", "")
                .Trim();
        private bool ActorExists(int id)
        {
            return _context.Actors.Any(a => a.Id == id);
        }
    }
}
