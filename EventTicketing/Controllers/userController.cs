using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventTicketing.Dto;
using EventTicketing.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventTicketing.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class userController : Controller
    {
        public IConfiguration _config { get; set; }
        public IUserServices _userService { get; set; }

        public userController(IConfiguration config, IUserServices userService)
        {
            _config = config;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("/register")]
        public async Task<IActionResult> addUser(UserDto userDto)
        {
            try
            {
                var createUser = await _userService.Register(userDto);
                return Ok(new { status = true, createUser });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpPost("/login")]
        public async Task<IActionResult> login(UserDto userDto)
        {
            try
            {
                bool isLogged = _userService.Login(userDto).Result;
                if (!isLogged)
                {
                    return BadRequest(new { status = false, message= "login failed" });
                }
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
                  _config["Jwt:Issuer"],
                  null,
                  expires: DateTime.Now.AddMinutes(120),
                  signingCredentials: credentials);

                var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

                return Ok(new {status=true , token});

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        //for searching events by Name
        [Authorize]
        [HttpGet("/SearchEvent", Name = "SearchEvent")]
        public async Task<IActionResult> SearchEvent(string name)
        {
            try
            {
                var data = await _userService.SearchEvent(name);
                if (data.Name == null)
                {
                    return BadRequest(new { status = false, messsage = "event not found" });
                }
                return Ok(new { status = true, data });

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }

        }

        //for giving feedback
        [Authorize]
        [HttpPatch("/feedback",Name ="Feedback")]
        public async Task<IActionResult> EventFeedback(int user_id,string event_name,string feedback)
        {
            try
            {
                bool data = await _userService.EventFeedback(user_id,event_name,feedback);
                if (!data)
                {
                    return BadRequest(new { status = false, messsage = "error" });
                }
                return Ok(new { status = true, message="Feedback process successfull" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }

        }
        //Registration for an event   type should be vip , regular ,early_bird
        [Authorize]
        [HttpPost("/EventRegistration", Name = "EventRegistration")]
        public async Task<IActionResult> EventRegistration(int user_id, string event_name, string event_type)
        {
            try
            {
                bool isRegistered = await _userService.EventRegistration(user_id,event_name,event_type);
                if (!isRegistered)
                {
                    return BadRequest(new { status = false, messsage = "registration failed" });
                }
                return Ok(new { status = true, message = "event registration is succesfull" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }

        }

        // for event registration cancellation
        [Authorize]
        [HttpDelete("/EventCancellation", Name = "EventCancellation")]
        public async Task<IActionResult> EventCancellation(int user_id, string event_name, string event_type)
        {
            try
            {
                bool iscancelled = await _userService.EventCancellation(user_id, event_name, event_type);
                if (!iscancelled)
                {
                    return BadRequest(new { status = false, messsage = "registration cancelation failed" });
                }
                return Ok(new { status = true, message = "registration cancelled sucessfully" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }

        }
    }
}

