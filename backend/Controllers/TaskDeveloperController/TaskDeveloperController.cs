using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using developers.Models;
using developers.DTOs;
using developers.Data; // Import your repository namespace

namespace developers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskDevelopersController : ControllerBase
    {
        private readonly IDataRepository<TaskDeveloper> _taskDeveloperRepository;

        public TaskDevelopersController(IDataRepository<TaskDeveloper> taskDeveloperRepository)
        {
            _taskDeveloperRepository = taskDeveloperRepository;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDeveloper>>> GetTaskDevelopers()
        {
            var taskDevelopers = await _taskDeveloperRepository.GetAllAsync();
            return Ok(taskDevelopers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDeveloper>> GetTaskDeveloper(int id)
        {
            var taskDeveloper = await _taskDeveloperRepository.GetByIdAsync(id);

            if (taskDeveloper == null)
            {
                return NotFound();
            }

            return taskDeveloper;
        }

        [HttpPost]
        public async Task<ActionResult<TaskDeveloper>> PostTaskDeveloper(TaskDeveloper taskDeveloper)
        {
            await _taskDeveloperRepository.AddAsync(taskDeveloper);
            await _taskDeveloperRepository.Save();

            return CreatedAtAction(nameof(GetTaskDeveloper), new { id = taskDeveloper.ID }, taskDeveloper);
        }

        [HttpPut("{taskId}/{developerId}")]
        public async Task<IActionResult> PutTaskDeveloper(int taskId, int developerId, TaskDeveloper taskDeveloper)
        {
            if (taskId != taskDeveloper.TaskId || developerId != taskDeveloper.DeveloperId)
            {
                return BadRequest();
            }

            try
            {
                await _taskDeveloperRepository.UpdateAsync(taskDeveloper);
                await _taskDeveloperRepository.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskDeveloperExists(taskId, developerId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{taskId}/{developerId}")]
        public async Task<IActionResult> DeleteTaskDeveloper(int taskId, int developerId)
        {
            var taskDeveloper = await _taskDeveloperRepository.GetContext().TaskDevelopers
                .FirstOrDefaultAsync(td => td.TaskId == taskId && td.DeveloperId == developerId);

            if (taskDeveloper == null)
            {
                return NotFound();
            }

            await _taskDeveloperRepository.DeleteAsync(taskDeveloper);
            await _taskDeveloperRepository.Save();

            return NoContent();
        }

        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<TaskDeveloperViewModel>>> GetAssignedTasks()
        {
            try
            {
                var taskDeveloperViewModels = await _taskDeveloperRepository.GetContext().TaskDevelopers
                    .Include(td => td.Task)
                        .ThenInclude(t => t.Project)
                    .Include(td => td.Developer)
                        .ThenInclude(d => d.user) // Assuming there's a navigation property from Developer to User
                    .Select(td => new TaskDeveloperViewModel
                    {
                        TaskId = td.TaskId,
                        DeveloperId = td.DeveloperId,
                        ProjectId = td.Task.ProjectID,
                        DeveloperName = td.Developer.user.Name,
                        TaskName = td.Task.Title,
                        ProjectName = td.Task.Project.Name,
                        Status = td.Status
                    })
                    .ToListAsync();

                return taskDeveloperViewModels;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


[HttpGet("files")]
public async Task<IActionResult> GetAllFiles()
{
    try
    {
        var files = await _taskDeveloperRepository.GetContext().TaskDevelopers
            .Include(td => td.Developer)
            .Include(td => td.Task)
            .Select(td => new 
            {
                DeveloperName = td.Developer.user.Name,
                TaskName = td.Task.Title,
                FilePath = td.FilePath
            })
            .ToListAsync();

        return Ok(files);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"An error occurred: {ex.Message}");
    }
}


        private async Task<bool> TaskDeveloperExists(int taskId, int developerId)
        {
            var taskDeveloper = await _taskDeveloperRepository.GetContext().TaskDevelopers
                .FirstOrDefaultAsync(td => td.TaskId == taskId && td.DeveloperId == developerId);
            return taskDeveloper != null;
        }


    }
}
