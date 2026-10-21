using MasterNet.Domain.Abstractions;
using MasterNet.Domain.Instructors;
using MasterNet.Domain.Photos;
using MasterNet.Domain.Prices;
using MasterNet.Domain.Ratings;

namespace MasterNet.Domain.Courses;

public class Course : BaseEntity
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<Rating>? Ratings { get; set; }

    public ICollection<Price>? Prices { get; set; }
    public ICollection<CoursePrice>? CoursePrices { get; set; }

    public ICollection<Instructor>? Instructors { get; set; }
    public ICollection<CourseInstructor>? CourseInstructors { get; set; }

    public ICollection<Photo>? Photos { get; set; }
}
