using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Shared;

namespace DziennikPlecakowy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MountainController : Controller
    {
        private readonly IMountainService _mountainService;
        private readonly IUserService _userService;
        public MountainController(IMountainService mountainService, IUserService userService)
        {
            _mountainService = mountainService;
            _userService = userService;
        }
        [HttpGet("getMountainById/{Id}")]
        public IActionResult GetMountainById(string Id)
        {
            try
            {
                return Ok(_mountainService.GetMountainById(Id));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Nie udało się pobrać góry o podanym Id.");
            }
        }
        [HttpPut("updateMountain/{mountain}")]
        public IActionResult UpdateMountain(Mountain mountain)
        {
            try 
            {
                return Ok(_mountainService.UpdateMountain(mountain));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Nie udało się zaktualizować góry.");
            }
        }
        [HttpPost("addMountain")]
        public IActionResult AddMountain([FromBody]MountainCreateRequest mountainCreateRequest)
        {
            try
            {
                var mountain = new Mountain
                {
                    Name = mountainCreateRequest.Name,
                    //Height = mountainCreateRequest.Height,
                    Location = mountainCreateRequest.Location,
                    Description = mountainCreateRequest.Description,
                    //Trails = mountainCreateRequest.Trails
                };
                return Ok(_mountainService.AddMountain(mountain));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Nie udało się dodać góry.");
            }
        }
        [HttpDelete("deleteMountain/{Id}")]
        public IActionResult DeleteMountain(string Id)
        {
            try
            {
                return Ok(_mountainService.DeleteMountain(Id));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Nie udało się usunąć góry o podanym Id.");
            }
        }
    }
}
