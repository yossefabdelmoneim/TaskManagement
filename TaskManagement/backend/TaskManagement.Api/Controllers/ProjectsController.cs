using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs.Projects;
using TaskManagement.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using TaskManagement.Api.DTOs.Common;

namespace TaskManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateProject(CreateProjectDto dto)
    {
        var project = await _projectService.CreateProjectAsync(dto);

         return CreatedAtAction(
            nameof(CreateProject),
            new { id = project.Id },
            project);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProjects([FromQuery] QueryParameters query)
    {
        var projects = await _projectService.GetAllProjectsAsync(query);

        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProjectById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);

        return Ok(project);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto dto)
    {
        var updatedProject = await _projectService.UpdateProjectAsync(id, dto);

        return Ok(updatedProject);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var isDeleted = await _projectService.DeleteProjectAsync(id);

        return NoContent();
    }
}