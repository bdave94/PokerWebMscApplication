using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PokerWebApp.DAL.Models;
using PokerWebApp.DAL.Manager;
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
        

        public UserController(
            UserManager<PokerAppUser> userManager,
            SignInManager<PokerAppUser> signInManager, 
            IConfiguration configuration
             )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            
        }


        // POST api/user
        [HttpPost]
        public async Task<object> Post([FromBody]RegisterUserModel model)
        {

            var user = new PokerAppUser()
            {
                UserName = model.Username,
                Email = model.Email,
                Money = 1000,              
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                
                PokerAppDBManager manager = new PokerAppDBManager();
               
                await manager.InsertNewUserAsync(user);
                
                return new OkObjectResult(new string[] { "User Successfuly created!" });
               
            }
            else
            {
                return  BadRequest(result.Errors.First().Description);
            }
        }


        [HttpPost("login")]
        public async Task<object> Login([FromBody] LoginUserModel model)
        {
           
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.Username);

                var token =  GenerateJwtToken(model.Username, appUser);
                
                return  new LoginResponse() { Auth_Token =  token };
            }
            else
            {
                return BadRequest("Invalid username or password.");
            }
        }



        [Authorize]
        [HttpGet("profile")]
        public async Task<ProfileInfo> Protected()
        {
            var appUser = await _userManager.FindByNameAsync(User.Identity.Name);
            PokerAppDBManager manager = new PokerAppDBManager();
            Statistics st = await manager.GetStatisticsAsync(appUser);

            return new ProfileInfo() { UserName= appUser.UserName,
                Money = appUser.Money,
                GamesPlayed = st.TotalGamesPlayed,
                TotalWins = st.TotalWins }; 
        }


        private string GenerateJwtToken(string username, IdentityUser user)
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
