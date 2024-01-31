using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventTicketing.Dto;
using EventTicketing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EventTicketing.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        public IConfiguration _config { get; set; }
        public IAdminServices _adminService { get; set; }

        public AdminController(IConfiguration config, IAdminServices adminService)
        {
            _config = config;
            _adminService = adminService;
        }

        [AllowAnonymous]
        [HttpPost("/admin/login")]
        public async Task<IActionResult> login(UserDto userDto)
        {
            try
            {
                bool isLogged = _adminService.Login(userDto).Result;
                if (!isLogged)
                {
                    return BadRequest(new { status = false, message = "login failed" });
                }
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
                  _config["Jwt:Issuer"],
                  null,
                  expires: DateTime.Now.AddMinutes(120),
                  signingCredentials: credentials);

                var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

                return Ok(new { status = true, token });

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }
        [Authorize]
        [HttpGet("readXlsx_Add", Name = "ReadXlsx")]
        public ActionResult<bool> ReadXlsx()
        {
            try
            {
                _adminService.ReadXlsxFile();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(false);
            }
        }

        [Authorize]
        [HttpPost("/admin/event",Name ="AddEvent")]
        public async Task<IActionResult> AddEvents(EventDto eventDto)
        {
            try
            {
                bool isCreated = _adminService.AddEvent(eventDto).Result;
                if (isCreated)
                {
                    return Ok(new { status = true, message = "event created" });
                }
                return BadRequest(new { status = false, message = "event creation failed" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
            
        }

        [Authorize]
        [HttpPut("/admin/event", Name = "UpdateEvent")]
        public async Task<IActionResult> UpdateEvents(EventDto eventDto,int id)
        {
            try
            {
                bool isUpdated= _adminService.UpdateEvent(eventDto,id).Result;
                if (isUpdated)
                {
                    return Ok(new { status = true, message = "event updated" });
                }
                return BadRequest(new { status = false, message = "event updation failed" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }

        }

        [Authorize]
        [HttpDelete("/admin/event", Name = "DeleteEvent")]
        public async Task<IActionResult> DeleteEvents(int id)
        {
            try
            {
                bool isDeleted = _adminService.DeleteEvent(id).Result;
                if (isDeleted)
                {
                    return Ok(new { status = true, message = "event deleted" });
                }
                return BadRequest(new { status = false, message = "event deletion failed" });

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }

        }

        //for getting events
        [Authorize]
        [HttpGet("/admin/event", Name = "GetEvent")]
        public async Task<IActionResult> GetEvent(int id)
        {
            try
            {
                var data = await _adminService.GetEvent(id);
                
                return Ok(new { status = true, data});

            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }

        }
    }
}

