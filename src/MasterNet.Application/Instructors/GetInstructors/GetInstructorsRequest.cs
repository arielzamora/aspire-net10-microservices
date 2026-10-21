using MasterNet.Application.Core;

namespace MasterNet.Application.Instructors.GetInstructors;

public class GetInstructorsRequest : PagingParams
{

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

}
