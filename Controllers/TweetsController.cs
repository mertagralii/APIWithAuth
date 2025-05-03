using System.Net;
using System.Net.Mail;
using APIWithAuth.Data;
using APIWithAuth.Entities;
using APIWithAuth.Model.Dto.Comment;
using APIWithAuth.Model.Dto.Tweet;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace APIWithAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TweetsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext   _context;
        private readonly IMapper _mapper;

        public TweetsController(UserManager<IdentityUser> userManager, AppDbContext context, IMapper mapper )
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }
        
        [HttpGet]
        public ActionResult<TweetDto[]> Index()
        {
            return _mapper.Map<TweetDto[]>(_context.Tweets
                .Include(t => t.User)
                .ToArray());
        }
        
        [HttpGet("{userId}")]
        public async Task<ActionResult<TweetDto>> GetTweetByUserId(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            var userTweet = _context.Tweets.Where(t=>t.UserId == userId).Include(x=>x.User).ToArray();
            return _mapper.Map<TweetDto>(userTweet);
        }
        
        [HttpGet("{userId}/{tweetId}")]
        public ActionResult<TweetDto> GetTweetById(string userId, int tweetId)
        {
            var tweet = _context.Tweets
                .Where(t => t.Id == tweetId && t.UserId == userId)
                .Include(t => t.User)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefault();
            if (tweet == null)
            {
                return NotFound();
            }
            return  _mapper.Map<TweetWithCommentsDto>(tweet);
        }
        
        [Authorize]
        [HttpPost("{tweetUserId}/{tweetId}/[action]")]
        public async Task<ActionResult> AddComment(string tweetUserId, int tweetId, CommentAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
        
            var tweet = await _context.Tweets.FindAsync(tweetId);
            if (tweet == null)
            {
                return NotFound();
            }

            // bu çok elzem değil fakat adres bar ile tutarlılık olsa iyi olur
            if (tweet.UserId != tweetUserId)
            {
                return BadRequest();
            }
        
            var newComment = _mapper.Map<Comment>(model);
            var userId = _userManager.GetUserId(User);
            newComment.UserId = userId;
            newComment.TweetId = tweetId;
            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        [Authorize]
        [HttpPost("add")]
        public async Task<ActionResult> AddTweet(TweetAddDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
    
            // aslında kullanıcıyı almamıza gerek yok
            // çünkü sadece kullanıcı id'si bizim için yeterli
            // buna bağlı olarak mevcut login olan kullanıcıyı claims üzerinden alabiliriz
            // var user = await _userManager.GetUserAsync(User);
            var userId = _userManager.GetUserId(User);
    
            var newTweet = _mapper.Map<Tweet>(model);
            newTweet.UserId = userId;
    
            _context.Tweets.Add(newTweet);
            await _context.SaveChangesAsync();
    
            return Ok();
        }
        
       
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTweet(int id)
        {
            var tweet = await _context.Tweets.FindAsync(id);
            if (tweet == null)
            {
                return NotFound();  // böyle bir tweet yok.
            }
            var userId = _userManager.GetUserId(User);
            if (tweet.UserId != userId)
            {
                return Unauthorized(); // senin buna yetkin yok demek.
            }
            _context.Tweets.Remove(tweet);
            await _context.SaveChangesAsync();
            return NoContent();
            
        }
        // Swagger Dökümantasyonu 
        // [SwaggerOperation( 
        //     Summary = "Creates a new product",
        //     Description = "Requires admin privileges",
        //     OperationId = "CreateProduct",
        //     Tags = new[] { "Purchase", "Products" }
        // )]
        
        [HttpPost("[action]")]
        public async Task<ActionResult> SendEmail() // Mail gönderme kısmı 
        {
            var client = new SmtpClient();
            client.Host = "smtp.resend.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("resend", "re_FJ5SaQpc_6YiVNdRttEnhGk3RRa7TbLf8"); // Mail Apı key eklendi, Resend.com
            var message = new MailMessage();
            message.To.Add(new MailAddress("ins.muh.onuragrali@gmail.com", "Onur Ağralı")); // Gönderilecek kişinin maili, Alıcının ismi
            message.From = new MailAddress("noreply@bildirim.mertagrali.com", "Staj başvurusu"); // Gönderen kişinin Mail adresi ve marka başlığı
            message.Subject = "Dolandırıcı Maili"; // Konu Başlığı
            message.Body = "Dolandırıcı maili atıyorum burdan"; // Metin Kısmı
            try
            {
                await client.SendMailAsync(message);

            }
            catch
            {

            }

            message.Dispose();
            client.Dispose();
            return Ok();
        }
        
        // eğer alt alta yazarsak iki rolde de bulunmak zorunda
        // eğer tek satırda yazıp virgül ile ayırırsak iki rolden birinde olması yeterli demek
        // [Authorize(Roles = "SuperUser")]
        [Authorize(Roles = "Admin")]
        [HttpGet("[action]")]
        public ActionResult AdminOnly()
        {
            return Ok("Admin only");
        }
    }
}
// [Authorize] // Eğer kullanıcı login olmazsa bunu görmene izin yok.
// [HttpGet("All")]
// public IActionResult ListAllTweets()
// {
//     var user = _userManager.GetUserId(User);
//     return Ok(user);
// }
        
       
// [HttpGet("All")]
// public ActionResult<Tweet[]>ListAllTweets() // Burd derlenme esnasında kontrol ediyor. Servislerde tercih ediliyor.
// {
//    return _context.Tweets.ToArray();
// }


// [Authorize] // Eğer kullanıcı login olmazsa bunu görmene izin yok.
// [HttpPost]
// public IActionResult AddTweet()
// {
//     var userId = _userManager.GetUserId(User);
//     var newTweet = new Tweet()
//     {
//         Body = "Merhaba Tweet Dünyası",
//         UserId = userId
//     };
//     _context.Tweets.Add(newTweet);
//     _context.SaveChanges();
//     return Ok(newTweet);
// }

// rabit mq
// postgare sql
// oracle 
