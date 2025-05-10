using Coursework.DataAccess;
using Coursework.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Coursework.Tests.ControllerTests
{
    public class ServiceControllerTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;

        public ServiceControllerTest(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        }

        private HttpClient GetAuthenticatedClient(int userId = 1, bool isAdmin = false)
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptorsToRemove = services
                       .Where(d =>
                           d.ServiceType.FullName != null &&
                           (d.ServiceType.FullName.Contains("DbContextOptions") ||
                            d.ImplementationType?.Namespace?.Contains("SqlServer") == true))
                       .ToList();
                    foreach (var descriptor in descriptorsToRemove)
                        services.Remove(descriptor);

                    var dbName = $"ServiceTestDb_{Guid.NewGuid()}";
                    services.AddDbContext<CourseworkDbContext>(opt =>
                        opt.UseInMemoryDatabase(dbName));

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var ctx = scope.ServiceProvider.GetRequiredService<CourseworkDbContext>();

                    ctx.Database.EnsureDeleted();
                    ctx.Database.EnsureCreated();

                    ctx.Users.AddRange(
                        new UserEntity { Id = 1, Username = "admin", Email = "admin@example.com", PasswordHash = "", Role = isAdmin ? UserRole.Admin : UserRole.Customer },
                        new UserEntity { Id = 2, Username = "user2", Email = "user2@example.com", PasswordHash = "", Role = UserRole.Customer }
                    );

                    ctx.Services.AddRange(
                        new ServiceEntity { Id = 1, Name = "Svc1", DefaultPrice = 100, DiscountPrice = 0, Description = "desc", UserId = 1 },
                        new ServiceEntity { Id = 2, Name = "Svc2", DefaultPrice = 200, DiscountPrice = 0, Description = "desc", UserId = 2 }
                    );

                    ctx.SaveChanges();
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var token = GenerateJwtToken(userId, isAdmin ? "Admin" : "User");
            client.DefaultRequestHeaders.Add("Cookie", $"jwtToken={token}");

            return client;
        }

        private string GenerateJwtToken(int userId, string role)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, role.ToLower()),
                new Claim(ClaimTypes.Role, role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("jdRJV3Cm6ytS7ecaXZAfU2WhnvsgwKF4xzbLHpPuN8qkT"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: "AuthApi",
                audience: "AuthClient",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private async Task<string> ExtractRequestVerificationToken(HttpClient client, string url)
        {
            var res = await client.GetAsync(url);
            var html = await res.Content.ReadAsStringAsync();
            _output.WriteLine($"GET {url} returned {res.StatusCode}");
            _output.WriteLine(html);

            var match = Regex.Match(html, @"<input name=""__RequestVerificationToken""[\s\S]*?value=""([^""]+)""");
            Assert.True(match.Success, "Anti-forgery token not found in form markup");
            return match.Groups[1].Value;
        }

        [Fact]
        public async Task Admin_CreateService_ReturnsRedirect()
        {
            var client = GetAuthenticatedClient(userId: 1, isAdmin: true);
            var token = await ExtractRequestVerificationToken(client, "/Service/Create");
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", token),
                new KeyValuePair<string,string>("Name", "NewSvc"),
                new KeyValuePair<string,string>("Description", "desc"),
                new KeyValuePair<string,string>("DefaultPrice", "50"),
                new KeyValuePair<string,string>("DiscountPrice", "10")
            });
            var res = await client.PostAsync("/Service/Create", form);
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
        }

        [Fact]
        public async Task Admin_DeleteService_ReturnsRedirect_AndServiceRemoved()
        {
            var client = GetAuthenticatedClient(userId: 1, isAdmin: true);
            var token = await ExtractRequestVerificationToken(client, "/Service/Create");
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", token),
                new KeyValuePair<string,string>("id", "2")
            });
            var res = await client.PostAsync("/Service/Delete/2", form);
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<CourseworkDbContext>();
            Assert.DoesNotContain(ctx.Services, s => s.Id == 2);
        }

        [Fact]
        public async Task Admin_CreateService_InvalidModel_ReturnsViewWithErrors()
        {
            var client = GetAuthenticatedClient(userId: 1, isAdmin: true);
            var token = await ExtractRequestVerificationToken(client, "/Service/Create");

            // missing Name -> expect model error, return view
            var form1 = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", token),
                new KeyValuePair<string,string>("Description", "desc"),
                new KeyValuePair<string,string>("DefaultPrice", "50"),
                new KeyValuePair<string,string>("DiscountPrice", "10")
            });
            var r1 = await client.PostAsync("/Service/Create", form1);
            Assert.Equal(HttpStatusCode.OK, r1.StatusCode);

            // negative price -> expect model error, return view
            var form2 = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", token),
                new KeyValuePair<string,string>("Name", "BadSvc"),
                new KeyValuePair<string,string>("Description", "desc"),
                new KeyValuePair<string,string>("DefaultPrice", "-1"),
                new KeyValuePair<string,string>("DiscountPrice", "0")
            });
            var r2 = await client.PostAsync("/Service/Create", form2);
            Assert.Equal(HttpStatusCode.OK, r2.StatusCode);
        }

        [Fact]
        public async Task RegisteredUser_CanCreateAndDeleteOwnService()
        {
            var client = GetAuthenticatedClient(userId: 2, isAdmin: false);
            var token = await ExtractRequestVerificationToken(client, "/Service/Create");
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", token),
                new KeyValuePair<string,string>("Name", "UserSvc"),
                new KeyValuePair<string,string>("Description", "desc"),
                new KeyValuePair<string,string>("DefaultPrice", "30"),
                new KeyValuePair<string,string>("DiscountPrice", "5")
            });
            var res = await client.PostAsync("/Service/Create", form);
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);

            var delToken = await ExtractRequestVerificationToken(client, "/Service/Create");
            var delForm = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", delToken),
                new KeyValuePair<string,string>("id", "3")
            });
            var r2 = await client.PostAsync("/Service/Delete/3", delForm);
            Assert.Equal(HttpStatusCode.Redirect, r2.StatusCode);
        }
    }
}
