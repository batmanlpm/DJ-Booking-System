using DJBookingSystem.Models;

namespace DJBookingSystem.Services
{
    /// <summary>
    /// Validates role changes based on hierarchy and permissions
    /// </summary>
    public static class RoleChangeValidator
    {
        /// <summary>
        /// Check if a user can promote/demote another user to a specific role
        /// </summary>
        /// <param name="currentUserRole">The role of the user making the change</param>
        /// <param name="targetUserRole">The current role of the user being changed</param>
        /// <param name="newRole">The desired new role</param>
        /// <param name="isSelfChange">True if user is trying to change their own role</param>
        /// <param name="errorMessage">Output error message if validation fails</param>
        /// <returns>True if the change is allowed</returns>
        public static bool CanChangeRole(
            UserRole currentUserRole,
            UserRole targetUserRole,
            UserRole newRole,
            bool isSelfChange,
            out string errorMessage)
        {
            // RULE 1: Nobody can change their own role (prevents self-promotion)
            if (isSelfChange)
            {
                errorMessage = "You cannot change your own role.";
                return false;
            }

            // RULE 2: SysAdmin cannot be demoted by anyone (including themselves)
            if (targetUserRole == UserRole.SysAdmin && newRole != UserRole.SysAdmin)
            {
                errorMessage = "SysAdmin role cannot be demoted. This is a protected role.";
                return false;
            }

            // RULE 3: Only SysAdmin can promote to SysAdmin
            if (newRole == UserRole.SysAdmin && currentUserRole != UserRole.SysAdmin)
            {
                errorMessage = "Only a SysAdmin can promote users to SysAdmin role.";
                return false;
            }

            // RULE 4: Managers can only manage roles below them
            if (currentUserRole == UserRole.Manager)
            {
                // Manager cannot touch SysAdmin
                if (targetUserRole == UserRole.SysAdmin)
                {
                    errorMessage = "Managers cannot modify SysAdmin accounts.";
                    return false;
                }

                // Manager cannot promote anyone to SysAdmin
                if (newRole == UserRole.SysAdmin)
                {
                    errorMessage = "Managers cannot promote users to SysAdmin.";
                    return false;
                }

                // Manager CAN promote to Manager, DJ, VenueOwner, or User
                // Manager CAN demote Manager, DJ, VenueOwner to any role below
                // This is all allowed
            }

            // RULE 5: DJ and VenueOwner have no promotion/demotion powers
            if (currentUserRole == UserRole.DJ || currentUserRole == UserRole.VenueOwner)
            {
                errorMessage = $"{currentUserRole} role does not have permission to change user roles.";
                return false;
            }

            // RULE 6: Regular users have no promotion/demotion powers
            if (currentUserRole == UserRole.User)
            {
                errorMessage = "User role does not have permission to change user roles.";
                return false;
            }

            // All checks passed
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Get a user-friendly explanation of what roles a user can manage
        /// </summary>
        public static string GetRoleManagementInfo(UserRole role)
        {
            return role switch
            {
                UserRole.SysAdmin => "SysAdmin can promote/demote anyone to any role (except cannot demote other SysAdmins).",
                UserRole.Manager => "Manager can promote users to User/DJ/VenueOwner/Manager and demote those roles to User.",
                UserRole.DJ => "DJ role cannot promote or demote users.",
                UserRole.VenueOwner => "VenueOwner role cannot promote or demote users.",
                UserRole.User => "User role cannot promote or demote users.",
                _ => "Unknown role."
            };
        }
    }
}
