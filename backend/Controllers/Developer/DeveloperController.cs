using AutoMapper;
using developers.Data;
using developers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace developers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly IDataRepository<Developer> _developerRepository;
        private readonly IMapper _mapper;

        public DeveloperController(IDataRepository<Developer> developerRepository, IMapper mapper)
        {
            _developerRepository = developerRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeveloperDTO>>> GetDevelopersAsync()
        {
            var developers = await _developerRepository.GetAllAsync();
            var developerDTOs = _mapper.Map<IEnumerable<DeveloperDTO>>(developers);
            return Ok(developerDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeveloperDTO>> GetDeveloperAsync(int id)
        {
            var developer = await _developerRepository.GetByIdAsync(id);
            if (developer == null)
            {
                return NotFound();
            }
            var developerDTO = _mapper.Map<DeveloperDTO>(developer);
            return Ok(developerDTO);
        }

        [HttpPost]
        public async Task<ActionResult<DeveloperDTO>> AddDeveloperAsync(DeveloperDTO developerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var developer = _mapper.Map<Developer>(developerDTO);
            await _developerRepository.AddAsync(developer);
            await _developerRepository.Save();
            return CreatedAtAction(nameof(GetDeveloperAsync), new { id = developer.ID }, _mapper.Map<DeveloperDTO>(developer));
        }

        [HttpGet("withName")]
        public async Task<ActionResult<IEnumerable<DeveloperDTO>>> GetDevelopersWithName()
        {
            var dbContext = _developerRepository.GetContext();
            var developers = await (
                from dev in dbContext.Set<Developer>()
                join user in dbContext.Set<User>() on dev.UserID equals user.ID
                select new DeveloperDTO
                {
                    ID = dev.ID,
                    Name = user.Name
                }
            ).ToListAsync();

            return developers;
        }
    }
}
