using MasterNet.Application.Ratings.GetRatings;
using MasterNet.Application.Instructors.GetInstructors;
using MasterNet.Application.Prices.GetPrices;
using MasterNet.Application.Photos.GetPhoto;

namespace MasterNet.Application.Courses.GetCourse;

public record CourseResponse(
    Guid Id,
    string Title,
    string Description,
    int Score,
    DateTime? PublishedAt,
    List<InstructorResponse> Instructors,
    List<RatingResponse> Ratings,
    List<PriceResponse> Prices,
    List<PhotoResponse> Photos
);