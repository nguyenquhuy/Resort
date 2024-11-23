using DATN.Models;
using DATN.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
    public class AmenityController : Controller
    {
        private readonly DATNDbContext _context;

        public AmenityController(DATNDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Amenity.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Amenity amen)
        {
            if (ModelState.IsValid)
            {
                _context.Add(amen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(amen);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var amen = await _context.Amenity.FindAsync(id);
            if (amen == null)
            {
                return NotFound();
            }
            return View(amen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Amenity amen)
        {
            if (id != amen.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(amen);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AmenityExists(amen.ID))
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
            return View(amen);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var amen = await _context.Amenity
                .FirstOrDefaultAsync(m => m.ID == id);
            if (amen == null)
            {
                return NotFound();
            }

            return View(amen);
        }

        // POST: RoomTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var amen = await _context.Amenity.FindAsync(id);
            if (amen != null)
            {
                _context.Amenity.Remove(amen);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool AmenityExists(int id)
        {
            return _context.Amenity.Any(e => e.ID == id);
        }
    }
}
