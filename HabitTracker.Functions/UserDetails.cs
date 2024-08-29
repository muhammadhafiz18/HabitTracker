using Azure.Storage.Blobs;
using Newtonsoft.Json;
using System.Text;
using HabitTracker.Functions.Models;

namespace HabitTracker
{
    public class UserDetails
    {
        string jsonUrl = "YourJsonURLForUserDataHere";
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string blobConnectionString = "YourBlobConnectionStringHere";
        private readonly string containerName = "userdata";
        private readonly string blobName = "UserData.json";

        public UserDetails()
        {
            _blobServiceClient = new BlobServiceClient(blobConnectionString);
        }

        public async Task<List<UserData>> UserDetailGetter()
        {
            var response = await _httpClient.GetAsync(jsonUrl);

            // Read the JSON content directly from the response
            var jsonString = await response.Content.ReadAsStringAsync();


            var existingData = JsonConvert.DeserializeObject<List<UserData>>(jsonString) ?? new List<UserData>();

            return existingData;
        }

        public async Task UserDetailAdder(UserData userDetails)
        {
            var response = await _httpClient.GetAsync(jsonUrl);

            var jsonString = await response.Content.ReadAsStringAsync();
            var existingData = JsonConvert.DeserializeObject<List<UserData>>(jsonString) ?? new List<UserData>();

            existingData.Add(userDetails);

            var updatedJson = JsonConvert.SerializeObject(existingData, Formatting.Indented);

            // Step 3: Upload the updated JSON back to Azure Blob Storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(updatedJson)))
            {
                await blobClient.UploadAsync(uploadStream, overwrite: true);
            }
        }

        public async Task UserDetailRemover(UserData userDetailToRemove)
        {
            var response = await _httpClient.GetAsync(jsonUrl);

            var jsonString = await response.Content.ReadAsStringAsync();
            var existingData = JsonConvert.DeserializeObject<List<UserData>>(jsonString) ?? new List<UserData>();

            var chatToRemove = existingData.FirstOrDefault(c => c.UserId == userDetailToRemove.UserId);

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
