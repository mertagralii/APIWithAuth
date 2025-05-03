using APIWithAuth.Model.Dto.Tweet;
using APIWithAuth.Model.Dto.User;

namespace APIWithAuth.Model.Dto.Comment;

public class CommentDto
{
    public int Id { get; set; }
    public string Body { get; set; }
    public ApplicationUserDto ApplicationUser { get; set; }
    public DateTime Created { get; set; }
    
}