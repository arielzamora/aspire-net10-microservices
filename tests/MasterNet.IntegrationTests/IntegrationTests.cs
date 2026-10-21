using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MasterNet.IntegrationTests
{
    public class IntegrationTests
    {
        [Fact]

        public async Task GetCoursesShouldReturnOk()
        {
            var waf = new CustomWebApplicationFactroy();
            var httpClient = waf.CreateClient();
            var response = await httpClient.GetAsync("/api/courses");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
