using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDom.Api.Controllers;

[ApiController]
[Route("api/utility")]
[Authorize]
public class UtilityController : ControllerBase
{
    /// <summary>Returns the current server date as ISO-8601 string (yyyy-MM-dd).</summary>
    [HttpGet("data-corrente")]
    public IActionResult GetDataCorrente()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return Ok(new { data = today.ToString("yyyy-MM-dd") });
    }
}
