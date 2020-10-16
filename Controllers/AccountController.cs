using DatingAPI.Common;
using DatingAPI.Common.Interfaces;
using DatingAPI.Common.Services;
using DatingAPI.Data;
using DatingAPI.DTOs;
using DatingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DatingAPI.Controllers
{
    public class AccountController : ApiControllerBase
    {
        private DataContext _context;
        private TokenService _tokenService;

        public AccountController(DataContext context, TokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await IsUserExist(registerDto.Username)) return BadRequest("User name has been taken");
            using var hmac = new HMACSHA256();
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            var passwordSalt = hmac.Key;
            var user = new UserModel(registerDto.Username.ToLower(), passwordHash, passwordSalt);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.Name,
                Token = _tokenService.CreateToken(user),
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDtos)
        {
            var existedUser = await _context.Users.SingleOrDefaultAsync(ele => ele.Name == loginDtos.Username);
            if (existedUser == null) return Unauthorized("Invalid username");
            using var hmac = new HMACSHA256(existedUser.PasswordSalt);
            var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDtos.Password));
            for (int i = 0; i < passwordHash.Length; i++)
            {
                if (passwordHash[i] != existedUser.PasswordHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }
            return new UserDto
            {
                Username = existedUser.Name,
                Token = _tokenService.CreateToken(existedUser),
            };
        }

        private async Task<bool> IsUserExist(string username)
        {
            return await _context.Users.AnyAsync(user => user.Name == username.ToLower());
        }
    }
}
