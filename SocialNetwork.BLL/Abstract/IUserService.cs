using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Concrete;

namespace SocialNetwork.BLL.Abstract
{
    public interface IUserService
    {
        public void UserInfo(User user);
        public void AddOrRemoveFriends(User currentUser, User otherUser);
        public void FollowOrUnfollow(User currentUser, User otherUser);
        Task<User?> AuthenticateUserAsync(string email, string password);
        Task<bool> ValidateUserInput(string? firstname, string? lastname, string? email, string? password, string? passwordConfirm);
        Task<User> GetUserByEmailAsync(string email);
        Task CreateUserAsync(User newUser);
    }
}
