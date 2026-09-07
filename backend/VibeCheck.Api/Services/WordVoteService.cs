using Microsoft.EntityFrameworkCore;
using VibeCheck.Data;
using VibeCheck.Data.Data;
using VibeCheck.Data.Models;

namespace VibeCheck.Api.Services;

public class WordVoteService
{
    private readonly VibeCheckDbContext _context;

    public WordVoteService(VibeCheckDbContext context)
    {
        _context = context;
    }

    public async Task VoteAsync(int wordId, int userId, bool isPositive)
    {
        var vote = await _context.WordVotes
            .FirstOrDefaultAsync(v => v.WordID == wordId && v.UserID == userId);

        if (vote is null)
        {
            vote = new WordVote
            {
                WordID = wordId,
                UserID = userId,
                IsPositive = isPositive
            };

            _context.WordVotes.Add(vote);
        }
        else
        {
            vote.IsPositive = isPositive;
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveVoteAsync(int wordId, int userId)
    {
        var vote = await _context.WordVotes
            .FirstOrDefaultAsync(v => v.WordID == wordId && v.UserID == userId);

        if (vote is not null)
        {
            _context.WordVotes.Remove(vote);
            await _context.SaveChangesAsync();
        }
    }
}