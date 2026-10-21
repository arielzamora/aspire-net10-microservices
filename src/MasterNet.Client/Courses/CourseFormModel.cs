using System.ComponentModel.DataAnnotations;

namespace MasterNet.Client.Courses;

public sealed class CourseFormModel
{
    [Required(ErrorMessage = "Title is required.")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    public string? Description { get; set; }

    public DateTime? PublishedAt { get; set; }
}
