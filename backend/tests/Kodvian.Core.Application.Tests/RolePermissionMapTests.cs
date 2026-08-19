using Kodvian.Core.Application.Common.Security;

namespace Kodvian.Core.Application.Tests;

public class RolePermissionMapTests
{
    [Fact]
    public void DeveloperRoleOnlyReceivesMyWorkPermissions()
    {
        var permissions = RolePermissionMap.GetPermissions(RoleNames.Developer);

        Assert.Contains(PermissionCodes.DeveloperWorkRead, permissions);
        Assert.Contains(PermissionCodes.DeveloperTasksStatusWrite, permissions);
        Assert.DoesNotContain(PermissionCodes.ProjectsRead, permissions);
        Assert.DoesNotContain(PermissionCodes.TasksWrite, permissions);
        Assert.DoesNotContain(PermissionCodes.FinancesRead, permissions);
        Assert.Equal(2, permissions.Count);
    }
}
