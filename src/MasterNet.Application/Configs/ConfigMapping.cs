using System;
using Core.Mappy.Interfaces;
using MasterNet.Application.Courses.GetCourse;
using MasterNet.Application.Instructors.GetInstructors;
using MasterNet.Application.Photos.GetPhoto;
using MasterNet.Application.Prices.GetPrices;
using MasterNet.Application.Ratings.GetRatings;
using MasterNet.Domain.Courses;
using MasterNet.Domain.Instructors;
using MasterNet.Domain.Photos;
using MasterNet.Domain.Prices;
using MasterNet.Domain.Ratings;

namespace MasterNet.Application.Configs;

public class ConfigMapping : IMappingProfile
{
    public void Configure(IMapper mapper)
    {
         mapper.CreateMap<Course, CourseResponse>();
         mapper.CreateMap<Photo, PhotoResponse>();

        mapper.CreateMap<Price, PriceResponse>();

        mapper.CreateMap<Instructor, InstructorResponse>();

        mapper.CreateMap<Rating, RatingResponse>();


    }
}
