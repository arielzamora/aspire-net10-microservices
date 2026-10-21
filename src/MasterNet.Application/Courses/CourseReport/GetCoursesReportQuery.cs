using Core.MediatOR.Contracts;
using MasterNet.Application.Interfaces;
using MasterNet.Domain.Courses;
using MasterNet.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasterNet.Application.Courses.CourseReport;

public class GetCoursesReportQuery
{
    // Return a byte[] instead of a Stream to avoid exposing a stream that may be disposed.
    public record GetCoursesReportQueryRequest : IRequest<byte[]>;

    internal class GetCoursesReportQueryHandler
        : IRequestHandler<GetCoursesReportQueryRequest, byte[]>
    {
        private readonly MasterNetDbContext _context;
        private readonly IReportService<Course> _reportService;

        public GetCoursesReportQueryHandler(
            MasterNetDbContext context,
            IReportService<Course> reportService
        )
        {
            _context = context;
            _reportService = reportService;
        }

        public async Task<byte[]> Handle(
            GetCoursesReportQueryRequest request,
            CancellationToken cancellationToken
        )
        {
            var courses = await _context.Courses!.Take(10).Skip(0).ToListAsync(cancellationToken);

            // GetCsvReport may return a stream backed by network resources.
            // Copy its contents into a new MemoryStream (or get a byte[]) so the caller
            // can't accidentally operate on a stream that was disposed externally.
            var sourceStream = await _reportService.GetCsvReport(courses);

            if (sourceStream is null)
            {
                return Array.Empty<byte>();
            }

            // Ensure position if the returned stream supports seeking
            if (sourceStream.CanSeek)
            {
                try { sourceStream.Position = 0; } catch { /* ignore */ }
            }

            using var ms = new MemoryStream();
            await sourceStream.CopyToAsync(ms, cancellationToken);
            return ms.ToArray();
        }
    }
}
