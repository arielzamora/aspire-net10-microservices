using MasterNet.Application.Contracts;

namespace MasterNet.WebApi
{
    public class RatingServiceHttpClient(HttpClient client,ILogger <RatingServiceHttpClient> logger):IRatingServiceClient
    {
        public Task<int> GetRating(string id) =>
            client.GetFromJsonAsync<int>($"/ratings?id={id}");
    
        public Task SendRating(string id,int rating)
        {
            logger.LogInformation("Enviando clasificacion al curso {id} de {ratimg}", rating, id);
            return client.PostAsJsonAsync($"/ratings", new {
                                                            Id=id,
                                                            Rating = rating 
                                                            });
    
        }
    }
}
