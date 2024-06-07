using developers.Models;
using developers.Services;
using Microsoft.AspNetCore.Mvc;



[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Comments>> GetComment(int id)
    {
        var comment = await _commentService.GetCommentByIdAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        return comment;
    }


    [HttpPost]
    public async Task<ActionResult<Comments>> PostComment(Comments comment)
    {
        var success = await _commentService.AddCommentAsync(comment);
        if (!success)
        {
            return BadRequest("You are not assigned to this task.");
        }

        return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
    }




    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var success = await _commentService.DeleteCommentAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }


[HttpGet("user/{userEmail}")]
    public async Task<ActionResult<List<Comments>>> GetCommentsByUserEmail(string userEmail)
    {
        var comments = await _commentService.GetCommentsByUserEmailAsync(userEmail);
        if (comments == null || comments.Count == 0)
        {
            return NotFound("No comments found for the specified user email.");
        }

        return comments;
    }

}