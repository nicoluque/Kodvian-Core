namespace Kodvian.Core.Application.Common.Security;

public static class RolePermissionMap
{
    public static IReadOnlyCollection<string> GetPermissions(string roleName)
    {
        return roleName switch
        {
            RoleNames.Administrator =>
            [
                PermissionCodes.DashboardRead,
                PermissionCodes.ClientsRead,
                PermissionCodes.ClientsWrite,
                PermissionCodes.ProjectsRead,
                PermissionCodes.ProjectsWrite,
                PermissionCodes.TasksRead,
                PermissionCodes.TasksWrite,
                PermissionCodes.FinancesRead,
                PermissionCodes.FinancesWrite,
                PermissionCodes.AdministrationRead,
                PermissionCodes.AdministrationWrite
            ],
            RoleNames.Operative =>
            [
                PermissionCodes.DashboardRead,
                PermissionCodes.ClientsRead,
                PermissionCodes.ClientsWrite,
                PermissionCodes.ProjectsRead,
                PermissionCodes.ProjectsWrite,
                PermissionCodes.TasksRead,
                PermissionCodes.TasksWrite,
                PermissionCodes.FinancesRead
            ],
            RoleNames.ReadOnly =>
            [
                PermissionCodes.DashboardRead,
                PermissionCodes.ClientsRead,
                PermissionCodes.ProjectsRead,
                PermissionCodes.TasksRead,
                PermissionCodes.FinancesRead,
                PermissionCodes.AdministrationRead
            ],
            _ => Array.Empty<string>()
        };
    }
}
