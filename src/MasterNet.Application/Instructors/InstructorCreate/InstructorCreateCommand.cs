using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Domain.Abstractions;
using MasterNet.Domain.Instructors;


namespace MasterNet.Application.Instructors.InstructorCreate;

public class InstructorCreateCommand
{
    public class InstructorCreateCommandRequest : IRequest<Guid>, ICommandBase
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class InstructorCreateCommandHandler : IRequestHandler<InstructorCreateCommandRequest, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public InstructorCreateCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(InstructorCreateCommandRequest request, CancellationToken cancellationToken)
        {
            var instructor = new Instructor
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
            };

            var instructorRepository = _unitOfWork.Repository<Instructor>();
            await instructorRepository.AddAsync(instructor);
            await _unitOfWork.SaveChangesAsync();

            return instructor.Id;
        }
    }
}
