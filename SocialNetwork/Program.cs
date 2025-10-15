using MongoDB.Driver;
using SocialNetwork.BLL.Abstract;
using SocialNetwork.BLL.Concrete;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Abstract;
using SocialNetwork.DAL.Concrete;
using SocialNetwork.DAL.MgContext;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SocialNetwork
{
    public class Program
    {
        private static IUserService _userService;
        private static IPostService _postService;
        private static User _currentUser;
        static async Task Main(string[] args)
        {
            string connectionString = "mongodb://localhost:27017";
            string databaseName = "SocialNetworkDB";
            var context = new MongoDbContext(connectionString, databaseName);

            var userRepo = new UserRepository(context);
            var postRepo = new PostRepository(context);

            await userRepo.CreateEmailIndexAsync();

            _userService = new UserService(userRepo);
            _postService = new PostService(postRepo, userRepo);

            //pass 12341234 myemail@gmail.com

            Console.WriteLine("Welcome to my SocialNetwork");

            while (true)
            {
                if (_currentUser == null)
                    await ShowMainMenu();
                else
                    await ShowUserMenu();
            }

        }
        private static async Task ShowMainMenu()
        {
                Console.WriteLine("\nSelect option");
                Console.WriteLine("r - Register");
                Console.WriteLine("l - Login");
                Console.WriteLine("q - Quit");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) return;
                var c = char.ToLowerInvariant(line[0]);
                switch (c)
                {
                    case 'q':
                        Environment.Exit(0);
                        break;
                    case 'r':
                        await Registration();
                        break;
                    case 'l':
                        await Login();

                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            
        }
        private static async Task Login()
        {
            Console.WriteLine("\nLogining in");
            Console.Write("Enter your email:");
            var email = Console.ReadLine();
            Console.Write("Enter your password:");
            var password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Error: Email or password is null.");
                return;
            }
            var user = await _userService.AuthenticateUserAsync(email, password);

            if (user != null)
            {
                _currentUser = user;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Login successful! Welcome, {_currentUser.FirstName}!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Invalid email or password.");
                Console.ResetColor();
            }
        }
        private static async Task Registration()
        {

            Console.WriteLine("\nCreate new profile");

            Console.Write("Enter your firstname: ");
            var firstname = Console.ReadLine();

            Console.Write("Enter yout lastname:");
            var lastname = Console.ReadLine();

            Console.Write("Enter your email:");
            var email = Console.ReadLine();

            Console.Write("Enter your password:");
            var password = Console.ReadLine();

            Console.Write("Confirm password:");
            var passwordConfirm = Console.ReadLine();

            Console.Write("Enter your interests (optional)");
            var interests = new List<string>();
            while (true)
            {
                Console.WriteLine("Enter interest or leave empty to finish:");
                var interest = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(interest)) break;
                interests.Add(interest);
            }


            if (await _userService.ValidateUserInput(firstname, lastname, email, password, passwordConfirm))
            {
                try
                {
                    var newUser = new User
                    {
                        FirstName = firstname,
                        LastName = lastname,
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        Interests = interests
                    };

                    await _userService.CreateUserAsync(newUser);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Congrats!!! You are registered ");
                    Console.ResetColor();
                }
                catch (MongoWriteException mwx) when (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: User with this email '{email}' is already exists.");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
        private static async Task ShowUserMenu()
        {
            Console.WriteLine($"\n--- Logged in as {_currentUser.Email} ---");
            while (true)
            {
                Console.WriteLine("\nSelect option");
                Console.WriteLine("1 - View My Profile & Posts");
                Console.WriteLine("2 - Create Post");
                Console.WriteLine("3 - Search other users");
                Console.WriteLine("q - Logout");

                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var c = char.ToLowerInvariant(line[0]);

                switch (c)
                {
                    case 'q':
                        _currentUser = null;
                        Console.WriteLine("You have been logged out.");
                        return;
                    case '1':
                         _userService.UserInfo(_currentUser);
                         await _postService.ShowUserPosts(_currentUser);
                         await PostsMenu(_currentUser);
                        break;
                    case '2':
                        await _postService.CreatePost(_currentUser.Email);
                        break;
                    case '3':
                        await SearchOtherUsers();
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
        }
        private static async Task SearchOtherUsers()
        {
            Console.WriteLine("Enter email(example@gmail.com) or username(example):");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Error: Input cannot be empty.");
                return;
            }
            if (!input.Contains("@"))
                input += "@gmail.com";

            var otherUser = await _userService.GetUserByEmailAsync(input);

            if (otherUser == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"User with email '{input}' not found.");
                Console.ResetColor();
                return;
            }

            if (otherUser.Id == _currentUser.Id)
            {
                Console.WriteLine("You cannot search for yourself.");
                return;
            }

            _userService.UserInfo(otherUser);

            while (true)
            {
                Console.WriteLine("\nSelect option");
                Console.WriteLine("1 - Add to friends/Remove from friends");
                Console.WriteLine("2 - Follow/Unfollow");
                Console.WriteLine("3 - View posts");
                Console.WriteLine("q - Back to main menu");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var c = char.ToLowerInvariant(line[0]);
                switch (c)
                {
                    case 'q':
                        return;
                    case '1':
                        _userService.AddOrRemoveFriends(_currentUser, otherUser);
                        break;
                    case '2':
                        _userService.FollowOrUnfollow(_currentUser, otherUser);
                        break;
                    case '3':
                        await _postService.ShowUserPosts(otherUser);
                        await PostsMenu(otherUser);
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }

        }
        static async Task PostsMenu(User otherUser)
        {
            Post localPost = null;
            while (true)
            {
                Console.WriteLine("\nSelect option");
                Console.WriteLine("1 - React on post");
                Console.WriteLine("2 - Remove reaction");
                Console.WriteLine("3 - Comment on post");
                Console.WriteLine("q - Back to main menu");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var c = char.ToLowerInvariant(line[0]);
                switch (c)
                {
                    case 'q':
                        return;
                    case '1':
                        var selectedPost = await _postService.ReactToPost(_currentUser, otherUser);
                        localPost = selectedPost;
                        break;
                    case '2':
                        if (localPost == null)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Please select a post first by choosing option 1.");
                            Console.ResetColor();
                        }
                        else
                        {
                            await _postService.RemoveReaction(_currentUser, localPost);
                        }
                        break;
                    case '3':
                        await _postService.AddCommentToPost(_currentUser, otherUser);
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }

        }

    }
}
