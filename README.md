# 🔐 ASP.NET Core Identity + JWT Authentication/Authorization Rehberi

Bu rehber, bir ASP.NET Core API projesine kimlik doğrulama (Authentication) ve yetkilendirme (Authorization) sistemini kurmak için gerekli adımları içerir.

🚪 ASP.NET Core Identity + JWT Authentication/Authorization Nedir?

ASP.NET Core Identity, kullanıcı kimliği yönetimi için kullanılır. Yani kullanıcı kayıt, giriş, parola işlemleri gibi işleri senin yerine kolaylaştırır.

JWT (JSON Web Token) ise kullanıcı giriş yaptıktan sonra ona bir dijital kimlik kartı gibi bir token verir. Bu token, API’ye gelen her istekte gönderilir ve kullanıcının yetkili olup olmadığı bu token üzerinden kontrol edilir.

💡 Kısacası:

Identity: Kullanıcıyı tanı.

JWT: Tanınan kullanıcıya giriş izni ver, ve bu izni koru.

🧠 1. ASP.NET Core Identity Neyi Sağlar?

Kullanıcı kaydı ve oturum açma

Rollerle yetkilendirme (admin, user vs.)

Parola sıfırlama

2 aşamalı doğrulama gibi ekstra güvenlik adımları

Ve bunu hazır tablolar ve servislerle sağlar. Senin baştan kullanıcı yönetim sistemi yazmana gerek kalmaz.

🔑 2. JWT Token Nedir?

JWT (JSON Web Token), giriş yapmış kullanıcıya oluşturulan şifreli bir bilgi paketidir. Örneğin:

```csharp
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Bu token:

Kullanıcının kim olduğunu,

Token’ın ne zaman sona ereceğini,

Hangi rollerle geldiğini (örn: "admin", "user")

gibi bilgileri içerir.

🧩 3. Identity + JWT Birlikte Nasıl Çalışır?

Kullanıcı giriş yapar (/login endpoint'i).

Identity bu kişinin doğru kullanıcı olduğunu onaylar.

Uygulama bu kullanıcı için bir JWT üretir.

Kullanıcı sonraki isteklerinde bu token'ı Authorization başlığında (Bearer şeklinde) gönderir.

Uygulama bu token’ı kontrol eder ve ona göre işlem yapar.

---

## 🔧 1. Gerekli NuGet Paketlerini Projeye Ekle

### Terminalden ya da NuGet Package Manager'dan yükle:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

> Diğer alternatif veritabanları:
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Microsoft.EntityFrameworkCore.InMemory` (test amaçlı)

---

## 🔗 2. `appsettings.json` Dosyasına Veritabanı Bağlantısı Ekle

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

✅ `TrustServerCertificate=True` → Sertifika hatalarını önler.

---

## 🗃️ 3. AppDbContext Sınıfı

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APIWithAuth.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
```

> `IdentityDbContext<IdentityUser>` sayesinde Identity tabloları otomatik oluşur.

---

## ⚙️ 4. Program.cs İçinde Gerekli Servisleri Tanımla

### 🔌 Veritabanı bağlantısı:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 👤 Identity sistemini ekle:

```csharp
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();
```

### 🔐 Yetkilendirme (Authorization):

```csharp
builder.Services.AddAuthorization();
```

### 🔑 Şifre ve kullanıcı giriş kuralları:

```csharp
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});
```

### 🧭 Identity endpoint'lerine `/user` prefix’i ekle:

```csharp
app.MapGroup("/user").MapIdentityApi<IdentityUser>();
```

---

## 🛣️ 5. Endpoint'leri Kebab-case Yapmak (Opsiyonel Ama Şık)

### 🔧 SlugifyParameterTransformer sınıfını oluştur:

```csharp
public sealed class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value == null) return null;
        string? str = value.ToString();
        if (string.IsNullOrEmpty(str)) return null;

        return Regex.Replace(str, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
    }
}
```

### 🔧 `AddControllers()` satırını değiştir:

```csharp
builder.Services
    .AddControllers(options =>
    {
        options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
```

---

## 🧱 6. Migration ve Veritabanı Oluşturma

### Terminalden çalıştır:

```bash
dotnet ef migrations add InitialCreated
dotnet ef database update
```

- `migrations add` → Tabloların kodda karşılığı oluşturulur.
- `database update` → Veritabanına uygulanır.

---

## 🧚‍♂️ 7. Controller’da Kullanıcı İşlemleri Yapmak için Tanımlamalar

### Kullanıcı ile işlem yapmak için UserManager kullanılır:

```csharp
private readonly UserManager<IdentityUser> _userManager;
private readonly AppDbContext _context;

public TweetsController(UserManager<IdentityUser> userManager, AppDbContext context)
{
    _userManager = userManager;
    _context = context;
}
```

> `UserManager` sayesinde kullanıcı oluşturma, şifre kontrolü, giriş çıkış gibi işlemler yapılabilir.

---

