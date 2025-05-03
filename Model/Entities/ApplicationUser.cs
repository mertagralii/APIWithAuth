using APIWithAuth.Model.Dto.Tweet;
using Microsoft.AspNetCore.Identity;

namespace APIWithAuth.Entities;

public class ApplicationUser: IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public ICollection<Tweet> Tweets { get; set; } = new List<Tweet>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

}