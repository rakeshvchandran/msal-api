using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using msal_api;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//  .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.Authority = "https://login.microsoftonline.com/374ffac1-37d2-40de-a775-31ed3b93e71e/v2.0";
           options.Audience = builder.Configuration["AzureAd:ClientId"];
           options.Events = new JwtBearerEvents
           {
               //OnAuthenticationFailed = AuthenticationFailed
           };

           /* Note:Authority and audience are enough to validate AD B2C Authentication but if your requirement is to validate the Token issuer & SigninKey also then include the below code as well.*/
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               //ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = "https://sts.windows.net/374ffac1-37d2-40de-a775-31ed3b93e71e/",
               ValidAudience = builder.Configuration["AzureAd:ClientId"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AzureAd:SecretKey"]))
           };
       });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddScoped<IServiceBusMessageHandler, ServiceBusMessageHandler>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("oAuth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(builder.Configuration["SwaggerAzureAD:AuthorizationUrl"]),
                TokenUrl = new Uri(builder.Configuration["SwaggerAzureAD:TokenUrl"]),
                Scopes = new Dictionary<string, string>
                {
                    { builder.Configuration["SwaggerAzureAD:Scope"], "Access API as user" }
                }
            }
        }
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { builder.Configuration["ApiScope"] }
        }
    });
});

builder.Services.AddCors(o => o.AddPolicy("msal-api-Policy", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId(builder.Configuration["SwaggerAzureAD:ClientId"]);
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
    });
    app.UseCors(x =>
    {
        x.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
