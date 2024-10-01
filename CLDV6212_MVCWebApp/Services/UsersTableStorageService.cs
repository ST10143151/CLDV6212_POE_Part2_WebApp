using Azure;
using Azure.Data.Tables;
using ABC_Retailers.Models;
using System.Threading.Tasks;

namespace ABC_Retailers.Services
{
    public class UserService
    {
        private readonly TableClient _tableClient;

        public UserService(string connectionString)
        {
            var tableServiceClient = new TableServiceClient(connectionString);
            _tableClient = tableServiceClient.GetTableClient("Users");
            _tableClient.CreateIfNotExists();
        }

        public async Task<User> GetUserAsync(string username)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<User>("Users", username);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // User not found
            }
        }

        public async Task AddUserAsync(User user)
        {
            await _tableClient.AddEntityAsync(user);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            var user = await GetUserAsync(username);
            return user != null;
        }
    }
}
