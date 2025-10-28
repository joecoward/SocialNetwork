using SocialNetwork.Core.Models;

namespace SocialNetwork.BLL.Abstract
{
    public interface IUserService
    {
        public void UserInfo(User user);
        Task AddOrRemoveFriends(User currentUser, User otherUser);
        Task FollowOrUnfollow(User currentUser, User otherUser);
        Task<User?> AuthenticateUserAsync(string email, string password);
        Task<bool> ValidateUserInput(string? firstname, string? lastname, string? email, string? password, string? passwordConfirm);
        Task<User> GetUserByEmailAsync(string email);
        Task CreateUserAsync(User newUser);
        Task DeleteUserAsync(User currentUser);
        Task UpdateUserProfileAsync(string email);
        Task UserShortestPath(User currentUser, User otherUser);
    }
}
