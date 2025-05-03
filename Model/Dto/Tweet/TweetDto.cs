using System.Text.Json.Serialization;
using APIWithAuth.Model.Dto.User;
using APIWithAuth.Entities;
using APIWithAuth.Model.Dto.Comment;

namespace APIWithAuth.Model.Dto.Tweet;

public class TweetDto
{
    public int Id { get; set; }
    public string Body { get; set; }
    public ApplicationUserDto ApplicationUser { get; set; }
    public DateTime Created { get; set; }
}

public class TweetWithCommentsDto : TweetDto
{
    // This Property is used to serialize the comments in the correct order
    [JsonPropertyOrder(1)]
    public ICollection<CommentDto> Comments { get; set; }
}