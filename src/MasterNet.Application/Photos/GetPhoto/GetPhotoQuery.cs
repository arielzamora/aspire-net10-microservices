namespace MasterNet.Application.Photos.GetPhoto;

public record PhotoResponse(
    Guid? Id,
    string? Url,
    Guid? CourseId
)
{
    public PhotoResponse() : this(null, null, null)
    {
    }
}

public class PhotoUploadResult
{
    public string? PublicId { get; set; }
    public string? Url { get; set; }

}