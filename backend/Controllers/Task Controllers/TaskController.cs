using AutoMapper;
using developers.Data;
using developers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace developers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {   private readonly IDataRepository<TaskCard> _taskRepository;
    private readonly IDataRepository<Project> _projectRepository;
    private readonly IDataRepository<ProjectDeveloper> _projectDeveloperRepository;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IDataRepository<TaskDeveloper> _taskDeveloperRepository; // Inject TaskDeveloper repository
    private readonly IMapper _mapper;

    public TaskController(
        IDataRepository<TaskCard> taskRepository,
        IDataRepository<Project> projectRepository,
        IDataRepository<ProjectDeveloper> projectDeveloperRepository,
        IDataRepository<TaskDeveloper> taskDeveloperRepository, // Inject TaskDeveloper repository
        IMapper mapper,IWebHostEnvironment hostingEnvironment)

    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _projectDeveloperRepository = projectDeveloperRepository;
        _taskDeveloperRepository = taskDeveloperRepository; // Assign TaskDeveloper repository
        _mapper = mapper;
        _hostingEnvironment = hostingEnvironment;

    }



[HttpGet]
public async Task<ActionResult<IEnumerable<TaskCard>>> GetAllTasks()
{
    var tasks = await _taskRepository.GetAllAsync();

    // Load related entities for each task
    foreach (var task in tasks)
    {
        await _taskRepository.GetContext().Entry(task)
            .Collection(t => t.TaskDevelopers)
            .LoadAsync();

        await _taskRepository.GetContext().Entry(task)
            .Reference(t => t.Project)
            .LoadAsync();
    }

    var taskViewModels = tasks.Select(task => new TaskCard
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        ProjectID = task.ProjectID,
        ImageFilePath = task.ImageFilePath, 
    });

    return Ok(taskViewModels);
}



        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] TaskDto updatedTaskDto)
        {
            var existingTask = await _taskRepository.GetByIdAsync(taskId);
            if (existingTask == null)
            {
                return NotFound("Task not found.");
            }

            var updatedTask = _mapper.Map<TaskCard>(updatedTaskDto);
            if (taskId != updatedTask.Id)
            {
                return BadRequest("Task ID in the URL does not match the ID in the request body.");
            }

            existingTask.Title = updatedTask.Title;
            existingTask.Description = updatedTask.Description;
            existingTask.ProjectID = updatedTask.ProjectID;

            await _taskRepository.UpdateAsync(existingTask);

            try
            {
                await _taskRepository.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskCardExists(taskId))
                {
                    return NotFound("Task not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

[HttpPost]
public async Task<ActionResult<TaskCard>> PostTask(TaskCard task, [FromServices] IWebHostEnvironment hostingEnvironment)
{
    try
    {
        if (task.ImageFile != null && task.ImageFile.Length > 0)
        {
            var uploadDir = Path.Combine(hostingEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(task.ImageFile.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await task.ImageFile.CopyToAsync(fileStream);
            }

            task.ImageFilePath = fileName;
        }

        var project = await _projectRepository.GetByIdAsync(task.ProjectID);

        if (project == null)
        {
            return NotFound("Project not found");
        }

        // Add the task to the repository and save changes
        _taskRepository.AddAsync(task);
        await _taskRepository.Save();

        // Retrieve the ID of the newly created task
        var newTaskId = task.Id;

        var developer = await _projectDeveloperRepository.GetContext().ProjectDevelopers
            .FirstOrDefaultAsync(pd => pd.ProjectID == project.ID && pd.Accepted == "Accepted");

        if (developer != null)
        {
            // Create TaskDeveloper entry for the task
            var taskDeveloper = new TaskDeveloper
            {
                TaskId = newTaskId, // Set TaskId property to the ID of the newly created task
                DeveloperId = developer.DeveloperID,
                Status = "in progress"
            };

            _taskDeveloperRepository.AddAsync(taskDeveloper);
            await _taskDeveloperRepository.Save();
        }

        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"An error occurred while saving task: {ex.Message}");
    }
}







[HttpGet("{taskId}")]
public async Task<ActionResult<TaskViewModel>> GetTaskById(int taskId)
{
    var task = await _taskRepository.GetByIdAsync(taskId);

    if (task == null)
    {
        return NotFound();
    }

    var developers = await _taskDeveloperRepository.GetContext().TaskDevelopers
        .Where(td => td.TaskId == taskId)
        .Include(td => td.Developer)
            .ThenInclude(d => d.user)
        .Select(td => td.Developer.user.Name)
        .ToListAsync();

    // Concatenate developer names into a single string
    var developerNames = string.Join(", ", developers);

    var taskViewModel = new TaskViewModel
    {
        TaskId = task.Id,
        TaskName = task.Title,
        TaskDescription = task.Description,
        DeveloperName = developerNames
    };

    return taskViewModel;
}





[HttpGet("ByDeveloperName/{developerName}")]
public async Task<ActionResult<IEnumerable<TaskViewModel>>> GetTasksByDeveloperName(string developerName)
{
    var tasks = await _taskRepository.GetContext().TaskCards
        .Where(t => t.TaskDevelopers.Any(td => td.Developer.user.Name == developerName))
        .ToListAsync();

    if (tasks == null || tasks.Count == 0)
    {
        return NotFound("No tasks found for the specified developer.");
    }

    var taskViewModels = new List<TaskViewModel>();

    foreach (var task in tasks)
    {
        var developers = await _taskDeveloperRepository.GetContext().TaskDevelopers
            .Where(td => td.TaskId == task.Id)
            .Include(td => td.Developer)
                .ThenInclude(d => d.user)
            .ToListAsync();

        // Concatenate developer names into a single string
        var developerNames = string.Join(", ", developers.Select(td => td.Developer.user.Name));

        var taskViewModel = new TaskViewModel
        {
            TaskId = task.Id,
            TaskName = task.Title,
            TaskDescription = task.Description,
            DeveloperName = developerNames
        };

        taskViewModels.Add(taskViewModel);
    }

    return taskViewModels;
}





[HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return NotFound();
            }

            await _taskRepository.DeleteAsync(task);
            await _taskRepository.Save();

            return NoContent();
        }





private bool TaskCardExists(int id)
        {
            return _taskRepository.GetContext().TaskCards.Any(e => e.Id == id);
        }
    }
}
