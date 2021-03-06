using System.Collections.Generic;
using System.Threading.Tasks;
using StudioX.Authorization;
using StudioX.Zero.SampleApp.Roles;
using Shouldly;
using Xunit;

namespace StudioX.Zero.SampleApp.Tests.Roles
{
    public class RoleManagerTests : SampleAppTestBase
    {
        [Fact]
        public async Task ShouldCreateAndRetrieveRole()
        {
            await CreateRole("Role1");

            var role1Retrieved = await RoleManager.FindByNameAsync("Role1");
            role1Retrieved.ShouldNotBe(null);
            role1Retrieved.Name.ShouldBe("Role1");
        }

        [Fact]
        public async Task ShouldNotCreateForDuplicateNameOrDisplayName()
        {
            //Create a role and check
            await CreateRole("Role1", "Role One");
            (await RoleManager.FindByNameAsync("Role1")).ShouldNotBe(null);

            //Create with same name
            (await RoleManager.CreateAsync(new Role(null, "Role1", "Role Uno"))).Succeeded.ShouldBe(false);
            (await RoleManager.CreateAsync(new Role(null, "Role2", "Role One"))).Succeeded.ShouldBe(false);
        }

        [Fact]
        public async Task PermissionTests()
        {
            var role1 = await CreateRole("Role1");

            (await RoleManager.IsGrantedAsync(role1.Id, PermissionManager.GetPermission("Permission1"))).ShouldBe(false);
            (await RoleManager.IsGrantedAsync(role1.Id, PermissionManager.GetPermission("Permission3"))).ShouldBe(false);

            await GrantPermissionAsync(role1, "Permission1");
            await ProhibitPermissionAsync(role1, "Permission1");
            await ProhibitPermissionAsync(role1, "Permission3");
            await GrantPermissionAsync(role1, "Permission3");
            await GrantPermissionAsync(role1, "Permission1");
            await ProhibitPermissionAsync(role1, "Permission3");

            var grantedPermissions = await RoleManager.GetGrantedPermissionsAsync(role1);
            grantedPermissions.Count.ShouldBe(1);
            grantedPermissions.ShouldContain(p => p.Name == "Permission1");

            var newPermissionList = new List<Permission>
                                    {
                                        PermissionManager.GetPermission("Permission1"),
                                        PermissionManager.GetPermission("Permission2"),
                                        PermissionManager.GetPermission("Permission3")
                                    };

            await RoleManager.SetGrantedPermissionsAsync(role1, newPermissionList);

            grantedPermissions = await RoleManager.GetGrantedPermissionsAsync(role1);

            grantedPermissions.Count.ShouldBe(3);
            grantedPermissions.ShouldContain(p => p.Name == "Permission1");
            grantedPermissions.ShouldContain(p => p.Name == "Permission2");
            grantedPermissions.ShouldContain(p => p.Name == "Permission3");
        }
    }
}