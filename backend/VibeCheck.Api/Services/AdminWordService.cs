using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;

namespace VibeCheck.Api.Services;

public class AdminWordService
{
    private readonly VibeCheckDbContext _context;

    public AdminWordService(VibeCheckDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminWordListItemDTO>> GetAllAsync()
    {
        return await _context.Words
            .AsNoTracking()
            .OrderBy(word => word.WordDesc)
            .Select(word => new AdminWordListItemDTO
            {
                WordId = word.WordID,
                Word = word.WordDesc,
                Meaning = word.Meaning.MeaningText,
                ExampleCount = word.WordExamples.Count(),

                Tags = word.WordTags
                    .OrderBy(wordTag => wordTag.Tag.TagName)
                    .Select(wordTag => new AdminTagListItemDTO
                    {
                        TagId = wordTag.TagID,
                        TagName = wordTag.Tag.TagName
                    })
                    .ToList(),

                IsUsedInQuiz = word.Questions.Any()
            })
            .ToListAsync();
    }
}