using StackExchange.Redis;
using System.Reflection.PortableExecutable;

namespace MasterNet.RatingService
{
    public class RatingStore(IConnectionMultiplexer connection)
    {
        public void AddRating(string id, int rating)
        {
            var db = connection.GetDatabase();


            db.ListRightPushAsync(id, rating);
        }

        public async Task<int>GetAverageRating(string id)
        {
            var db = connection.GetDatabase();
            
            var values = await db.ListRangeAsync(id);

            if (values.Length == 0) return 0;


            var promedio =(int)Math.Round(values.Select(x => (int)x).Average(),0);

            return promedio;
        }

    }


}
