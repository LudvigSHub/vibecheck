using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;

namespace VibeCheck.Api.Services;

public class AdminTagService
{
    private readonly VibeCheckDbContext _context;

    public AdminTagService(VibeCheckDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminTagListItemDTO>> GetAllAsync()
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.TagName)
            .Select(tag => new AdminTagListItemDTO
            {
                TagId = tag.TagID,
                TagName = tag.TagName
            })
            .ToListAsync();
    }
}