using MasterNet.Domain.Courses;

namespace MasterNet.Domain.Prices;

public class Price
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal PromotionPrice { get; set; }
    public ICollection<Course>? Courses { get; set; }
    public ICollection<CoursePrice>? CoursePrices { get; set; }
}
