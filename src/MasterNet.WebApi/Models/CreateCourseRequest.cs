namespace MasterNet.WebApi.Models;

public class CreateCourseRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? PublishedAt { get; set; }
    public IFormFile? Photo { get; set; }
    public Guid? InstructorId { get; set; }
    public Guid? PriceId { get; set; }
}
