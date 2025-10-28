using MongoDB.Driver;
using Org.BouncyCastle.Crypto.Generators;
using SocialNetwork.BLL.Abstract;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.MongoDb.Abstract;
using SocialNetwork.DAL.Neo4j.Abstract;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace SocialNetwork.BLL.Concrete
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly INodesRepository _nodesRepository;
        private readonly IRelationshipsRepository _relationshipsRepository;
        public UserService(IUserRepository userRepository, INodesRepository nodesRepository, IRelationshipsRepository relationshipsRepository)
        {
            _userRepository = userRepository;
            _nodesRepository = nodesRepository;
            _relationshipsRepository = relationshipsRepository;
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
        public async Task AddOrRemoveFriends(User currentUser, User otherUser)
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

                    await _userRepository.UpdateAsync(currentUser);
                    await _userRepository.UpdateAsync(otherUser);

                    await _relationshipsRepository.DeleteRelationshipAsync(currentUser.Id, otherUser.Id, "FRIEND");
                    await _relationshipsRepository.DeleteRelationshipAsync(otherUser.Id, currentUser.Id, "FRIEND");

                    Console.WriteLine($"{otherUser.FirstName} {otherUser.LastName} has been removed from friends.");
                }
                else
                    return;
                

            }
            else
            {
                currentUser.Friends.Add(otherUser.Id);
                otherUser.Friends.Add(currentUser.Id);

                await _userRepository.UpdateAsync(currentUser);
                await _userRepository.UpdateAsync(otherUser);

                await _relationshipsRepository.CreateRelationshipAsync(currentUser.Id, otherUser.Id, "FRIEND");
                await _relationshipsRepository.CreateRelationshipAsync(otherUser.Id, currentUser.Id, "FRIEND");

                Console.WriteLine($"{otherUser.FirstName} {otherUser.LastName} has been added to friends.");
            }
        }
        public async Task FollowOrUnfollow(User currentUser, User otherUser)
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

                    await _userRepository.UpdateAsync(currentUser);
                    await _userRepository.UpdateAsync(otherUser);

                    await _relationshipsRepository.DeleteRelationshipAsync(currentUser.Id, otherUser.Id, "FOLLOW");
             
                    Console.WriteLine($"\nYou have unfollowed {otherUser.FirstName} {otherUser.LastName}.");
                }
                else
                    return;
            }
            else
            {
                currentUser.Following.Add(otherUser.Id);
                otherUser.Followers.Add(currentUser.Id);

                await _userRepository.UpdateAsync(currentUser);
                await _userRepository.UpdateAsync(otherUser);

                await _relationshipsRepository.CreateRelationshipAsync(currentUser.Id, otherUser.Id, "FOLLOW");

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
            await _nodesRepository.CreateNodeUserAsync(newUser);

        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task DeleteUserAsync(User currentUser)
        {
            await _userRepository.DeleteAsync(currentUser.Id);
            await _nodesRepository.DeleteNodeAsync(currentUser.Id);
        }

        public async Task UpdateUserProfileAsync(string email)
        {
            var newUser = await _userRepository.GetUserByEmailAsync(email);
            if (newUser == null)
            {
                Console.WriteLine("User not found.");
                return;
            }
            Console.Write("Enter new first name (leave blank to keep current): ");
            var newFirstName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newFirstName))
            {
                newUser.FirstName = newFirstName;
            }
            Console.Write("Enter new last name (leave blank to keep current): ");
            var newLastName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newLastName))
            {
                newUser.LastName = newLastName;
            }
            Console.Write("Enter new interests (comma separated, leave blank to keep current): ");
            var newInterestsInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newInterestsInput))
            {
                var newInterests = newInterestsInput.Split(',').Select(i => i.Trim()).ToList();
                newUser.Interests = newInterests;
            }
            await _userRepository.UpdateAsync(newUser);
            await _nodesRepository.UpdateNodeAsync(newUser);
        }

        public async Task UserShortestPath(User currentUser, User otherUser)
        {
            var pathLength = await _relationshipsRepository.ShortestPath(currentUser.Id, otherUser.Id, "FRIEND");

            if (pathLength == 0)
            {
                Console.WriteLine($"\nNo FRIEND path found between {currentUser.FirstName} and {otherUser.FirstName}.");
            }
            else
            {
                Console.WriteLine($"\nThe shortest FRIEND path between {currentUser.FirstName} and {otherUser.FirstName} is {pathLength} connections.");

            }
        }
    }
}
