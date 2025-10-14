using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.BLL.Abstract
{
    public interface IPostService
    {
        public bool ShowUserPosts(PostRepository postRepo, User user);
    }
}
