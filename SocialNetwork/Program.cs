using BCrypt.Net;
using MongoDB.Driver;
using Org.BouncyCastle.Crypto.Generators;
using SocialNetwork.BLL.Abstract;
using SocialNetwork.BLL.Concrete;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Concrete;
using SocialNetwork.DAL.MgContext;
using System.Text.RegularExpressions;

namespace SocialNetwork
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = "mongodb://localhost:27017";
            string databaseName = "SocialNetworkDB";
            var context = new MongoDbContext(connectionString, databaseName);
            var userRepo = new UserRepository(context);
            var postRepo = new PostRepository(context);
            var posts = await postRepo.GetAllAsync();
            //pass 12341234 myemail@gmail.com

            Console.WriteLine("Welcome to my SocialNetwork");

            while (true)
            {
                Console.WriteLine("\nSelect option");
                Console.WriteLine("r - Register");
                Console.WriteLine("l - Login");
                Console.WriteLine("q - Quit");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var c = char.ToLowerInvariant(line[0]);
                switch (c)
                {
                    case 'q':
                        return;
                    case 'r':
                        await Registration(userRepo);
                        break;
                    case 'l':
                        await Login(userRepo, postRepo);

                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }



        }

        private static async Task Login(UserRepository userRepo, PostRepository postRepo)
        {
            Console.WriteLine("\nLogining");
            Console.Write("Enter your email:");
            var email = Console.ReadLine();
            Console.Write("Enter your password:");
            var password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Error: Email or password is null.");
                return;
            }
            var user = await userRepo.GetUserByEmailAsync(email);
            if (user == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: User with this email doesn't exists.");
                Console.ResetColor();
                return;
            }
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (isPasswordValid)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Login successful! Congratulations!, {user.Email}!");
                Console.ResetColor();
                UserMenu(userRepo, postRepo, email);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Incorrect password.");
                Console.ResetColor();
            }
        }

        private static void UserMenu(UserRepository userRepo, PostRepository postRepo, string email)
        {
            while (true)
            {
                Console.WriteLine("\nSelect option");
                Console.WriteLine("1 - View Profile");
                Console.WriteLine("2 - Create Post");
                Console.WriteLine("3 - Search other users");
                Console.WriteLine("q - Logout");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var c = char.ToLowerInvariant(line[0]);
                switch (c)
                {
                    case 'q':
                        return;
                    case '1':
                        ViewUserProfile(userRepo, postRepo, email);
                        break;
                    case '2':
                        CreatePost(userRepo,postRepo, email);
                        break;
                    case '3':
                        SearchOtherUsers(userRepo, postRepo,email);
                        break;
                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
            static void CreatePost(UserRepository userRepo, PostRepository postRepo, string email)
            {
                var user = userRepo.GetUserByEmailAsync(email).Result;
                Console.WriteLine("Enter context of post:");
                var content = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(content))
                {
                    Console.WriteLine("Error: Post content cannot be empty.");
                    return;
                }
                var newPost = new Post
                {
                    UserId = user.Id,
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    Reactions = new List<Reaction>(),
                    Comments = new List<Comment>()

                };
                postRepo.CreateAsync(newPost).Wait();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Post created successfully!");
                Console.ResetColor();


            }
            static void ViewUserProfile(UserRepository userRepo, PostRepository postRepo, string email)
            {
                var user = userRepo.GetUserByEmailAsync(email).Result;
                if (user == null)
                {
                    Console.WriteLine("User not found.");
                    return;
                }
                var userService = new UserService();
                var postService = new PostService();

                {
                    userService.UserInfo(user);
                    bool flowControl = postService.ShowUserPosts(postRepo, user);
                    if (!flowControl)
                    {
                        return;
                    }

                }
            }
            static void SearchOtherUsers(UserRepository userRepo, PostRepository postRepo,string email)
            {
                Console.WriteLine("Enter email(example@gmail.com) or username(example):");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Error: Input cannot be empty.");
                    return;
                }
                if (!input.EndsWith("@gmail.com"))
                    input += "@gmail.com";
                ViewUserProfile(userRepo, postRepo, input);

                while (true)
                {
                    Console.WriteLine("\nSelect option");
                    Console.WriteLine("1 - Add to friends");
                    Console.WriteLine("2 - Follow");
                    Console.WriteLine("3 - View posts");
                    Console.WriteLine("q - Back");
                    var line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var c = char.ToLowerInvariant(line[0]);
                    switch (c)
                    {
                        case 'q':
                            return;
                        case '1':
                            var userService = new UserService();
                            var mainUser = userRepo.GetUserByEmailAsync(email).Result;
                            var friend = userRepo.GetUserByEmailAsync(input).Result;
                            userService.AddToFriends(mainUser, friend, userRepo);
                            break;
                        case '2':
                            Console.WriteLine("Not ready");
                            break;
                        case '3':
                            Console.WriteLine("Not ready");
                            break;
                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }
                }

            }
            
        }


        private static async Task Registration(UserRepository userRepo)
        {
            await userRepo.CreateEmailIndexAsync();

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


            if (await ValidateInput(firstname, lastname, email, password, passwordConfirm, userRepo))
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

                    await userRepo.CreateAsync(newUser);

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
            static async Task<bool> ValidateInput(string? firstname, string? lastname, string? email, string? password, string? passwordConfirm, UserRepository userRepo)
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
                if (await userRepo.UserExistsAsync(email))
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
        }
    
    }
}
