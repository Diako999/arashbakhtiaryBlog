using ArashBlog.Api.Common;
using ArashBlog.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArashBlog.Api.Features.Dashboard;

// Mirrors apps/dashboard/views.py's CommentDashboardListView/
// toggle_comment_approved/CommentDeleteView — pending comments first
// (is_approved ascending), newest first within each group.
[ApiController]
[Route("api/dashboard/comments")]
[RequireVerifiedTwoFactor]
public class CommentsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DashboardCommentDto>>> List()
    {
        var comments = await db.Comments
            .Include(c => c.Post)
            .OrderBy(c => c.IsApproved)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments.Select(c => new DashboardCommentDto(
            c.Id, c.PostId, c.Post.TitleFa, c.Name, c.Email, c.Body, c.IsApproved, c.CreatedAt)).ToList());
    }

    [HttpPost("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment is null) return NotFound();

        comment.IsApproved = !comment.IsApproved;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment is null) return NotFound();

        db.Comments.Remove(comment);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
