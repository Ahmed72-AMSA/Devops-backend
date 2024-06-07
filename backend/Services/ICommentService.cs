
using developers.Models;

namespace developers.Services
{
    public interface ICommentService
    {
        Task<bool> AddCommentAsync(Comments comment);
        Task<Comments> GetCommentByIdAsync(int id);
        Task<bool> DeleteCommentAsync(int commentId);

        Task<List<Comments>> GetCommentsByUserEmailAsync(string userEmail);
        
    }
}
