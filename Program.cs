    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;
    using APIWithAuth.Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.EntityFrameworkCore;

    namespace APIWithAuth;
    public sealed class SlugifyParameterTransformer : IOutboundParameterTransformer // Slug oluşturmak için kullanyoruz.
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null) { return null; }
            string? str = value.ToString();
            if (string.IsNullOrEmpty(str)) { return null; }

            return Regex.Replace(str, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services
                .AddControllers(options =>
                {
                    options.Conventions.Add(
                        new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations(); // Swagger Dökümantasyon için 
            });
            builder.Services.AddAutoMapper(typeof(Program).Assembly); // AutoMapper Kullanacağımız Zaman bunu kullanmamız gerekiyor.
            builder.Services.AddAuthorization();
            
            builder.Services.Configure<IdentityOptions>(options =>
            { 
                // Burada istediğimiz ayarlamalarımızı yapıyoruz. Giriş ve kayıt ayarlamaları
                options.SignIn.RequireConfirmedAccount = false; // Eğer herhangi bir kullanıcı e posta ve şifre ile kayıt yaoarsa doğrulaması gerek ama biz false dedik.
                options.Password.RequiredLength = 3; // Şifrenin uzunluğu en az 3 karakter olsun.
                options.Password.RequireNonAlphanumeric = false; // Mutlaka rakam olsun
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false; // büyük harf olsun
                options.Password.RequireLowercase = false; // küçük harf olsun
                //options.Lockout.MaxFailedAccessAttempts = 5; // 5 defa kullanıcı girişi hatalı olursa patlatıyor.
            });
            // end pointleri oluşturması için çağırdık.
            builder.Services.AddIdentityApiEndpoints<IdentityUser>()
                .AddRoles<IdentityRole>() // IdentityRole
                .AddEntityFrameworkStores<AppDbContext>();
            
            var app = builder.Build();
            
            using (var scope = app.Services.CreateScope()) // İdentityRole
            {
                var services = scope.ServiceProvider;
                await SeedRoles(services);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            // identity'lerin enpointlerin başına ekleme yapmak için
            //prefix ön ekle suffix son ek
            app.MapGroup("/user").MapIdentityApi<IdentityUser>(); // Normalde Identity'in başında birşey yazmıyordu biz bunu user diye bir bölümün altına alalım.
            app.MapControllers();

            app.Run();
        }
        public static async Task SeedRoles(IServiceProvider serviceProvider) // İdentityRole
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    
            if (await roleManager.RoleExistsAsync("Admin")) { return; }
    
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            //await roleManager.CreateAsync(new IdentityRole("User"));
    
            var adminUser = new IdentityUser { UserName = "admin", Email = "orhanekici@gmail.com" };
            adminUser.EmailConfirmed = true;
            await userManager.CreateAsync(adminUser, "P99yG-wSd8T$");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }