using MasterNet.Domain.Abstractions;
using MasterNet.Domain.Courses;

namespace MasterNet.Domain.Ratings;
public class Rating : BaseEntity
{
    public string? Student { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }
}
