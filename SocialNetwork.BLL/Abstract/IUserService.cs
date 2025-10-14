using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Concrete;

namespace SocialNetwork.BLL.Abstract
{
    public interface IUserService
    {
        public void UserInfo(User user);
        public void AddToFriends(User user, User friend, UserRepository userRepo);
    }
}
