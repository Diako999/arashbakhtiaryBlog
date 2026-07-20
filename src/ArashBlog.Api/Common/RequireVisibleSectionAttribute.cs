using ArashBlog.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Common;

// The view-level half of the phased-rollout mechanism, ported from the
// Django project's SectionVisibleRequiredMixin. 404s the request unless
// the matching NavItem.IsVisible is true — the same flag the public nav
// listing reads to decide whether to show the link at all, so "nav
// hidden" and "URL blocked" can't drift out of sync the way two separate
// flags could.
public class RequireVisibleSectionAttribute : TypeFilterAttribute
{
    public RequireVisibleSectionAttribute(string key) : base(typeof(RequireVisibleSectionFilter))
    {
        Arguments = [key];
    }
}

public class RequireVisibleSectionFilter(string key, ApplicationDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var isVisible = await db.NavItems.AnyAsync(n => n.Key == key && n.IsVisible);
        if (!isVisible)
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}
