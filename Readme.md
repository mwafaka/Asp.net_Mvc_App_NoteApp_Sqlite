## Create  NoteApp with MVC .Net8  and sqlite database

1- Open Terminal and run the following command
```bash
dotnet new mvc -n NoteApp
cd NoteApp

```

2- Install the following packages
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools

```


3- Create  new file Notes.cs in Models folder
```bash
using System.ComponentModel.DataAnnotations;
namespace Demo.Models;
public class DemoModel
{
    public int Id { get; set; }

    [Required]
    public string? Title { get; set; }

    [Required]
    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

```
4- Create a class ApplicationDbContext.cs in the Data folder to manage the connection to the database.

```bash
public class ApplicationDbContext : DbContext
{
    public DbSet<Note> Notes { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
}

```

5- Set up Entity Framework and Database Connection

```bash
"ConnectionStrings": {
    "DefaultConnection":"Data Source=app.db"
}

```

6- Configure the Database Context in Program.cs

```bash
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

```

7- Create and Apply Migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update

```
8- Create NotestController.cs file in Controller folder

```bash
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

```


9- Create the Notes folder in Views folder and add The folowings pages to it

- Index.cshtml
```bash
 @model IEnumerable<NoteApp.Models.Note>

<h2>Notes</h2>
<a href="/Notes/Create">Create New Note</a>
<table class="table">
    <thead>
        <tr>
            <th>Title</th>
            <th>Created At</th>
            <th></th>
        </tr>
    </thead>
    <tbody>
        @foreach (var note in Model)
        {
            <tr>
                <td>@note.Title</td>
                <td>@note.CreatedAt.ToShortDateString()</td>
                <td>
                    <a href="/Notes/Edit/@note.Id">Edit</a> |
                    <a href="/Notes/Delete/@note.Id">Delete</a>
                </td>
            </tr>
        }
    </tbody>
</table>

```

- Create.cshtml

```bash
@model NoteApp.Models.Note

<h2>Create Note</h2>
<form asp-action="Create">
    <div class="form-group">
        <label asp-for="Title"></label>
        <input asp-for="Title" class="form-control" />
    </div>
    <div class="form-group">
        <label asp-for="Content"></label>
        <textarea asp-for="Content" class="form-control"></textarea>
    </div>
    <button type="submit" class="btn btn-primary">Save</button>
</form>



```

- Edit.cshtml

```bash
@model NoteApp.Models.Note

@{
    ViewBag.Title = "Edit Note";
}

<h2>Edit Note</h2>

@using (Html.BeginForm("Edit", "Notes", FormMethod.Post, new { @class = "form-horizontal" }))
{
    @Html.AntiForgeryToken()
    
    <div class="form-horizontal">
        <h4>Note</h4>
        <hr />
        @Html.ValidationSummary(true, "", new { @class = "text-danger" })

        @Html.HiddenFor(model => model.Id)

        <div class="form-group">
            @Html.LabelFor(model => model.Title, htmlAttributes: new { @class = "control-label col-md-2" })
            <div class="col-md-10">
                @Html.EditorFor(model => model.Title, new { htmlAttributes = new { @class = "form-control" } })
                @Html.ValidationMessageFor(model => model.Title, "", new { @class = "text-danger" })
            </div>
        </div>

        <div class="form-group">
            @Html.LabelFor(model => model.Content, htmlAttributes: new { @class = "control-label col-md-2" })
            <div class="col-md-10">
                @Html.TextAreaFor(model => model.Content, new { htmlAttributes = new { @class = "form-control", rows = 5 } })
                @Html.ValidationMessageFor(model => model.Content, "", new { @class = "text-danger" })
            </div>
        </div>

        <div class="form-group">
            <div class="col-md-offset-2 col-md-10">
                <input type="submit" value="Save" class="btn btn-primary" />
                @Html.ActionLink("Back to List", "Index", null, new { @class = "btn btn-danger" })
            </div>
        </div>
    </div>
}

```

- Delete.cshtml

```bash
@model NoteApp.Models.Note

@{
    ViewBag.Title = "Delete Note";
}

<h2>Delete Note</h2>

<h3>Are you sure you want to delete this?</h3>
<div>
    <h4>Note</h4>
    <hr />
    <dl class="dl-horizontal">
        <dt>
            @Html.DisplayNameFor(model => model.Title)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.Title)
        </dd>

        <dt>
            @Html.DisplayNameFor(model => model.Content)
        </dt>
        <dd>
            @Html.DisplayFor(model => model.Content)
        </dd>
    </dl>

    @using (Html.BeginForm("Delete", "Notes", FormMethod.Post))
    {
        @Html.AntiForgeryToken()
        <input type="submit" value="Delete" class="btn btn-danger" /> 
        @Html.ActionLink("Cancel", "Index", null, new { @class = "btn btn-secondary" })
    }
</div>

```