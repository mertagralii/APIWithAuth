using Microsoft.AspNetCore.Identity;

namespace APIWithAuth.Entities;

public class Tweet
{
    public int Id { get; set; }
    public string Body { get; set; }
    public string UserId { get; set; } // eğer sizde string olmazsa guid yapabilirsiniz
    public ApplicationUser User { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
}