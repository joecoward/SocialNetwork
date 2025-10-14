using SocialNetwork.BLL.Abstract;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.BLL.Concrete
{
    public class PostService : IPostService
    {
        public  bool ShowUserPosts(PostRepository postRepo, User user)
        {
            var posts = postRepo.GetPostsByUserIdAsync(user.Id).Result;
            if (posts == null || posts.Count == 0)
            {
                Console.WriteLine("No posts available.");
                return false;
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

            return true;
        }
    }
}
