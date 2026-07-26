using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Data;
using TaskManagement.Api.DTOs.Projects;
using TaskManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives; // Needed for cache invalidation token
using TaskManagement.Api.Exceptions;
using TaskManagement.Api.DTOs.Common;
using TaskManagement.Api.Controllers;

namespace TaskManagement.Api.Services;

public class ProjectService : IProjectService
{
    private const string ProjectsCacheKey = "projects";

    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    // Used to invalidate ALL cached project pages at once.
    // Instead of deleting every cache key manually, we change this token.
    private CancellationTokenSource _cacheResetToken = new();

    public ProjectService(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        _context = appDbContext;
        _cache = memoryCache;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Projects.Add(project);

        await _context.SaveChangesAsync();

        // Invalidate ALL cached project pages.
        InvalidateProjectsCache();

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt
        };
    }

    public async Task<PagedResponse<ProjectDto>> GetAllProjectsAsync(QueryParameters query)
    {
        // Cache key includes ALL query parameters.
        // Different searches/sorts/pages should not share the same cache.
        var cacheKey =
            $"{ProjectsCacheKey}" +
            $"_page_{query.Page}" +
            $"_size_{query.PageSize}" +
            $"_search_{query.Search}" +
            $"_sort_{query.SortBy}" +
            $"_direction_{query.SortDirection}";

        
        // Cache stores the final List<ProjectDto>, not IQueryable<Project>.
        if (_cache.TryGetValue(cacheKey, out PagedResponse<ProjectDto>? cachedProjects))
        {
            Console.WriteLine("Returned from cache");
            return cachedProjects!;
        }

    
        // Build the SQL query step by step.
        // Nothing is executed yet.
        IQueryable<Project> projects = _context.Projects;

       
        // Apply searching if a search value exists.
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            projects = projects.Where(p =>
                p.Name.Contains(query.Search));
        }

       
        // Apply sorting before pagination.
        projects = query.SortBy?.ToLower() switch
        {
            "name" =>
                query.SortDirection?.ToLower() == "desc"
                    ? projects.OrderByDescending(p => p.Name)
                    : projects.OrderBy(p => p.Name),

            "createdat" =>
                query.SortDirection?.ToLower() == "desc"
                    ? projects.OrderByDescending(p => p.CreatedAt)
                    : projects.OrderBy(p => p.CreatedAt),

            
            // Default ordering to guarantee consistent pagination.
            _ => projects.OrderBy(p => p.Id)
        };


        var totalItems = await projects.CountAsync();
    
        // Pagination comes AFTER filtering and sorting.
        projects = projects
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

        var totalPages =(int)Math.Ceiling(totalItems / (double)query.PageSize);

        // Execute the SQL query and project directly to DTOs.
        var result = await projects
            .Select(project => new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            })
            .ToListAsync();

        Console.WriteLine("Returned from database");

        
        // Cache the FINAL RESULT (List<ProjectDto>),
        // not the IQueryable<Project>.
        var paged = new PagedResponse<ProjectDto>
        {
            Items = result,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNextPage = query.Page < totalPages,
            HasPreviousPage = query.Page > 1
        };

        _cache.Set(
            cacheKey,
            paged,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),

                
                // Every cached query depends on this token.
                // When Create/Update/Delete occurs, all cached queries expire.
                ExpirationTokens =
                {
                    new CancellationChangeToken(_cacheResetToken.Token)
                }
            });

            return paged;
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        // UPDATED:
        // One database query instead of two.
        var project = await _context.Projects
            .Where(p => p.Id == id)
            .Select(project => new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {id} was not found.");
        }

        return project;
    }

    public async Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto dto)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {id} was not found.");
        }

        project.Name = dto.Name;
        project.Description = dto.Description;

        await _context.SaveChangesAsync();

        // Invalidate all cached project pages.
        InvalidateProjectsCache();

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt
        };
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {id} was not found.");
        }

        _context.Projects.Remove(project);

        await _context.SaveChangesAsync();

        // Invalidate all cached project pages.
        InvalidateProjectsCache();

        return true;
    }

    
    // Centralized cache invalidation.
    // Cancels the current token, which expires every cached page,
    // then creates a new token for future cache entries.
    private void InvalidateProjectsCache()
    {
        _cacheResetToken.Cancel();
        _cacheResetToken.Dispose();
        _cacheResetToken = new CancellationTokenSource();
    }
}