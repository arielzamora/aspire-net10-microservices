using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Application.Interfaces;
using MasterNet.Domain.Abstractions;
using MasterNet.Domain.Courses;
using MasterNet.Domain.Instructors;
using MasterNet.Domain.Photos;
using MasterNet.Domain.Prices;

namespace MasterNet.Application.Courses.CourseCreate;

public class CreateCourseCommand
{
    public record CreateCourseCommandRequest(string? Title,
    string? Description,
    Guid? PriceId,
    Guid? InstructorId,
    DateTime? PublishedAt=null,
    byte[]? Photo = null)
    : IRequest<Result<Guid>>, ICommandBase;

    internal class CreateCourseCommandHandler
    : IRequestHandler<CreateCourseCommandRequest, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;

        public CreateCourseCommandHandler(
            IUnitOfWork unitOfWork,
            IPhotoService photoService
        )
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
        }

        public async Task<Result<Guid>> Handle(
            CreateCourseCommandRequest request,
            CancellationToken cancellationToken
        )
        {
            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = request.Title,
                Description = request.Description,
                PublishedAt = request.PublishedAt
            };

            if (request.Photo is not null)
            {
                var photoUploadResult =
                    await _photoService.AddPhoto(request.Photo);

                var photo = new Photo
                {
                    Id = Guid.NewGuid(),
                    Url = photoUploadResult.Url,
                    PublicId = photoUploadResult.PublicId,
                    CourseId = courseId
                };

                course.Photos = new List<Photo> { photo };
            }

            if (request.InstructorId is not null)
            {
                var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(request.InstructorId.Value);

                if (instructor is null)
                {
                    return Result<Guid>.Failure("Instructor not found");
                }

                course.Instructors = new List<Instructor> { instructor };
            }

            if (request.PriceId is not null)
            {
                var price = await _unitOfWork.Repository<Price>().GetByIdAsync(request.PriceId.Value);

                if (price is null)
                {
                    return Result<Guid>.Failure("Price not found");
                }

                course.Prices = new List<Price> { price };
            }

            await _unitOfWork.Repository<Course>().AddAsync(course);

            var result = await _unitOfWork.SaveChangesAsync();

            return result
                ? Result<Guid>.Success(course.Id)
                : Result<Guid>.Failure("Could not insert course");
        }
    }
    public sealed class CreateCourseCommandRequestValidator
    : IValidator<CreateCourseCommandRequest>
    {
        public IEnumerable<ValidationError> Validate(CreateCourseCommandRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                yield return new ValidationError("Title", "Title is required.");

            if (string.IsNullOrWhiteSpace(req.Description))
                yield return new ValidationError("Description", "Description is required.");
        }
    }

}
