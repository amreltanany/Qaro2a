using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Mapping; 
using ECommerce.Application.Services.Implementations;
using ECommerce.Application.Services.Interfaces;
using ECommerce.API.Options;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services.Implementations;
using ECommerce.Infrastructure.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.ResponseCompression;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using System.IO.Compression;
using System.Globalization;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

#region Services
builder.Services.AddControllers();

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();
builder.Services.AddLocalization();

builder.Services.Configure<SeoOptions>(builder.Configuration.GetSection(SeoOptions.SectionName));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce API", Version = "v1" });

    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securitySchema);
    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securitySchema, new[] { "Bearer" } }
    };
    c.AddSecurityRequirement(securityRequirement);
});

builder.Services.AddMemoryCache();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);

builder.Services.AddImageSharp(options =>
{
    options.BrowserMaxAge = TimeSpan.FromDays(7);
    options.OnParseCommandsAsync = ctx =>
    {
        if (ctx.Commands.Count == 0)
            return Task.CompletedTask;
        if (ctx.Commands.TryGetValue(ResizeWebProcessor.Width, out var wVal)
            && uint.TryParse(wVal, NumberStyles.Integer, CultureInfo.InvariantCulture, out var width)
            && width > 2560)
            ctx.Commands[ResizeWebProcessor.Width] = "2560";
        return Task.CompletedTask;
    };
});
#endregion

#region DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.MigrationsAssembly("ECommerce.Infrastructure");
            // Azure SQL can return 40613 while the database resumes from pause/auto-pause.
            sql.EnableRetryOnFailure(
                maxRetryCount: 6,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
#endregion

#region Authentication / Authorization
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Login";
});

builder.Services.AddScoped<ITokenService, TokenService>();

// Explicitly set JWT as default to override Identity Cookie defaults
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.MapInboundClaims = false;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token))
            {
                var token = context.Request.Cookies["token"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is not set"))),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            ECommerce.API.Helpers.UserClaimsHelper.IsAdminEmail(
                ECommerce.API.Helpers.UserClaimsHelper.GetEmail(context.User))));
});

#endregion

#region Custom Business Services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishlistItemRepository, WishlistItemRepository>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IPublishRepository, PublishRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();

// Cleaned up Validators & AutoMapper
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDto>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Cart expiry: clear carts older than 2 days (runs once per day)
builder.Services.AddHostedService<ECommerce.API.BackgroundServices.CartExpiryHostedService>();

#endregion

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

#region Middleware Pipeline
if (app.Environment.IsDevelopment())
{
   app.UseDeveloperExceptionPage();
   app.UseSwagger();
   app.UseSwaggerUI();
}
else
{
   app.UseExceptionHandler("/Home/Error");
}

app.UseResponseCompression();

app.UseImageSharp();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ar"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    // Cookie only: explicit user choice overrides default. No Accept-Language header,
    // so first-time visitors get Arabic (DefaultRequestCulture) instead of browser locale.
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider()
    }
});

var staticFileOptions = new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name;
        if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            return;

        if (app.Environment.IsDevelopment())
            ctx.Context.Response.Headers.CacheControl = "public,max-age=3600,must-revalidate";
        else
            ctx.Context.Response.Headers.CacheControl = "public,max-age=604800";
    }
};

app.UseDefaultFiles();
app.UseStaticFiles(staticFileOptions);
app.UseHttpsRedirection();
app.UseMiddleware<ECommerce.API.Middleware.ExceptionHandlingMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.MapGet("/health", async (AppDbContext db, CancellationToken cancellationToken) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            return Results.Json(new
            {
                status = "degraded",
                database = "unavailable",
                utc = DateTime.UtcNow
            }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Results.Ok(new
        {
            status = "ok",
            database = "connected",
            utc = DateTime.UtcNow
        });
    }
    catch
    {
        return Results.Json(new
        {
            status = "degraded",
            database = "error",
            utc = DateTime.UtcNow
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
#endregion

app.Run();