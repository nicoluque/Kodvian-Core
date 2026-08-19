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
                PermissionCodes.ProjectsDocumentsRead,
                PermissionCodes.ProjectsDocumentsWrite,
                PermissionCodes.ProjectsDocumentsDelete,
                PermissionCodes.TasksRead,
                PermissionCodes.TasksWrite,
                PermissionCodes.TeamRead,
                PermissionCodes.TeamWrite,
                PermissionCodes.FinancesRead,
                PermissionCodes.FinancesWrite,
                PermissionCodes.AdministrationRead,
                PermissionCodes.AdministrationWrite
            ],
            RoleNames.Operative =>
            [
                PermissionCodes.ClientsRead,
                PermissionCodes.ClientsWrite,
                PermissionCodes.ProjectsRead,
                PermissionCodes.ProjectsWrite,
                PermissionCodes.ProjectsDocumentsRead,
                PermissionCodes.ProjectsDocumentsWrite,
                PermissionCodes.ProjectsDocumentsDelete,
                PermissionCodes.TasksRead,
                PermissionCodes.TasksWrite,
                PermissionCodes.TeamRead,
                PermissionCodes.TeamWrite
            ],
            RoleNames.ReadOnly =>
            [
                PermissionCodes.ClientsRead,
                PermissionCodes.ProjectsRead,
                PermissionCodes.ProjectsDocumentsRead,
                PermissionCodes.TasksRead,
                PermissionCodes.TeamRead,
                PermissionCodes.AdministrationRead
            ],
            RoleNames.Analyst =>
            [
                PermissionCodes.ClientsRead,
                PermissionCodes.ClientsWrite,
                PermissionCodes.ProjectsRead,
                PermissionCodes.ProjectsWrite,
                PermissionCodes.ProjectsDocumentsRead,
                PermissionCodes.ProjectsDocumentsWrite,
                PermissionCodes.ProjectsDocumentsDelete,
                PermissionCodes.TasksRead,
                PermissionCodes.TasksWrite,
                PermissionCodes.TeamRead,
                PermissionCodes.TeamWrite
            ],
            RoleNames.Developer =>
            [
                PermissionCodes.DeveloperWorkRead,
                PermissionCodes.DeveloperTasksStatusWrite
            ],
            _ => Array.Empty<string>()
        };
    }
}
