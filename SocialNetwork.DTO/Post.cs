//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SocialNetwork.DTO
//{
//    public class ReactionDto
//    {
       
//        public string UserId { get; set; }

//        public string Type { get; set; } 
//    }
//    public class CommentDto
//    {
       
//        public string CommentId { get; set; }
//        public string UserId { get; set; }
//        public string Text { get; set; }
//        public DateTime CreatedAt { get; set; }

//        public List<ReactionDto> Reactions { get; set; } = new List<ReactionDto>();
//    }
//    internal class PostDto
//    {
//        public string Id { get; set; }

//        public string UserId { get; set; }
//        public string Context { get; set; }
//        public DateTime CreatedAt { get; set; }

//        public List<ReactionDto> Reactions { get; set; } = new List<ReactionDto>();
//        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();

//    }
//}
