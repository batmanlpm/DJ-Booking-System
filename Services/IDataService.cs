using System.Threading.Tasks;
using DJBookingSystem.Models;
using System.Collections.Generic;
using System;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Interface that both FirebaseService and LocalJsonService implement
    /// Allows easy switching between storage backends
    /// </summary>
    public interface IDataService
    {
        // User Management
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByUsernameAsync(string username);
        Task<string> AddUserAsync(User user);
        Task UpdateUserAsync(string id, User user);
        Task DeleteUserAsync(string id);
        Task DeleteUserAccountAsync(string username);
        Task InitializeDefaultAdminAsync();
        Task<User?> GetBannedUserByIPAsync(string ip);

        // App Settings
        Task<AppSettings> GetAppSettingsAsync();
        Task SaveAppSettingsAsync(AppSettings settings);

        // Update Management
        Task<bool> IsUpdateAvailableAsync();
        Task<AppVersion?> GetLatestVersionAsync();

        // Error Logging
        Task LogErrorToChatAsync(string errorMessage, string errorCode, string username);
    }

    /// <summary>
    /// Factory to create the appropriate data service
    /// </summary>
    public static class DataServiceFactory
    {
        public enum ServiceType
        {
            Local,      // JSON file storage
            Supabase    // Future: Supabase PostgreSQL
        }

        public static IDataService CreateService(ServiceType type, string? connectionString = null)
        {
            return type switch
            {
                ServiceType.Local => new LocalJsonServiceAdapter(),
                ServiceType.Supabase => throw new NotImplementedException("Supabase not yet implemented"),
                _ => throw new ArgumentException("Invalid service type")
            };
        }
    }

    // Adapter for LocalJsonService
    internal class LocalJsonServiceAdapter : IDataService
    {
        private readonly LocalJsonService _service = new();

        public Task<List<User>> GetAllUsersAsync() => _service.GetAllUsersAsync();
        public Task<User?> GetUserByUsernameAsync(string username) => _service.GetUserByUsernameAsync(username);
        public Task<string> AddUserAsync(User user) => _service.AddUserAsync(user);
        public Task UpdateUserAsync(string id, User user) => _service.UpdateUserAsync(id, user);
        public Task DeleteUserAsync(string id) => _service.DeleteUserAsync(id);
        public Task DeleteUserAccountAsync(string username) => _service.DeleteUserAccountAsync(username);
        public Task InitializeDefaultAdminAsync() => _service.InitializeDefaultAdminAsync();
        public Task<User?> GetBannedUserByIPAsync(string ip) => _service.GetBannedUserByIPAsync(ip);
        public Task<AppSettings> GetAppSettingsAsync() => _service.GetAppSettingsAsync();
        public Task SaveAppSettingsAsync(AppSettings settings) => _service.SaveAppSettingsAsync(settings);
        public Task<bool> IsUpdateAvailableAsync() => _service.IsUpdateAvailableAsync();
        public Task<AppVersion?> GetLatestVersionAsync() => _service.GetLatestVersionAsync();
        public Task LogErrorToChatAsync(string errorMessage, string errorCode, string username) => 
            _service.LogErrorToChatAsync(errorMessage, errorCode, username);
    }
}
