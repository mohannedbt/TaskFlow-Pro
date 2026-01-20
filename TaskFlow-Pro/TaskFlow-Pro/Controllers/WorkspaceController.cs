using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow_Pro.Models;
using TaskFlow_Pro.Services.Implementations;

namespace TaskFlow_Pro.Controllers
{
    public class WorkspaceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly StartUpService _startUpService;

        public WorkspaceController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            StartUpService startUpService)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _startUpService = startUpService;
        }

        // =========================================================
        // WORKSPACE SETUP (GET) - anonymous (comes from Register Owner)
        // =========================================================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Setup()
        {
            if (TempData["pending_email"] == null ||
                TempData["pending_password"] == null ||
                TempData["pending_username"] == null)
                return RedirectToAction("Register", "Account");

            TempData.Keep("pending_username");
            TempData.Keep("pending_email");
            TempData.Keep("pending_password");

            return View(new WorkspaceViewModel
            {
                MaxMembers = 50,
                EmailRule = "@example.com" // ✅ domain rule, NOT regex
            });
        }

        // =========================================================
        // WORKSPACE SETUP (POST) - anonymous (creates workspace + owner)
        // =========================================================
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(WorkspaceViewModel vm)
        {
            var username = (TempData["pending_username"] as string)?.Trim();
            var email = (TempData["pending_email"] as string)?.Trim();
            var password = TempData["pending_password"] as string;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
                return RedirectToAction("Register", "Account");

            vm.EmailRule = (vm.EmailRule ?? "").Trim();

            var domain = StartUpService.NormalizeDomain(vm.EmailRule); // "@x.com" => "x.com"

            // basic validation
            if (string.IsNullOrWhiteSpace(domain) || !domain.Contains('.') || domain.Contains(' ') || domain.Contains('@'))
            {
                ModelState.AddModelError(nameof(vm.EmailRule), "Please enter a valid domain like @x.com");
                TempData.Keep("pending_username");
                TempData.Keep("pending_email");
                TempData.Keep("pending_password");
                return View(vm);
            }

            // Owner email must match domain
            if (!StartUpService.EmailMatchesDomain(email, domain))
            {
                ModelState.AddModelError(nameof(vm.EmailRule), $"Your account email '{email}' must end with @{domain}");
                TempData.Keep("pending_username");
                TempData.Keep("pending_email");
                TempData.Keep("pending_password");
                return View(vm);
            }

            // Create workspace
            var workspace = new Workspace
            {
                Name = vm.WorkspaceName.Trim(),
                MaxMembers = vm.MaxMembers,
                EmailPattern = domain // store domain only
            };

            _db.Workspaces.Add(workspace);
            await _db.SaveChangesAsync();

            // Create owner user
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                WorkspaceId = workspace.Id
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                TempData.Keep("pending_username");
                TempData.Keep("pending_email");
                TempData.Keep("pending_password");
                return View(vm);
            }

            await _userManager.AddToRoleAsync(user, "Owner");
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        // =========================================================
        // WORKSPACE INDEX - Admin/Owner only
        // =========================================================
        [Authorize(Roles = "Owner,Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Unauthorized();
            if (me.WorkspaceId == null) return Forbid();

            var ws = await _db.Workspaces.FirstOrDefaultAsync(w => w.Id == me.WorkspaceId);
            if (ws == null) return NotFound("Workspace not found.");

            var users = await _db.Users
                .Where(u => u.WorkspaceId == ws.Id)
                .ToListAsync();

            var teams = await _db.Teams
                .Where(t => t.WorkspaceId == ws.Id)
                .ToListAsync();

            var membersVm = new List<MemberVm>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var role = roles.FirstOrDefault();
                var teamName = teams.FirstOrDefault(t => t.Id == u.TeamId)?.Name;

                membersVm.Add(new MemberVm
                {
                    UserId = u.Id,
                    Email = u.Email ?? "",
                    Username = u.UserName ?? "",
                    Role = role,
                    TeamId = u.TeamId,
                    TeamName = teamName
                });
            }

            var teamsVm = teams.Select(t => new TeamWithMembersVm
            {
                TeamId = t.Id,
                Name = t.Name,
                Description = t.Description,
                Members = membersVm.Where(m => m.TeamId == t.Id).ToList()
            }).ToList();

            var vm = new WorkspaceIndexViewModel
            {
                WorkspaceId = ws.Id,
                WorkspaceName = ws.Name,
                MaxMembers = ws.MaxMembers,
                EmailDomain = ws.EmailPattern, // domain
                Members = membersVm.OrderBy(m => m.Username).ToList(),
                Teams = teamsVm.OrderBy(t => t.Name).ToList(),
                MemberCount = membersVm.Count
            };

            return View(vm);
        }

        // =========================================================
        // UPDATE WORKSPACE - Owner only
        // =========================================================
        [Authorize(Roles = "Owner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWorkspace(WorkspaceIndexViewModel vm)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Unauthorized();
            if (me.WorkspaceId == null) return Forbid();

            var ws = await _db.Workspaces.FirstOrDefaultAsync(w => w.Id == me.WorkspaceId);
            if (ws == null) return NotFound();

            var name = (vm.WorkspaceName ?? "").Trim();
            var domain = StartUpService.NormalizeDomain(vm.EmailDomain); // allow "@x.com" or "x.com"

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Workspace name is required.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(domain) || !domain.Contains('.') || domain.Contains(' ') || domain.Contains('@'))
            {
                TempData["Error"] = "Email domain must look like: @company.com";
                return RedirectToAction(nameof(Index));
            }

            ws.Name = name;
            ws.MaxMembers = vm.MaxMembers;
            ws.EmailPattern = domain;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Workspace updated.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // CREATE TEAM - Admin/Owner
        // =========================================================
        [Authorize(Roles = "Owner,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeam(WorkspaceIndexViewModel vm)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Unauthorized();
            if (me.WorkspaceId == null) return Forbid();

            var name = (vm.NewTeamName ?? "").Trim();
            var desc = (vm.NewTeamDescription ?? "").Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Team name is required.";
                return RedirectToAction(nameof(Index));
            }

            var exists = await _db.Teams.AnyAsync(t => t.WorkspaceId == me.WorkspaceId && t.Name == name);
            if (exists)
            {
                TempData["Error"] = "A team with that name already exists.";
                return RedirectToAction(nameof(Index));
            }

            _db.Teams.Add(new Team
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(desc) ? null : desc,
                WorkspaceId = me.WorkspaceId
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Team created.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // VERIFY INVITE - anonymous (domain-only policy)
        // =========================================================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> VerifyInvite(string code, string? email = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest();

            var invite = await _db.WorkspaceInvites
                .Include(i => i.Workspace)
                .FirstOrDefaultAsync(i => i.Code == code);

            if (invite == null || invite.Used || invite.ExpiresAt < DateTime.UtcNow)
                return View("InviteStatus", new InviteStatusVm(false, "Invalid or expired invite."));

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim();

                if (!string.IsNullOrWhiteSpace(invite.Email) &&
                    !string.Equals(invite.Email, email, StringComparison.OrdinalIgnoreCase))
                    return View("InviteStatus", new InviteStatusVm(false, "Invite email mismatch."));

                // ✅ domain-only check (workspace.EmailPattern stores "x.com")
                if (!StartUpService.EmailMatchesDomain(email, invite.Workspace.EmailPattern))
                    return View("InviteStatus", new InviteStatusVm(false, "Email does not match workspace policy."));

                var members = await _db.Users.CountAsync(u => u.WorkspaceId == invite.WorkspaceId);
                if (members >= invite.Workspace.MaxMembers)
                    return View("InviteStatus", new InviteStatusVm(false, "Workspace is full."));
            }

            return View("InviteStatus", new InviteStatusVm(true, "Invite is valid.")
            {
                WorkspaceName = invite.Workspace.Name,
                RoleToGrant = invite.RoleToGrant
            });
        }
        
[Authorize(Roles="Owner,Admin")]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateInvite(string roleToGrant, string? email = null, int days = 7)
{
    var me = await _userManager.GetUserAsync(User);
    if (me == null) return Unauthorized();
    if (me.WorkspaceId == null) return Forbid();

    roleToGrant = (roleToGrant ?? "").Trim();
    if (!string.Equals(roleToGrant, "Admin", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(roleToGrant, "Member", StringComparison.OrdinalIgnoreCase))
    {
        TempData["Error"] = "Role must be Admin or Member.";
        return RedirectToAction(nameof(Index));
    }

    // optional: if email is provided, enforce domain policy
    if (!string.IsNullOrWhiteSpace(email))
    {
        email = email.Trim();
        var ws = await _db.Workspaces.FirstAsync(w => w.Id == me.WorkspaceId);
        if (!StartUpService.EmailMatchesDomain(email, ws.EmailPattern))
        {
            TempData["Error"] = $"Email must end with @{ws.EmailPattern}";
            return RedirectToAction(nameof(Index));
        }
    }

    var code = GenerateInviteCode(24);

    var invite = new WorkspaceInvite
    {
        WorkspaceId = me.WorkspaceId,
        Code = code,
        Email = string.IsNullOrWhiteSpace(email) ? null : email,
        RoleToGrant = roleToGrant.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Member",
        ExpiresAt = DateTime.UtcNow.AddDays(Math.Clamp(days, 1, 30))
    };

    _db.WorkspaceInvites.Add(invite);
    await _db.SaveChangesAsync();

    // ✅ build the URL: /Account/Register?code=XXXX
    var link = Url.Action("Register", "Account", new { code = invite.Code }, Request.Scheme);

    TempData["Success"] = $"Invite created: {link}";
    return RedirectToAction(nameof(Index));
}

private static string GenerateInviteCode(int length)
{
    // URL-safe random
    var bytes = RandomNumberGenerator.GetBytes(length);
    var s = Convert.ToBase64String(bytes)
        .Replace("+", "-")
        .Replace("/", "_")
        .Replace("=", "");
    return s.Length > length ? s.Substring(0, length) : s;
}
    }

    public record InviteStatusVm(bool Ok, string Message)
    {
        public string? WorkspaceName { get; init; }
        public string? RoleToGrant { get; init; }
    }
}
