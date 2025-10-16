using SocialNetwork.Core.Models;

namespace SocialNetwork.BLL.Abstract
{
    public interface IPostService
    {
        Task ShowUserPosts(User user);
        Task CreatePost(string email);
        Task AddCommentToPost(User currentUser, User otherUser);
        Task<Post> ReactOrRemoveToPost(User currentUser, User otherUser);
        Task SteamPosts();
    }
}
