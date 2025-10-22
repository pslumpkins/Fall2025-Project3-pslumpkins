using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_pslumpkins.Data;
using Fall2025_Project3_pslumpkins.Models;

namespace Fall2025_Project3_pslumpkins.Controllers
{
    public class ActorMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ActorMoviesController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var rows = await _context.ActorMovies
                .Include(am => am.Actor)
                .Include(am => am.Movie)
                .OrderBy(am => am.Actor.Name).ThenBy(am => am.Movie.Title)
                .ToListAsync();
            return View(rows);
        }

        public async Task<IActionResult> Details(int? actorId, int? movieId)
        {
            if (actorId == null || movieId == null) return NotFound();

            var row = await _context.ActorMovies
                .Include(am => am.Actor).Include(am => am.Movie)
                .FirstOrDefaultAsync(m => m.ActorId == actorId && m.MovieId == movieId);

            if (row == null) return NotFound();
            return View(row);
        }

      
        public async Task<IActionResult> Create()
        {
           await FillDropdowns();
           return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ActorId,MovieId")] ActorMovie am)
        {

            if (am.ActorId <= 0) ModelState.AddModelError(nameof(am.ActorId), "Please select an actor.");
            if (am.MovieId <= 0) ModelState.AddModelError(nameof(am.MovieId), "Please select a movie.");

         
            if (ModelState.IsValid &&
                await _context.ActorMovies.AnyAsync(x => x.ActorId == am.ActorId && x.MovieId == am.MovieId))
            {
                ModelState.AddModelError(string.Empty, "That relationship already exists.");
            }

            if (!ModelState.IsValid)
            {
                await FillDropdowns(am.ActorId, am.MovieId);
                return View(am);
            }

            _context.ActorMovies.Add(am);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? actorId, int? movieId)
        {
            if (actorId is null || movieId is null) return NotFound();

            var row = await _context.ActorMovies.FindAsync(actorId.Value, movieId.Value);
            if (row == null) return NotFound();

            await FillDropdowns(row.ActorId, row.MovieId);
            return View(row);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int originalActorId, int originalMovieId, [Bind("ActorId,MovieId")] ActorMovie updated)
        {
            var same = originalActorId == updated.ActorId && originalMovieId == updated.MovieId;

            if (!same && await _context.ActorMovies.AnyAsync(x => x.ActorId == updated.ActorId && x.MovieId == updated.MovieId))
                ModelState.AddModelError(string.Empty, "That relationship already exists.");

            if (!ModelState.IsValid)
            {
                await FillDropdowns(updated.ActorId, updated.MovieId);
                return View(updated);
            }

            var old = await _context.ActorMovies.FindAsync(originalActorId, originalMovieId);
            if (old == null) return NotFound();

            _context.ActorMovies.Remove(old);
            _context.ActorMovies.Add(updated);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? actorId, int? movieId)
        {
            if (actorId == null || movieId == null) return NotFound();

            var row = await _context.ActorMovies
                .Include(am => am.Actor).Include(am => am.Movie)
                .FirstOrDefaultAsync(m => m.ActorId == actorId && m.MovieId == movieId);

            if (row == null) return NotFound();
            return View(row);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int actorId, int movieId)
        {
            var row = await _context.ActorMovies.FindAsync(actorId, movieId);
            if (row != null) _context.ActorMovies.Remove(row);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task FillDropdowns(int? selectedActorId = null, int? selectedMovieId = null)
        {
            ViewData["ActorId"] = new SelectList(
                await _context.Actors
                    .OrderBy(a => a.Name)
                    .ToListAsync(),
                "Id",     
                "Name",  
                selectedActorId
            );

            ViewData["MovieId"] = new SelectList(
                await _context.Movies
                    .OrderBy(m => m.Title)
                    .ToListAsync(),
                "Id",     
                "Title",  
                selectedMovieId
            );
        }
    }
}
