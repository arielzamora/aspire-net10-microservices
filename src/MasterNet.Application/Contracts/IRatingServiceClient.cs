using System;
using System.Collections.Generic;
using System.Text;

namespace MasterNet.Application.Contracts
{
    public interface IRatingServiceClient
    {
        Task<int> GetRating(string id);
        Task SendRating(string id,int rating);
    }
}
