using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace DJBookingSystem.Tools
{
    /// <summary>
    /// Migrates users from old "Users" container to new "users" container with correct partition key
    /// </summary>
    public class CosmosDbUserMigration
    {
        private const string ENDPOINT = "https://fallen-collective.documents.azure.com:443/";
        private const string KEY = "m3XmNlXeJo1eFKx0qkIQhd4CgB5P3xQbARjcdYQw18J1QfC22XLUCyEhZn7gBtfPSqPfH1KN2OuwACDbhbvMqQ==";
        private const string DATABASE_NAME = "DJBookingDB";
        private const string OLD_CONTAINER = "Users";
        private const string NEW_CONTAINER = "users";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("??????????????????????????????????????????????????????????????????");
            Console.WriteLine("?         ?? COSMOS DB USERS CONTAINER MIGRATION                ?");
            Console.WriteLine("??????????????????????????????????????????????????????????????????");
            Console.WriteLine();

            using var client = new CosmosClient(ENDPOINT, KEY);
            var database = client.GetDatabase(DATABASE_NAME);

            try
            {
                // Get containers
                var oldContainer = database.GetContainer(OLD_CONTAINER);
                var newContainer = database.GetContainer(NEW_CONTAINER);

                Console.WriteLine($"?? Reading users from '{OLD_CONTAINER}'...");

                // Query all users
                var query = "SELECT * FROM c";
                var iterator = oldContainer.GetItemQueryIterator<JObject>(query);
                
                int totalUsers = 0;
                int migratedUsers = 0;
                int errorUsers = 0;

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    
                    foreach (var user in response)
                    {
                        totalUsers++;
                        
                        // Get username (handle both lowercase and uppercase)
                        string username = user["username"]?.ToString() ?? user["Username"]?.ToString();
                        
                        if (string.IsNullOrEmpty(username))
                        {
                            Console.WriteLine($"? User {user["id"]} has no username!");
                            errorUsers++;
                            continue;
                        }

                        try
                        {
                            Console.Write($"   Migrating: {username}...");
                            
                            // Ensure lowercase username property exists
                            if (user["username"] == null && user["Username"] != null)
                            {
                                user["username"] = user["Username"];
                            }

                            // Create in new container with correct partition key
                            await newContainer.CreateItemAsync(user, new PartitionKey(username));
                            
                            Console.WriteLine(" ?");
                            migratedUsers++;
                        }
                        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                        {
                            Console.WriteLine($" ?? Already exists");
                            migratedUsers++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($" ? Error: {ex.Message}");
                            errorUsers++;
                        }
                    }
                }

                Console.WriteLine();
                Console.WriteLine("??????????????????????????????????????????????????????????????????");
                Console.WriteLine("?                     ? MIGRATION COMPLETE                       ?");
                Console.WriteLine("??????????????????????????????????????????????????????????????????");
                Console.WriteLine();
                Console.WriteLine($"?? SUMMARY:");
                Console.WriteLine($"   Total users:    {totalUsers}");
                Console.WriteLine($"   ? Migrated:    {migratedUsers}");
                Console.WriteLine($"   ? Errors:      {errorUsers}");
                Console.WriteLine();
                Console.WriteLine("?? All users have been migrated to the new 'users' container!");
                Console.WriteLine();
                Console.WriteLine("?? NEXT STEPS:");
                Console.WriteLine("   1. Test user registration and login");
                Console.WriteLine("   2. Verify everything works correctly");
                Console.WriteLine("   3. Delete old 'Users' container (Azure Portal)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Migration failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
