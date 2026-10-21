using MasterNet.Application.Core;

namespace MasterNet.Application.Ratings.GetRatings;

public class GetRatingsRequest : PagingParams
{

    public string? Student { get; set; }
    public Guid? CourseId { get; set; }

}