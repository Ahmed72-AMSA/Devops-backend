using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using developers.Models;
using developers.Data;
using Microsoft.AspNetCore.Authorization;


namespace developers.Controllers
{
    // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IDataRepository<Project> _projectRepository;

        public ProjectController(IDataRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        [Authorize(Roles = "Admin")]         
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var projects = await _projectRepository.GetAllAsync();
            return Ok(projects);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }

        [Authorize(Roles = "Admin")]         
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _projectRepository.AddAsync(project);
            await _projectRepository.Save();

            return CreatedAtAction(nameof(GetProject), new { id = project.ID }, project);
        }

        [Authorize(Roles = "Admin")] 
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
            if (id != project.ID)
            {
                return BadRequest("ID in URL does not match the ID in the request body");
            }

            await _projectRepository.UpdateAsync(project);
            var success = await _projectRepository.Save();

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin")]         
        [HttpDelete("{id}")]
        public async Task<ActionResult<Project>> DeleteProject(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            await _projectRepository.DeleteAsync(project);
            var success = await _projectRepository.Save();

            if (!success)
            {
                return StatusCode(500, "Failed to delete project");
            }

            return project;
        }
    }
}
