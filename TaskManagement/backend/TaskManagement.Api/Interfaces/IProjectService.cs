using TaskManagement.Api.DTOs.Projects;
using TaskManagement.Api.DTOs.Common;

namespace TaskManagement.Api.Interfaces;

public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto);
    Task<PagedResponse<ProjectDto>> GetAllProjectsAsync(QueryParameters query);
    Task<ProjectDto?> GetProjectByIdAsync(int id);
    Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto dto);
    Task<bool> DeleteProjectAsync(int id);
}