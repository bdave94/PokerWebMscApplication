using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PokerWebAppDAL.Data;
using PokerWebAppDAL.Models;
using PokerWebApplication.Model;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PokerWebApplication.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController:ControllerBase
    {
        private UserManager<PokerAppUser> _userManager = null;
        private SignInManager<PokerAppUser> _signInManager = null;
        private readonly IConfiguration _configuration = null;
        private PokerWebAppDALContext _appDbContext = null;

        public UserController(
            UserManager<PokerAppUser> userManager,
            SignInManager<PokerAppUser> signInManager, IConfiguration configuration,
            PokerWebAppDALContext appDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _appDbContext = appDbContext;
        }


        // POST api/user
        [HttpPost]
        public async Task<object> Post([FromBody]RegisterUserModel model)
        {

            var user = new PokerAppUser()
            {
                UserName = model.Username,
                Email = model.Email,
                Money = 1000                    
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
               
                await _appDbContext.Statistics.AddAsync(new Statistics { UserId = user.Id, TotalGamesPlayed=0,TotalWins=0 });
                await _appDbContext.SaveChangesAsync();

                return new OkObjectResult(new string[] { "User Successfuly created!" });
               

            }
            else
            {
                string desc = " ";
                var firstError = result.Errors.First();
               
                desc = firstError.Description;

                return  BadRequest(desc);

            }
        }


        [HttpPost("login")]
        public async Task<object> Login([FromBody] LoginUserModel model)
        {
           
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.Username);

                var token = await GenerateJwtToken(model.Username, appUser);
                
                return  new LoginResponse() { Auth_Token =  token };
            }
            else
            {
                return BadRequest("Invalid username or password.");
            }
        }



        [Authorize]
        [HttpGet("profile")]
        public async Task<object> Protected()
        {
            var username = User.Identity.Name;
            var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == username);
            var userStatistics = _appDbContext.Statistics.SingleOrDefault(s => s.UserId == appUser.Id);
            return new ProfileInfo() { UserName= username, Money = appUser.Money,
                GamesPlayed =userStatistics.TotalGamesPlayed,
                TotalWins = userStatistics.TotalWins }; 
        }


        private async Task<string> GenerateJwtToken(string username, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, username)

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
