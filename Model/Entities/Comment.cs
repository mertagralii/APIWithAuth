using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace APIWithAuth.Entities;

public class Comment
{
    public int Id { get; set; }
    public int TweetId { get; set; }
    [ForeignKey("TweetId")]
    public Tweet Tweet { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public required string Body { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
}