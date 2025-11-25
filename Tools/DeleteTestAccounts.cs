using System;
using System.Threading.Tasks;
using DJBookingSystem.Services;

namespace DJBookingSystem.Tools
{
    /// <summary>
    /// Simple tool to delete test accounts from Cosmos DB
    /// Usage: DeleteTestAccounts.exe [connectionString]
    /// </summary>
    class DeleteTestAccounts
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  DELETE TEST ACCOUNTS FROM COSMOS DB");
            Console.ResetColor();
            Console.WriteLine("===========================================");
            Console.WriteLine();

            string connectionString;
            
            if (args.Length > 0)
            {
                connectionString = args[0];
            }
            else
            {
                connectionString = Environment.GetEnvironmentVariable("COSMOS_DB_CONNECTION_STRING");
                
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR: No connection string provided!");
                    Console.ResetColor();
                    Console.WriteLine("Usage: DeleteTestAccounts.exe [connectionString]");
                    Console.WriteLine("Or set COSMOS_DB_CONNECTION_STRING environment variable");
                    return;
                }
            }

            var testAccounts = new[] { "test1", "test2" };

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Test accounts to delete:");
            foreach (var account in testAccounts)
            {
                Console.WriteLine($"  - {account}");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.Write("Are you sure you want to delete these accounts? (yes/no): ");
            var confirmation = Console.ReadLine();
            
            if (confirmation?.ToLower() != "yes")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Deletion cancelled.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Deleting test accounts...");
            Console.ResetColor();

            try
            {
                var cosmosService = new CosmosDbService(connectionString);
                
                foreach (var username in testAccounts)
                {
                    Console.Write($"Deleting account: {username}...");
                    
                    try
                    {
                        await cosmosService.DeleteUserAccountAsync(username);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(" ? DELETED");
                        Console.ResetColor();
                    }
                    catch (InvalidOperationException)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(" ? NOT FOUND (already deleted)");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" ? FAILED: {ex.Message}");
                        Console.ResetColor();
                    }
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("===========================================");
                Console.WriteLine("  TEST ACCOUNTS DELETION COMPLETE");
                Console.WriteLine("===========================================");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Remaining account: SysAdmin only");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
