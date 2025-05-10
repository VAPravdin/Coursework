using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Coursework.Entities;
using Coursework.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Text.RegularExpressions;



namespace Coursework.Tests.ControllerTests
{
    public class OrderControllerTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;

        public OrderControllerTest(WebApplicationFactory<Program> factory, ITestOutputHelper output)
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
                           (
                               d.ServiceType.FullName.Contains("DbContextOptions") ||
                               d.ImplementationType?.Namespace?.Contains("SqlServer") == true
                           ))
                       .ToList();

                    foreach (var descriptor in descriptorsToRemove)
                    {
                        services.Remove(descriptor);
                    }
                    var databaseName = $"TestDb_{Guid.NewGuid()}";
                    services.AddDbContext<CourseworkDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(databaseName);
                    });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<CourseworkDbContext>();

                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.Users.Add(new UserEntity { Id = 1, Username = "user1", Email = "user1@gmail.com", PasswordHash = "user1hash", Role = UserRole.Customer });
                    context.Users.Add(new UserEntity { Id = 2, Username = "user2", Email = "user2@gmail.com", PasswordHash = "user2hash", Role = UserRole.Customer });

                    context.Services.AddRange(
                        new ServiceEntity { Id = 1, Name = "Service A", DefaultPrice = 100, DiscountPrice = 80, Description = "descriptionforservice1", UserId = 1 },
                        new ServiceEntity { Id = 2, Name = "Service B", DefaultPrice = 200, DiscountPrice = 0, Description = "descriptionforservice2", UserId = 2 }
                    );

                    context.Orders.Add(new OrderEntity
                    {
                        Id = 1,
                        UserId = 1,
                        OrderDate = DateTime.UtcNow,
                        Price = 280
                    });

                    context.OrderServices.AddRange(
                        new OrderServiceEntity { OrderId = 1, ServiceId = 1, PriceAtPurchase = 80 },
                        new OrderServiceEntity { OrderId = 1, ServiceId = 2, PriceAtPurchase = 200 }
                    );

                    context.SaveChanges();
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
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, role)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("jdRJV3Cm6ytS7ecaXZAfU2WhnvsgwKF4xzbLHpPuN8qkT"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "AuthApi",
                audience: "AuthClient",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task<string> ExtractRequestVerificationToken(HttpClient client, string formUrl)
        {
            var getResponse = await client.GetAsync(formUrl);
            var html = await getResponse.Content.ReadAsStringAsync();

            _output.WriteLine($"Status code: {getResponse.StatusCode}");
            _output.WriteLine(html);

            var match = Regex.Match(html, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />");
            if (!match.Success)
            {
                _output.WriteLine("Token not found in form.");
            }

            return match.Groups[1].Value;
        }

        [Fact]
        public async Task Index_ReturnsOrdersForAuthenticatedUser()
        {
            var client = GetAuthenticatedClient(userId: 1);
            var response = await client.GetAsync("/Order/Index");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("280", content);
        }

        [Fact]
        public async Task Details_ReturnsOrderAndServices_WhenExists()
        {
            var client = GetAuthenticatedClient(userId: 1);
            var response = await client.GetAsync("/Order/Details/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine(content);
            Assert.Contains("Service A", content);
            Assert.Contains("Service B", content);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            var client = GetAuthenticatedClient(userId: 1);
            var response = await client.GetAsync("/Order/Details/999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_Post_ReturnsRedirect_WhenServicesSelected()
        {
            var client = GetAuthenticatedClient(userId: 1);
            var token = GenerateJwtToken(1, "User");
            client.DefaultRequestHeaders.Add("Cookie", $"jwtToken={token}");
            var antiforgeryToken = await ExtractRequestVerificationToken(client, "/Order/Create");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken),
                new KeyValuePair<string, string>("selectedServiceIds", "1"),
                new KeyValuePair<string, string>("selectedServiceIds", "2")
            });
            var response = await client.PostAsync("/Order/Create", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseContent);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Delete_RemovesOrder_WhenUserIsOwner()
        {
            var client = GetAuthenticatedClient(userId: 1);

            var antiforgeryToken = await ExtractRequestVerificationToken(client, "/Order/Create");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken),
                new KeyValuePair<string, string>("id", "1")
            });
            var response = await client.PostAsync("/Order/Delete/1", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine(responseContent);
            _output.WriteLine($"Status code: {response.StatusCode}");
            _output.WriteLine($"Headers: {response.Headers}");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsForbid_WhenUserIsNotOwnerOrAdmin()
        {
            var client = GetAuthenticatedClient(userId: 2);
            var antiforgeryToken = await ExtractRequestVerificationToken(client, "/Order/Create");
            var response = await client.PostAsync("/Order/Delete/1", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("__RequestVerificationToken",
                antiforgeryToken), new KeyValuePair<string, string>("id", "1")
            }));
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DownloadJson_ReturnsFile_WithCorrectContent()
        {
            var client = GetAuthenticatedClient(userId: 1);
            var response = await client.GetAsync("/Order/DownloadJson/1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("Service A", json);
            Assert.Contains("DiscountPrice", json);
        }
    }
}
