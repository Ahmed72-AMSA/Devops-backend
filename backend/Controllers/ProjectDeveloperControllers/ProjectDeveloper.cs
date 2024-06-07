using AutoMapper;
using developers.Data;
using developers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace developers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectDeveloperController : ControllerBase
    {
        private readonly IDataRepository<ProjectDeveloper> _projectDeveloperRepository;
        private readonly IMapper _mapper;

        public ProjectDeveloperController(IDataRepository<ProjectDeveloper> projectDeveloperRepository, IMapper mapper)
        {
            _projectDeveloperRepository = projectDeveloperRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDeveloper>> AddProjectToDeveloper([FromBody] ProjectDeveloper model)
        {
            try
            {
                var projectDeveloper = new ProjectDeveloper
                {
                    ProjectID = model.ProjectID,
                    DeveloperID = model.DeveloperID,
                    Accepted = "not yet"
                };

                await _projectDeveloperRepository.AddAsync(projectDeveloper);
                await _projectDeveloperRepository.Save();

                return CreatedAtAction(nameof(AddProjectToDeveloper), projectDeveloper);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

[HttpDelete("{projectId}/{developerId}")]
public async Task<IActionResult> RemoveProjectFromDeveloper(int projectId, int developerId)
{
    try
    {
        var projectDeveloper = await _projectDeveloperRepository.GetContext().ProjectDevelopers
            .FirstOrDefaultAsync(pd => pd.ProjectID == projectId && pd.DeveloperID == developerId);

        if (projectDeveloper == null)
            return NotFound();

        await _projectDeveloperRepository.DeleteAsync(projectDeveloper);
        await _projectDeveloperRepository.Save();

        return Ok("Project developer removed successfully.");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"An error occurred: {ex.Message}");
    }
}


[HttpGet("{projectId}/{developerId}")]
public async Task<ActionResult<ProjectDeveloper>> GetProjectDeveloper(int projectId, int developerId)
{
    try
    {
        var projectDeveloper = await _projectDeveloperRepository.GetContext().ProjectDevelopers
            .FirstOrDefaultAsync(pd => pd.ProjectID == projectId && pd.DeveloperID == developerId);

        if (projectDeveloper == null)
            return NotFound();

        return projectDeveloper;
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"An error occurred: {ex.Message}");
    }
}


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDeveloper>>> GetAllProjectDevelopers()
        {
            try
            {
                var projectDevelopers = await _projectDeveloperRepository.GetAllAsync();
                return Ok(projectDevelopers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<ProjectDeveloperDTO>>> GetProjectDevelopersView()
        {
            try
            {
                var projectDevelopers = await (
                    from pd in _projectDeveloperRepository.GetContext().ProjectDevelopers
                    join p in _projectDeveloperRepository.GetContext().Projects on pd.ProjectID equals p.ID
                    join d in _projectDeveloperRepository.GetContext().Developers on pd.DeveloperID equals d.ID
                    join a in _projectDeveloperRepository.GetContext().Users on d.UserID equals a.ID
                    select new ProjectDeveloperDTO
                    {
                        DeveloperId = d.ID,
                        ProjectId = p.ID,
                        DeveloperName = a.Name,
                        ProjectName = p.Name,
                        Accepted = pd.Accepted
                    }).ToListAsync();

                return projectDevelopers;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        



        private bool ProjectDeveloperExists(int projectId, int developerId)
        {
            return _projectDeveloperRepository.GetContext().ProjectDevelopers.Any(e => e.ProjectID == projectId && e.DeveloperID == developerId);
        }
    }
}
