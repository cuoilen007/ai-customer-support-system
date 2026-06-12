using System.Security.Claims;

namespace AI.CustomerSupport.API.Helpers
{
    public static class ClaimsHelper
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            return int.Parse(
                user.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);
        }
    }
}
