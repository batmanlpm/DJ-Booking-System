using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using DJBookingSystem.Models;

namespace CreateTestAccounts
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // GET YOUR COSMOS CONNECTION STRING
            Console.Write("Enter your Cosmos DB connection string: ");
            string connectionString = Console.ReadLine();
            
            var client = new CosmosClient(connectionString);
            var database = client.GetDatabase("DJBookingDB");
            var container = database.GetContainer("users");
            
            var accounts = new[]
            {
                new { Username = "1", Password = "asdfgh", Role = "User", IsDJ = false, IsVenueOwner = false },
                new { Username = "2", Password = "asdfgh", Role = "DJ", IsDJ = true, IsVenueOwner = false },
                new { Username = "3", Password = "asdfgh", Role = "VenueOwner", IsDJ = false, IsVenueOwner = true },
                new { Username = "4", Password = "asdfgh", Role = "Manager", IsDJ = false, IsVenueOwner = false },
                new { Username = "5", Password = "asdfgh", Role = "SysAdmin", IsDJ = false, IsVenueOwner = false }
            };
            
            foreach (var acc in accounts)
            {
                var user = new
                {
                    id = acc.Username,
                    username = acc.Username,  // FIXED: lowercase to match partition key
                    Password = acc.Password,
                    Role = acc.Role,
                    FullName = $"Test {acc.Role}",
                    Email = "", // No email
                    CreatedAt = DateTime.UtcNow,
                    IsOnline = false,
                    IsDJ = acc.IsDJ,
                    IsVenueOwner = acc.IsVenueOwner,
                    TutorialCompleted = false
                };
                
                try
                {
                    await container.UpsertItemAsync(user, new PartitionKey(acc.Username));
                    Console.WriteLine($"? Created: {acc.Username} ({acc.Role})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? Failed {acc.Username}: {ex.Message}");
                }
            }
            
            Console.WriteLine("\n? All accounts created!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
