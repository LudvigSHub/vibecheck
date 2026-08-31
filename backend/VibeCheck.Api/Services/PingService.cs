using Microsoft.EntityFrameworkCore;
using VibeCheck.Data.Data;

namespace VibeCheck.Api.Services;

public class PingService //Service bara för att testa att flödet fungerar som det ska
    //Inte för att använda för själva applikationen sen...
{
    private readonly VibeCheckDbContext _context;

    public PingService(VibeCheckDbContext context)
    {
        _context = context;
    }

    // Hämtar ett ord ur databasen, bara för att visa att kopplingen hela
    // vägen ut till SQL faktiskt fungerar.
    public async Task<string?> GetFirstWordAsync()
    {
        return await _context.Words
            .OrderBy(w => w.WordID)
            .Select(w => w.WordDesc)
            .FirstOrDefaultAsync();
    }
}