using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EasyJwt.Models;

namespace EasyJwt.Services
{
    public interface IUserService
    {
        Task<bool> IsAnExistingUser(string userName);
        Task<bool> IsValidUserCredentials(string userName, string password);
        Task<List<string>> GetUserRole(string userName);

        Task<JwtUser> GetUser(string userName);

        Task<bool> AddUser(string name, string passwd);
    }

    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        
        private readonly Dictionary<string, JwtUser> _userMap = new Dictionary<string, JwtUser>();

        public UserService(ILogger<UserService> logger, List<JwtUser> userList)
        {
            _logger = logger;
            if (userList != null && userList.Any())
            {
                foreach (var jwtUser in userList)
                {
                    AddUser_Internal(jwtUser.Name, jwtUser.Password);
                }
            }
        }

        public Task<bool> AddUser(string name, string passwd)
        {
            return Task.FromResult(AddUser_Internal(name, passwd));
        }

        private bool AddUser_Internal(string name, string passwd)
        {

            var newUser = new JwtUser()
            {
                Name = name,
                Password = passwd,
                Claims = GetClaims_Internal(name),
                Roles = new List<string>()
            };
            newUser.Roles.Add(GetUserRole_Internal(name));

            _userMap.Add(name, newUser);

            return true;
        }

        private Task<List<Claim>> GetClaims(string name)
        {
            return Task.FromResult(GetClaims_Internal(name));
        }

        private List<Claim> GetClaims_Internal(string name)
        {
            var role = GetUserRole_Internal(name);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,name),
                new Claim(ClaimTypes.Role, role)
            };

            if (role == "Admin")
            {
                claims.Add(new Claim("IsAdmin", "true"));
                claims.Add(new Claim("IsBasicUser", "true"));
            }

            if (role == "BasicUser")
                claims.Add(new Claim("IsBasicUser", "true"));

            return claims;
        }

        public Task<JwtUser> GetUser(string userName)
        {
            if (_userMap.TryGetValue(userName, out var user))
            {
                return Task.FromResult(user);
            }

            return null;
        }

        public Task<bool> IsValidUserCredentials(string userName, string password)
        {
            _logger.LogInformation($"Validating user [{userName}]");
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Task.FromResult(false);
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(_userMap.TryGetValue(userName, out var p) && p.Password == password);
        }

        public Task<bool> IsAnExistingUser(string userName)
        {
            return Task.FromResult(_userMap.ContainsKey(userName));
        }

        public async Task<List<string>> GetUserRole(string userName)
        {
            var result = new List<string>();
            if (!await IsAnExistingUser(userName))
            {
                return result;
            }

            result.Add(GetUserRole_Internal(userName));

            return result;

        }

        private string GetUserRole_Internal(string userName)
        {
            if (userName.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return UserRoles.Admin;
            }

            return UserRoles.BasicUser;
        }
    }
}
