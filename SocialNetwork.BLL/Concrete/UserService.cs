
using SocialNetwork.BLL.Abstract;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Abstract;
using SocialNetwork.DAL.Concrete;

namespace SocialNetwork.BLL.Concrete
{
    public class UserService : IUserService
    {
        public void UserInfo(User user)
        {
            Console.WriteLine($"\nUser: {user.FirstName} {user.LastName}");
            Console.WriteLine($"Email: {user.Email}");
            if (user.Interests != null && user.Interests.Count > 0)
            {
                Console.WriteLine("Interests: " + string.Join(", ", user.Interests));
            }
            else
            {
                Console.WriteLine("Interests: No interests specified.");
            }
            Console.WriteLine($"Friends count: {user.Friends.Count}");
            Console.WriteLine($"Followers count: {user.Followers.Count}");
            Console.WriteLine($"Following count: {user.Following.Count}");
        }
        public void AddToFriends(User user, User friend, UserRepository userRepo)
        {
            if (user.Friends.Contains(friend.Id))
            {
                Console.WriteLine($"{friend.FirstName} {friend.LastName} is already a friend.");
                return;
            }
            user.Friends.Add(friend.Id);
            friend.Friends.Add(user.Id);
            userRepo.UpdateAsync(user).Wait();
            userRepo.UpdateAsync(friend).Wait();
            Console.WriteLine($"{friend.FirstName} {friend.LastName} has been added to friends.");
        }

    }
}
