using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  // Import logging
using NoteApp.Models;
using Microsoft.EntityFrameworkCore;  // Required for async database operations

namespace NoteApp.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotesController> _logger;

        // Constructor with dependency injection for DbContext and Logger
        public NotesController(ApplicationDbContext context, ILogger<NotesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Action method to display all notes
        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch the list of notes from the database asynchronously
                var notes = await _context.Notes.ToListAsync();
                return View(notes);  // Pass notes list to the Index view
            }
            catch (Exception ex)
            {
                // Log error and return an error view if something goes wrong
                _logger.LogError($"Error occurred while fetching notes: {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // GET: Create note page
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create a new note
        [HttpPost]
        [ValidateAntiForgeryToken]  // Security measure to prevent CSRF
        public async Task<IActionResult> Create(Note note)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Add the new note to the database and save changes asynchronously
                    _context.Add(note);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));  // Redirect to the Index action after successful creation
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error occurred while creating note: {ex.Message}");
                }
            }
            return View(note);  // If ModelState is invalid, return to the Create view with the current note model
        }

        // GET: Edit note page
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit action was called with null ID.");
                return NotFound();  // Return 404 error if the ID is null
            }

            // Find the note by ID asynchronously
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                _logger.LogWarning($"No note found with ID: {id}");
                return NotFound();  // Return 404 if the note with the specified ID does not exist
            }

            return View(note);  // Pass the note to the Edit view
        }

        // POST: Save the edited note
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Note note)
        {
            if (id != note.Id)
            {
                _logger.LogWarning("Note ID mismatch in Edit action.");
                return NotFound();  // Return 404 if the note ID doesn't match the submitted note's ID
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the note in the database and save changes asynchronously
                    _context.Update(note);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));  // Redirect to Index after successful update
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError($"Error occurred while updating note: {ex.Message}");
                }
            }
            return View(note);  // If ModelState is invalid, return to the Edit view with the current note model
        }

        // GET: Delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete action was called with null ID.");
                return NotFound();  // Return 404 if the ID is null
            }

            // Find the note by ID asynchronously
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                _logger.LogWarning($"No note found with ID: {id}");
                return NotFound();  // Return 404 if the note doesn't exist
            }

            return View(note);  // Pass the note to the Delete confirmation view
        }

        // POST: Confirm deletion of a note
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Find and remove the note, then save changes asynchronously
                var note = await _context.Notes.FindAsync(id);
                if (note != null)
                {
                    _context.Notes.Remove(note);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));  // Redirect to Index after successful deletion
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while deleting note: {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
