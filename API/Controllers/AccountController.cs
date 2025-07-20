using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Services;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Object>> Register(RegisterDto registerDto)
        {
            if (await UserExist(registerDto.UserName)) return BadRequest("Username Is Taken");
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.UserName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseDto>> Login(RegisterDto loginCred)
        {
            var userFromDb = await _db.Users.SingleOrDefaultAsync(x => x.UserName == loginCred.UserName);

            if (userFromDb == null) return Unauthorized("Invalid UserName");
            using var hmac = new HMACSHA512(userFromDb.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginCred.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != userFromDb.PasswordHash[i])
                    return Unauthorized("Invalid Password");
            }
            var userResponse = new ResponseDto
            {
                UserName = userFromDb.UserName,
                Token = _tokenService.CreateToken(userFromDb)
            };
            return userResponse; 
            
        }

        private async Task<bool> UserExist(string userName)
        {
            return await _db.Users.AnyAsync(x => x.UserName.ToLower() == userName.ToLower());
        }
    }
}