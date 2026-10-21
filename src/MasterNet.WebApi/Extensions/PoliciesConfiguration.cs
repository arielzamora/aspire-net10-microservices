using MasterNet.Domain.Security;

namespace MasterNet.WebApi.Extensions;

public static class PoliciesConfiguration
{

    public static IServiceCollection AddPoliciesServices(
        this IServiceCollection services
    )
    {
        services.AddAuthorization(opt =>
        {
            opt.AddPolicy(
                PolicyMaster.COURSE_READ, policy =>
                   policy.RequireAssertion(
                    context => context.User.HasClaim(
                    c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.COURSE_READ
                    )
                   )
            );

            opt.AddPolicy(
                PolicyMaster.COURSE_WRITE, policy =>
                   policy.RequireAssertion(
                    context => context.User.HasClaim(
                    c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.COURSE_WRITE
                    )
                   )
            );

            opt.AddPolicy(
                PolicyMaster.COURSE_UPDATE, policy =>
                   policy.RequireAssertion(
                    context => context.User.HasClaim(
                    c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.COURSE_UPDATE
                    )
                   )
            );


            opt.AddPolicy(
              PolicyMaster.COURSE_DELETE, policy =>
                 policy.RequireAssertion(
                  context => context.User.HasClaim(
                  c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.COURSE_DELETE
                  )
                 )
          );


            opt.AddPolicy(
             PolicyMaster.INSTRUCTOR_CREATE, policy =>
                policy.RequireAssertion(
                 context => context.User.HasClaim(
                 c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.INSTRUCTOR_CREATE
                 )
                )
         );

            opt.AddPolicy(
                       PolicyMaster.INSTRUCTOR_UPDATE, policy =>
                          policy.RequireAssertion(
                           context => context.User.HasClaim(
                           c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.INSTRUCTOR_UPDATE
                           )
                          )
                   );

            opt.AddPolicy(
                       PolicyMaster.INSTRUCTOR_READ, policy =>
                          policy.RequireAssertion(
                           context => context.User.HasClaim(
                           c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.INSTRUCTOR_READ
                           )
                          )
                   );

            opt.AddPolicy(
                      PolicyMaster.RATING_READ, policy =>
                         policy.RequireAssertion(
                          context => context.User.HasClaim(
                          c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.RATING_READ
                          )
                         )
                  );

            opt.AddPolicy(
                       PolicyMaster.RATING_CREATE, policy =>
                          policy.RequireAssertion(
                           context => context.User.HasClaim(
                           c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.RATING_CREATE
                           )
                          )
                   );

            opt.AddPolicy(
                       PolicyMaster.RATING_DELETE, policy =>
                          policy.RequireAssertion(
                           context => context.User.HasClaim(
                           c => c.Type == CustomClaims.POLICIES && c.Value == PolicyMaster.RATING_DELETE
                           )
                          )
                   );


        }


        );




        return services;
    }

}