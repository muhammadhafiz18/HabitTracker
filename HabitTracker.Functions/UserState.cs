using Azure.Storage.Blobs;
using Newtonsoft.Json;
using System.Text;
using HabitTracker.Functions.Models;

namespace HabitTracker
{
    public class UserState
    {
        string jsonUrl = "YourJSONUrlforUserStateHere";
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string blobConnectionString = "YourBlobConnectionStringHere";
        private readonly string containerName = "userdata";
        private readonly string blobName = "UserState.json";

        public UserState()
        {
            _blobServiceClient = new BlobServiceClient(blobConnectionString);
        }

        public async Task<List<UserStatee>> UserDetailGetter()
        {
            var response = await _httpClient.GetAsync(jsonUrl);

            // Read the JSON content directly from the response
            var jsonString = await response.Content.ReadAsStringAsync();

            var existingData = JsonConvert.DeserializeObject<List<UserStatee>>(jsonString) ?? new List<UserStatee>();

            return existingData;
        }

        public async Task UserDetailAdder(UserStatee userState)
        {
            var response = await _httpClient.GetAsync(jsonUrl);

            var jsonString = await response.Content.ReadAsStringAsync();
            var existingData = JsonConvert.DeserializeObject<List<UserStatee>>(jsonString) ?? new List<UserStatee>();

            existingData.Add(userState);

            var updatedJson = JsonConvert.SerializeObject(existingData, Formatting.Indented);

            // Step 3: Upload the updated JSON back to Azure Blob Storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(updatedJson)))
            {
                await blobClient.UploadAsync(uploadStream, overwrite: true);
            }
        }

        public async Task UserDetailRemover(UserStatee userStateToRemove)
        {
            if (userStateToRemove == null)
            {
                return;
            }
            var response = await _httpClient.GetAsync(jsonUrl);

            var jsonString = await response.Content.ReadAsStringAsync();
            var existingData = JsonConvert.DeserializeObject<List<UserStatee>>(jsonString) ?? new List<UserStatee>();

            var chatToRemove = existingData.FirstOrDefault(c => c.UserId == userStateToRemove.UserId);


            existingData.Remove(chatToRemove);

            var updatedJson = JsonConvert.SerializeObject(existingData, Formatting.Indented);

            // Step 3: Upload the updated JSON back to Azure Blob Storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(updatedJson)))
            {
                await blobClient.UploadAsync(uploadStream, overwrite: true);
            }
        }
    }
}
