using System.Security.Claims;

namespace BackendTechnicalEquipmentBorrowingSystem.Utils;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static bool IsPrivileged(this ClaimsPrincipal user)
        => user.IsInRole("SuperAdmin") || user.IsInRole("Admin");
}
