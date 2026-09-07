using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;

namespace VibeCheck.Api.Services;

public class WordStashService
{
    private readonly VibeCheckDbContext _context;

    public WordStashService(VibeCheckDbContext context)
    {
        _context = context;
    }

    public async Task<List<WordStashDTO>> GetWordsAsync(
        string? search,
        string? tag,
        int? userId)
    {
        var query = _context.Words
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(w =>
                w.WordDesc.ToLower().Contains(normalizedSearch) ||
                w.Meaning.MeaningText.ToLower().Contains(normalizedSearch) ||
                w.WordExamples.Any(e =>
                    e.ExampleText.ToLower().Contains(normalizedSearch)));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var normalizedTag = tag.Trim().ToLower();

            query = query.Where(w =>
                w.WordTags.Any(wt =>
                    wt.Tag.TagName.ToLower() == normalizedTag));
        }

        return await query
            .OrderBy(w => w.WordDesc)
            .Select(w => new WordStashDTO
            {
                WordId = w.WordID,

                Word = w.WordDesc,

                Meaning = w.Meaning.MeaningText,

                Inflections = w.WordInflections
                    .OrderBy(i => i.InflectionType.SortOrder)
                    .ThenBy(i => i.WordInflectionID)
                    .Select(i => new WordInflectionDTO
                    {
                        InflectedText = i.InflectedText,
                        TypeCode = i.InflectionType.Code,
                        TypeName = i.InflectionType.DisplayName
                    })
                    .ToList(),

                Examples = w.WordExamples
                    .OrderBy(e => e.ExampleID)
                    .Select(e => e.ExampleText)
                    .ToList(),

                Tags = w.WordTags
                    .OrderBy(wt => wt.Tag.TagName)
                    .Select(wt => new WordTagDTO
                    {
                        TagId = wt.TagID,
                        TagName = wt.Tag.TagName
                    })
                    .ToList(),

                IsInappropriate = w.WordTags.Any(wt =>
                    wt.Tag.TagName.ToLower() == "svordom" ||
                    wt.Tag.TagName.ToLower() == "förolämpning"),

                Upvotes = w.WordVotes.Count(v => v.IsPositive),

                Downvotes = w.WordVotes.Count(v => !v.IsPositive),

                CurrentUserVote = userId == null
                ? null
                : w.WordVotes
                .Where(v => v.UserID == userId)
                .Select(v => (bool?)v.IsPositive)
                .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<WordStashDTO?> GetWordByIdAsync(
    int id,
    int? userId)
    {
        return await _context.Words
            .AsNoTracking()
            .Where(w => w.WordID == id)
            .Select(w => new WordStashDTO
            {
                WordId = w.WordID,

                Word = w.WordDesc,

                Meaning = w.Meaning.MeaningText,

                Inflections = w.WordInflections
                    .OrderBy(i => i.InflectionType.SortOrder)
                    .ThenBy(i => i.WordInflectionID)
                    .Select(i => new WordInflectionDTO
                    {
                        InflectedText = i.InflectedText,
                        TypeCode = i.InflectionType.Code,
                        TypeName = i.InflectionType.DisplayName
                    })
                    .ToList(),

                Examples = w.WordExamples
                    .OrderBy(e => e.ExampleID)
                    .Select(e => e.ExampleText)
                    .ToList(),

                Tags = w.WordTags
                    .OrderBy(wt => wt.Tag.TagName)
                    .Select(wt => new WordTagDTO
                    {
                        TagId = wt.TagID,
                        TagName = wt.Tag.TagName
                    })
                    .ToList(),

                IsInappropriate = w.WordTags.Any(wt =>
                    wt.Tag.TagName.ToLower() == "svordom" ||
                    wt.Tag.TagName.ToLower() == "förolämpning"),

                Upvotes = w.WordVotes.Count(v => v.IsPositive),

                Downvotes = w.WordVotes.Count(v => !v.IsPositive),

                CurrentUserVote = userId == null
                ? null
                : w.WordVotes
                .Where(v => v.UserID == userId)
                .Select(v => (bool?)v.IsPositive)
                .FirstOrDefault()

            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<WordTagDTO>> GetTagsAsync()
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBy(t => t.TagName)
            .Select(t => new WordTagDTO
            {
                TagId = t.TagID,
                TagName = t.TagName
            })
            .ToListAsync();
    }
}
