using System.Linq;
using System.Threading.Tasks;
using developers.Data;
using developers.Models;
using Microsoft.EntityFrameworkCore;

namespace developers.Services
{
    public class CommentService : ICommentService
    {
        private readonly IDataRepository<Comments> _commentRepository;
        private readonly IDataRepository<TaskDeveloper> _taskDeveloperRepository;

        private readonly IDataRepository<User> _userRepository;

        public CommentService(IDataRepository<Comments> commentRepository, IDataRepository<TaskDeveloper> taskDeveloperRepository , IDataRepository<User> userRepository)
        {
            _commentRepository = commentRepository;
            _taskDeveloperRepository = taskDeveloperRepository;
            _userRepository = userRepository; 
        }

public async Task<bool> AddCommentAsync(Comments comment)
{
    var user = await _userRepository.GetByIdAsync(comment.UserId);
    
    if (user == null || (user.Role != "User" && user.Role != "Admin"))
    {
    }

    if (user.Role != "User")
    {
        var newCommentUser = new Comments
        {
            commentBody = comment.commentBody,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            userEmail = comment.userEmail
        };

        await _commentRepository.AddAsync(newCommentUser);
        return await _commentRepository.Save();
    }

    // Check if the user has a relation with the provided task ID
    var userTaskRelation = await _taskDeveloperRepository.GetContext().TaskDevelopers
        .FirstOrDefaultAsync(td => td.TaskId == comment.TaskId && td.DeveloperId == comment.UserId);

    // Check if the user has the task assigned to them
    if (userTaskRelation == null)
    {
        return false; // Task is not assigned to the user
    }

    var newComment = new Comments
    {
        commentBody = comment.commentBody,
        TaskId = comment.TaskId,
        UserId = comment.UserId,
        userEmail = comment.userEmail
    };

    await _commentRepository.AddAsync(newComment);
    return await _commentRepository.Save();
}

public async Task<Comments> GetCommentByIdAsync(int id)
{
            return await _commentRepository.GetByIdAsync(id);
        
        
}


public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var commentToDelete = await _commentRepository.GetByIdAsync(commentId);
            if (commentToDelete == null)
            {
                return false; // Comment not found
            }

            _commentRepository.DeleteAsync(commentToDelete);
            return await _commentRepository.Save();
        }
        

        public async Task<List<Comments>> GetCommentsByUserEmailAsync(string userEmail)
        {
            return await _commentRepository.GetContext().Comments
                .Where(c => c.userEmail == userEmail)
                .ToListAsync();
        }

    }



        
    }

