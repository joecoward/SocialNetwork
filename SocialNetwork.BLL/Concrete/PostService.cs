using SocialNetwork.BLL.Abstract;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.MongoDb.Abstract;
using System.Linq;

namespace SocialNetwork.BLL.Concrete
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        public PostService(IPostRepository postRepository, IUserRepository userRepository)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
        }
        public async Task ShowUserPosts(User user)
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(user.Id);
            if (posts == null || posts.Count == 0)
            {
                Console.WriteLine("No posts available.");
                return;
            }
            else
            {
                Console.WriteLine($"\nPosts by {user.FirstName} {user.LastName}:\n");
                foreach (var post in posts)
                {
                    Console.WriteLine($"////////////////////////////////////\n");
                    Console.WriteLine($"Content: {post.Content}");
                    Console.WriteLine($"Created At: {post.CreatedAt}");
                    Console.WriteLine("------------------------------------");

                    var postReactions = post.Reactions;
                    if (postReactions != null && postReactions.Count > 0)
                    {
                        Console.WriteLine("Reactions:");
                        foreach (var reaction in postReactions)
                        {
                            Console.WriteLine($"- {reaction.Type} (at {reaction.CreatedAt})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No reactions on this post.");
                    }

                    var postComments = post.Comments;
                    if (postComments != null && postComments.Count > 0)
                    {
                        Console.WriteLine("Comments:");
                        foreach (var comment in postComments)
                        {
                            Console.WriteLine($"- {comment.Text} (at {comment.CreatedAt})");
                            var commentReactions = comment.Reactions;
                            if (commentReactions != null && commentReactions.Count > 0)
                            {
                                Console.WriteLine("  Reactions:");
                                foreach (var reaction in commentReactions)
                                {
                                    Console.WriteLine($"  - {reaction.Type} (at {reaction.CreatedAt})");
                                }
                            }
                            else
                            {
                                Console.WriteLine("  No reactions on this comment.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No comments on this post.");
                    }
                    Console.WriteLine($"\n////////////////////////////////////\n\n\n\n");

                }
            }

            return;
        }
        public async Task CreatePost( string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                Console.WriteLine("Error: Author not found.");
                return;
            }

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
            await _postRepository.CreateAsync(newPost);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Post created successfully!");
            Console.ResetColor();
        }
        public async Task AddCommentToPost(User currentUser, User otherUser)
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(otherUser.Id);
            if (posts == null || posts.Count == 0)
            {
                Console.WriteLine("No posts available to comment on.");
                return;
            }

            Console.WriteLine("Select a post to comment on:");
            for (int i = 0; i < posts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {posts[i].Content} (Created At: {posts[i].CreatedAt})");
            }
            if (!int.TryParse(Console.ReadLine(), out int postIndex) || postIndex < 1 || postIndex > posts.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            var selectedPost = posts[postIndex - 1];

            Console.WriteLine("Enter your comment:");
            var commentText = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(commentText))
            {
                Console.WriteLine("Error: Comment cannot be empty.");
                return;
            }
            var newComment = new Comment
            {
                UserId = currentUser.Id,
                Text = commentText,
                CreatedAt = DateTime.UtcNow,
                Reactions = new List<Reaction>()
            };

            if (selectedPost.Comments == null)
            {
                selectedPost.Comments = new List<Comment>();
            }
            selectedPost.Comments.Add(newComment);

            await _postRepository.UpdateAsync(selectedPost);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Comment added successfully!");
            Console.ResetColor();
        }
        public async Task<Post> ReactOrRemoveToPost(User currentUser, User otherUser)
        {
            var  posts = await _postRepository.GetPostsByUserIdAsync(otherUser.Id);
            if (posts == null || posts.Count == 0)
            {

                throw new Exception("No posts available to react on.");

            }
            Console.WriteLine("Select a post to react on:");
            for (int i = 0; i < posts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {posts[i].Content} (Created At: {posts[i].CreatedAt})");
            }
            if (!int.TryParse(Console.ReadLine(), out int postIndex) || postIndex < 1 || postIndex > posts.Count)
            {
                throw new Exception("Invalid selection.");
            }
            var selectedPost = posts[postIndex - 1];
            Console.WriteLine("Select a reaction type:");
            var reactionTypes = new List<string> { "Like", "Love", "Haha", "Wow", "Sad", "Angry" };
            for (int i = 0; i < reactionTypes.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {reactionTypes[i]}");
            }
            if (!int.TryParse(Console.ReadLine(), out int reactionIndex) || reactionIndex < 1 || reactionIndex > reactionTypes.Count)
            {
                throw new Exception("Invalid selection.");
            }
            if (selectedPost.Reactions.Any(r => r.UserId == currentUser.Id && r.Type == reactionTypes[reactionIndex - 1]))
            {
                Console.WriteLine("You have already reacted with this type to this post.");
                Console.WriteLine("Do you want to remove your reaction? (yes/no)");
                var response = Console.ReadLine();
                if (response?.ToLower() != "yes")
                {
                    return selectedPost;
                }
                else
                {
                    var filter = selectedPost.Reactions.Where(r => r.UserId == currentUser.Id).Count();
                    for (int i = 0; i < filter; i++)
                    {
                        Console.WriteLine($"{i + 1}. {selectedPost.Reactions[i].Type} (at {selectedPost.Reactions[i].CreatedAt})");
                    }
                    if (!int.TryParse(Console.ReadLine(), out int temp) || temp < 1 || temp > selectedPost.Reactions.Count)
                    {
                        throw new Exception("Invalid selection.");

                    }
                    var reactionToRemove = selectedPost.Reactions[reactionIndex - 1];
                    selectedPost.Reactions.Remove(reactionToRemove);
                    await _postRepository.UpdateAsync(selectedPost);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Reaction removed successfully!");
                    Console.ResetColor();
                }
            }

            var selectedReactionType = reactionTypes[reactionIndex - 1];
            var newReaction = new Reaction
            {
                UserId = currentUser.Id,
                Type = selectedReactionType,
                CreatedAt = DateTime.UtcNow
            };
            if (selectedPost.Reactions == null)
            {
                selectedPost.Reactions = new List<Reaction>();
            }
            selectedPost.Reactions.Add(newReaction);

            await _postRepository.UpdateAsync(selectedPost);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Reaction added successfully!");
            Console.ResetColor();
            return selectedPost;
        }

        public async Task SteamPosts()
        {
            var allPosts = await _postRepository.GetAllAsync();
            var filteredPosts = allPosts.OrderByDescending(p => p.CreatedAt).ToList();
            if (filteredPosts == null || filteredPosts.Count == 0)
            {
                Console.WriteLine("No posts available.");
                return;
            }
            else
            {
                Console.WriteLine($"\nAll Posts:\n");
                foreach (var post in filteredPosts)
                {
                    Console.WriteLine($"////////////////////////////////////\n");
                    Console.WriteLine($"Content: {post.Content}");
                    Console.WriteLine($"Created At: {post.CreatedAt}");
                    Console.WriteLine("------------------------------------");
                    var postReactions = post.Reactions;
                    if (postReactions != null && postReactions.Count > 0)
                    {
                        Console.WriteLine("Reactions:");
                        foreach (var reaction in postReactions)
                        {
                            Console.WriteLine($"- {reaction.Type} (at {reaction.CreatedAt})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No reactions on this post.");
                    }
                    var postComments = post.Comments;
                    if (postComments != null && postComments.Count > 0)
                    {
                        Console.WriteLine("Comments:");
                        foreach (var comment in postComments)
                        {
                            Console.WriteLine($"- {comment.Text} (at {comment.CreatedAt})");
                            var commentReactions = comment.Reactions;
                            if (commentReactions != null && commentReactions.Count > 0)
                            {
                                Console.WriteLine("  Reactions:");
                                foreach (var reaction in commentReactions)
                                {
                                    Console.WriteLine($"  - {reaction.Type} (at {reaction.CreatedAt})");
                                }
                            }
                            else
                            {
                                Console.WriteLine("  No reactions on this comment.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No comments on this post.");
                    }
                    Console.WriteLine($"\n////////////////////////////////////\n\n\n\n");
                }
            }
            return;
        }


    }
}
