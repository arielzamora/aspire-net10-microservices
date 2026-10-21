using MasterNet.Domain.Abstractions;
using MasterNet.Domain.Courses;

namespace MasterNet.Domain.Instructors;

public class Instructor : BaseEntity
{
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? Degree { get; set; }

    public ICollection<Course>? Courses { get; set; }
    public ICollection<CourseInstructor>? CourseInstructors { get; set; }
}