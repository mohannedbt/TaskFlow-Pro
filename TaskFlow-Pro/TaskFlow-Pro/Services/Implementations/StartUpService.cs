using Microsoft.AspNetCore.Identity;

namespace TaskFlow_Pro.Services.Implementations;
using System.Text.RegularExpressions;

public class StartUpService
{
    public static async Task SeedRolesAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Owner", "Admin", "Member" };
        foreach (var r in roles)
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
    }

    public static string NormalizeDomain(string emailRule)
    {
        emailRule = emailRule.Trim();

        // accept "@x.com" or "x.com"
        if (emailRule.StartsWith("@")) emailRule = emailRule[1..];

        return emailRule.ToLowerInvariant();
    }

    public static string GetDomainFromEmail(string email)
    {
        var at = email.LastIndexOf('@');
        if (at < 0 || at == email.Length - 1) return "";
        return email[(at + 1)..].ToLowerInvariant();
    }

    public static bool EmailMatchesDomain(string email, string domain)
    {
        return GetDomainFromEmail(email) == domain.ToLowerInvariant();
    }


}