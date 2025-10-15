using MongoDB.Driver;
using Org.BouncyCastle.Crypto.Generators;
using SocialNetwork.BLL.Abstract;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Abstract;
using SocialNetwork.DAL.Concrete;
using System.Text.RegularExpressions;



namespace SocialNetwork.BLL.Concrete
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
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
        public void AddOrRemoveFriends(User currentUser, User otherUser)
        {
            if (currentUser.Friends.Contains(otherUser.Id))
            {
                Console.WriteLine($"{otherUser.FirstName} {otherUser.LastName} is already a friend.");
                Console.Write("If you wont to delete from friend press (d):");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    return;
                }

                else if (input.Equals("d", StringComparison.OrdinalIgnoreCase))
                {
                    currentUser.Friends.Remove(otherUser.Id);
                    otherUser.Friends.Remove(currentUser.Id);
                    _userRepository.UpdateAsync(currentUser).Wait();
                    _userRepository.UpdateAsync(otherUser).Wait();
                    Console.WriteLine($"{otherUser.FirstName} {otherUser.LastName} has been removed from friends.");
                }
                else
                    return;
                

            }
            else
            {
                currentUser.Friends.Add(otherUser.Id);
                otherUser.Friends.Add(currentUser.Id);
                _userRepository.UpdateAsync(currentUser).Wait();
                _userRepository.UpdateAsync(otherUser).Wait();
                Console.WriteLine($"{otherUser.FirstName} {otherUser.LastName} has been added to friends.");
            }
        }
        public void FollowOrUnfollow(User currentUser, User otherUser)
        {
            if (currentUser.Following.Contains(otherUser.Id))
            {
                Console.WriteLine($"{otherUser.FirstName} {otherUser.LastName} is already being followed.");
                Console.Write("If you want to unfollow press (u):");
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    return;
                }
                
                else if (input.Equals("u", StringComparison.OrdinalIgnoreCase))
                {
                    currentUser.Following.Remove(otherUser.Id);
                    otherUser.Followers.Remove(currentUser.Id);
                    _userRepository.UpdateAsync(currentUser).Wait();
                    _userRepository.UpdateAsync(otherUser).Wait();
                    Console.WriteLine($"\nYou have unfollowed {otherUser.FirstName} {otherUser.LastName}.");
                }
                else
                    return;
            }
            else
            {
                currentUser.Following.Add(otherUser.Id);
                otherUser.Followers.Add(currentUser.Id);
                _userRepository.UpdateAsync(currentUser).Wait();
                _userRepository.UpdateAsync(otherUser).Wait();
                Console.WriteLine($"You are now following {otherUser.FirstName} {otherUser.LastName}.");
            }
        }
        public async Task<User?> AuthenticateUserAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isPasswordValid ? user : null;
        }
        public async Task<bool> ValidateUserInput(string? firstname, string? lastname, string? email, string? password, string? passwordConfirm)
        {
            if (string.IsNullOrWhiteSpace(firstname))
            {
                Console.WriteLine("Error: Firstname is null.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(lastname))
            {
                Console.WriteLine("Error: Lastname is null.");
                return false;
            }
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(email))
            {
                Console.WriteLine("Error: Incorrect input (example@gmail.com).");
                return false;
            }
            if (await _userRepository.UserExistsAsync(email))
            {
                Console.WriteLine("Error: This email is already exists.");
                return false;
            }

            if (password.Length < 8)
            {
                Console.WriteLine("Error: Password must contain at least 8 characters.");
                return false;
            }

            if (password != passwordConfirm)
            {
                Console.WriteLine("Error: The passwords do not match.");
                return false;
            }
            return true;
        }
        public async Task CreateUserAsync(User newUser)
        {
            await _userRepository.CreateAsync(newUser);
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }


    }
}
