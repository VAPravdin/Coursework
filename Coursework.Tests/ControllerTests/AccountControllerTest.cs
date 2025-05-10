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
    public class AuthControllerTest
    {
        private readonly ITestOutputHelper _output;

        public AuthControllerTest(ITestOutputHelper output)
        {
            _output = output;
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        }

        private WebApplicationFactory<Program> CreateFactory(string dbName, Action<CourseworkDbContext> seed = null)
        {
            return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services
                        .Where(d => d.ServiceType.FullName != null &&
                                    (d.ServiceType.FullName.Contains("DbContextOptions") ||
                                     d.ImplementationType?.Namespace?.Contains("SqlServer") == true))
                        .ToList();

                    descriptors.ForEach(d => services.Remove(d));

                    services.AddDbContext<CourseworkDbContext>(opt =>
                        opt.UseInMemoryDatabase(dbName));

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<CourseworkDbContext>();
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    seed?.Invoke(context);
                    context.SaveChanges();
                });
            });
        }

        private async Task<string> ExtractRequestVerificationToken(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Status code: {response.StatusCode}");
            _output.WriteLine(html);

            var match = Regex.Match(html, @"<input name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
            if (!match.Success)
                _output.WriteLine("Token not found in form.");

            return match.Groups[1].Value;
        }

        [Fact]
        public async Task Login_Post_Redirects_On_SuccessfulLogin()
        {
            var factory = CreateFactory(Guid.NewGuid().ToString(), ctx =>
            {
                ctx.Users.Add(new UserEntity
                {
                    Id = 1,
                    Username = "loginuser",
                    Email = "loginuser@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    Role = UserRole.Customer
                });
            });

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var token = await ExtractRequestVerificationToken(client, "/auth/login");

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", token),
                new KeyValuePair<string,string>("Email", "loginuser@example.com"),
                new KeyValuePair<string,string>("Password", "Password123!")
            });

            var response = await client.PostAsync("/auth/login", form);
            var responseContent = await response.Content.ReadAsStringAsync();

            _output.WriteLine(responseContent);
            _output.WriteLine($"Status code: {response.StatusCode}");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("Set-Cookie", response.Headers.ToString());
        }

        [Fact]
        public async Task Register_Post_Redirects_On_SuccessfulRegistration()
        {
            var dbName = Guid.NewGuid().ToString();
            var factory = CreateFactory(dbName);

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var token = await ExtractRequestVerificationToken(client, "/auth/register");

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", token),
                new KeyValuePair<string,string>("Username", "newuser"),
                new KeyValuePair<string,string>("Email", "newuser@example.com"),
                new KeyValuePair<string,string>("Password", "NewUserPassword123!"),
                new KeyValuePair<string,string>("ConfirmPassword", "NewUserPassword123!")
            });

            var response = await client.PostAsync("/auth/register", form);
            var responseContent = await response.Content.ReadAsStringAsync();

            _output.WriteLine(responseContent);
            _output.WriteLine($"Status code: {response.StatusCode}");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CourseworkDbContext>();
            var createdUser = context.Users.FirstOrDefault(u => u.Email == "newuser@example.com");

            Assert.NotNull(createdUser);
            Assert.Equal("newuser", createdUser.Username);
        }

        [Fact]
        public async Task Login_Post_ReturnsViewWithError_OnInvalidCredentials()
        {
            var factory = CreateFactory(Guid.NewGuid().ToString(), ctx =>
            {
                ctx.Users.Add(new UserEntity
                {
                    Id = 1,
                    Username = "loginuser",
                    Email = "loginuser@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    Role = UserRole.Customer
                });
            });

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var token = await ExtractRequestVerificationToken(client, "/auth/login");

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", token),
                new KeyValuePair<string,string>("Email", "loginuser@example.com"),
                new KeyValuePair<string,string>("Password", "WrongPassword!")
            });

            var response = await client.PostAsync("/auth/login", form);
            var html = await response.Content.ReadAsStringAsync();

            _output.WriteLine(html);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Invalid credentials", html);
        }

        [Fact]
        public async Task Register_Post_ReturnsViewWithError_WhenEmailExists()
        {
            var factory = CreateFactory(Guid.NewGuid().ToString(), ctx =>
            {
                ctx.Users.Add(new UserEntity
                {
                    Id = 2,
                    Username = "existing",
                    Email = "existing@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234!"),
                    Role = UserRole.Customer
                });
            });

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var token = await ExtractRequestVerificationToken(client, "/auth/register");

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", token),
                new KeyValuePair<string,string>("Username", "existing"),
                new KeyValuePair<string,string>("Email", "existing@example.com"),
                new KeyValuePair<string,string>("Password", "AnyPass1!"),
                new KeyValuePair<string,string>("ConfirmPassword", "AnyPass1!")
            });

            var response = await client.PostAsync("/auth/register", form);
            var html = await response.Content.ReadAsStringAsync();

            _output.WriteLine(html);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Email is already in use.", html);
        }
    }
}