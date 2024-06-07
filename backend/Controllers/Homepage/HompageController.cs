using AutoMapper;
using developers.Data;
using developers.DTOs;
using developers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace developers.Controllers

{
    // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HomepageController : ControllerBase
    {
        private readonly IDataRepository<ProjectDeveloper> _projectDeveloperRepository;
        private readonly IDataRepository<Developer> _developerRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;


        private readonly IDataRepository<TaskDeveloper> _taskDeveloperRepository;


        public HomepageController(IDataRepository<ProjectDeveloper> projectDeveloperRepository, 
                        IDataRepository<Developer> developerRepository,IDataRepository<TaskDeveloper> taskDeveloperRepository , IWebHostEnvironment hostingEnvironment)
        {
            _projectDeveloperRepository = projectDeveloperRepository;
            _developerRepository = developerRepository;
            _taskDeveloperRepository = taskDeveloperRepository;
            _hostingEnvironment = hostingEnvironment;

        }

        [HttpGet("projects/{userId}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjectsForDeveloper(int userId)
        {
            try
            {
                var developer = await _developerRepository.GetContext().Developers
                    .FirstOrDefaultAsync(d => d.UserID == userId);

                if (developer == null)
                    return NotFound("Developer not found.");

                // Retrieve projects associated with the DeveloperID
                var projects = await (from pd in _projectDeveloperRepository.GetContext().ProjectDevelopers
                                    join p in _projectDeveloperRepository.GetContext().Projects on pd.ProjectID equals p.ID
                                    where pd.DeveloperID == developer.ID
                                    select p).ToListAsync();

                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }




[HttpPost("AcceptProject")]
public async Task<IActionResult> AcceptProject([FromBody] ProjectDeveloper projectDeveloper)
{
    try
    {

        var existingProjectDeveloper = await _projectDeveloperRepository.GetContext().ProjectDevelopers
            .FirstOrDefaultAsync(pd => pd.ProjectID == projectDeveloper.ProjectID && pd.DeveloperID == projectDeveloper.DeveloperID && pd.Accepted == "Accepted");

        if (existingProjectDeveloper != null)
            return BadRequest("Project has already been accepted by the developer.");

        existingProjectDeveloper = await _projectDeveloperRepository.GetContext().ProjectDevelopers
            .FirstOrDefaultAsync(pd => pd.ProjectID == projectDeveloper.ProjectID && pd.DeveloperID == projectDeveloper.DeveloperID);

        if (existingProjectDeveloper == null)
            return NotFound("Project developer not found.");

        existingProjectDeveloper.Accepted = "Accepted";
        await _projectDeveloperRepository.GetContext().SaveChangesAsync();

        var project = await _projectDeveloperRepository.GetContext().Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.ID == projectDeveloper.ProjectID);

        // Check if the project exists
        if (project == null)
            return NotFound("Project not found.");

        // Assign tasks to the developer
        foreach (var task in project.Tasks)
        {
            var taskDeveloper = new TaskDeveloper
            {
                TaskId = task.Id,
                DeveloperId = projectDeveloper.DeveloperID,
                Status = "in progress"
            };
            _projectDeveloperRepository.GetContext().TaskDevelopers.Add(taskDeveloper);
        }
        await _projectDeveloperRepository.GetContext().SaveChangesAsync();

        return Ok("Project accepted and tasks assigned.");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"An error occurred: {ex.Message}");
    }
}


 [HttpGet("tasks/{developerId}")]
        public async Task<ActionResult<IEnumerable<TaskUserViewModel>>> GetTaskDetailsByDeveloper(int developerId)
        {
            try
            {
                var taskDetails = await _taskDeveloperRepository.GetContext().TaskDevelopers
                    .Where(td => td.DeveloperId == developerId)
                    .Include(td => td.Task)
                        .ThenInclude(t => t.Project)
                    .Select(td => new TaskUserViewModel
                    {
                        TaskId = td.TaskId,
                        TaskTitle = td.Task.Title,
                        TaskStatus = td.Status,
                        ProjectName = td.Task.Project.Name,
                        TaskDescription = td.Task.Description,
                        TaskImage = td.Task.ImageFilePath
                    })
                    .ToListAsync();

                return taskDetails;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("alltasks")]
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
                        Status = td.Status,
                        taskImage = td.Task.ImageFilePath

                    })
                    .ToListAsync();

                return taskDeveloperViewModels;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


[HttpPut("taskSubmission/{taskId}/{developerId}")]
public async Task<IActionResult> UpdateTaskDeveloper(int taskId, int developerId, [FromForm] TaskDeveloper model)
{
    try
    {
        if (model.File != null)
        {
            string fileExtension = Path.GetExtension(model.File.FileName).ToLower();
            if (fileExtension != ".pdf")
            {
                return BadRequest("Invalid file format. Please upload a .pdf file.");
            }

            var wwwRootPath = _hostingEnvironment.WebRootPath;

            var uploadsDirectory = Path.Combine(wwwRootPath, "uploads");
            var fileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(uploadsDirectory, fileName);

            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var taskDeveloper = await _taskDeveloperRepository.GetContext().TaskDevelopers
                .FirstOrDefaultAsync(td => td.TaskId == taskId && td.DeveloperId == developerId);

            if (taskDeveloper == null)
            {
                return NotFound("Task developer not found.");
            }

            taskDeveloper.FilePath = Path.Combine("uploads", fileName); 

            taskDeveloper.Status = "done";

            await _taskDeveloperRepository.GetContext().SaveChangesAsync();
        }

        // Return success message
        return Ok("Task developer updated successfully.");
    }
    catch (Exception ex)
    {
        // Return internal server error if any exception occurs
        return StatusCode(500, $"An error occurred: {ex.Message}");
    }
}


















    }
}




    

    



