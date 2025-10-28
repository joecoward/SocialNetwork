using MongoDB.Driver;
using SocialNetwork.BLL.Abstract;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Neo4j.Abstract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialNetwork
{
    public class SocialNetworkApp
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly INodesRepository _nodesRepository;
        private User _currentUser;
        public SocialNetworkApp(IUserService userService, IPostService postService,INodesRepository nodesRepository)
        {
            _userService = userService;
            _postService = postService;
            _nodesRepository = nodesRepository;
            _currentUser = null;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Welcome to my SocialNetwork");

            while (true)
            {
                if (_currentUser == null)
                    await ShowMainMenu();
                else
                    await ShowUserMenu();
            }
        }

        private async Task ShowMainMenu()
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
                    Console.WriteLine("Exiting...");
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
        private async Task Login()
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

        private async Task Registration()
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

        private async Task ShowUserMenu()
        {
            await _postService.SteamPosts();
            Console.WriteLine($"\n--- Logged in as {_currentUser.Email} ---");
            while (true)
            {
                Console.WriteLine("\nSelect option");
                Console.WriteLine("1 - View My Profile & Posts");
                Console.WriteLine("2 - Create Post");
                Console.WriteLine("3 - Search other users");
                Console.WriteLine("4 - Update profile");
                Console.WriteLine("5 - Delete account");
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
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Your Post has been added.");
                        Console.ResetColor();
                        break;
                    case '3':
                        await SearchOtherUsers();
                        break;
                    case '4':
                        await _userService.UpdateUserProfileAsync(_currentUser.Email);
                        break;
                    case '5':
                        await _userService.DeleteUserAsync(_currentUser);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Your account has been deleted.");
                        Console.ResetColor();
                        _currentUser = null;
                        return;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
        }

        private async Task SearchOtherUsers()
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
            await _userService.UserShortestPath(_currentUser, otherUser);


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
                        await _userService.AddOrRemoveFriends(_currentUser, otherUser);
                        break;
                    case '2':
                        await _userService.FollowOrUnfollow(_currentUser, otherUser);
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

        private async Task PostsMenu(User otherUser)
        {
            while (true)
            {
                Console.WriteLine("\nSelect option");
                Console.WriteLine("1 - React on post/Remove reaction");
                Console.WriteLine("2 - Comment on post");
                Console.WriteLine("q - Back to main menu");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var c = char.ToLowerInvariant(line[0]);
                switch (c)
                {
                    case 'q':
                        return;
                    case '1':
                        await _postService.ReactOrRemoveToPost(_currentUser, otherUser);
                        break;
                    case '2':
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
