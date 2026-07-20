using ArashBlog.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Navigation;

public record NavItemDto(string Key, string Title, string Path);

[ApiController]
[Route("api/nav")]
public class NavController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NavItemDto>>> Get([FromQuery] string? lang)
    {
        var l = lang == "ckb" ? "ckb" : "fa";
        var items = await db.NavItems.Where(n => n.IsVisible).OrderBy(n => n.Order).ToListAsync();
        return Ok(items.Select(n => new NavItemDto(n.Key, l == "ckb" ? n.TitleCkb : n.TitleFa, n.Path)).ToList());
    }
}
