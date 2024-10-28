namespace NoteApp.Models;
using System.ComponentModel.DataAnnotations;

public class Note
{
    public int Id { get; set; }

  [Required(ErrorMessage = "Title is required.")]
    [StringLength(10, ErrorMessage = "Title cannot exceed 10 characters.")]
    public string? Title { get; set; }

    
    [Required(ErrorMessage = "Content is required.")]
   [StringLength(10, ErrorMessage = "Title cannot exceed 10 characters.")]

    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
