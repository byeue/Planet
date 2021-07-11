using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class Trellos
    {
        private readonly PlanetContext _context;

        public Trellos(PlanetContext context)
        {
            _context = context;
        }

        public async Task<string> GetToken(ulong userId)
        {
            var token = await _context.Trellos
                .Where(x => x.UserId == userId)
                .Select(x => x.Token)
                .FirstOrDefaultAsync();

            return await Task.FromResult(token.ToString()) ;
        }

        public async Task AddToken(ulong userId, string token)
        {
            var trello = await _context.Trellos
                .Where(x => x.UserId == userId)
                .FirstOrDefaultAsync();

            if (trello == null)
                _context.Add(new Trello { UserId = userId, Token = token });
            else
                trello.Token = token;

            await _context.SaveChangesAsync();
        }

        public async Task SetDefaultBoard(ulong userId, string boardId)
        {
            var trello = await _context.Trellos
                .Where(x => x.UserId == userId)
                .FirstOrDefaultAsync();

            if (trello == null)
                _context.Add(new Trello { UserId = userId, BoardId = boardId });
            else
                trello.BoardId = boardId;

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetDefaultBoardId(ulong userId)
        {
            var boardId = await _context.Trellos
                .Where(x => x.UserId == userId)
                .Select(x => x.BoardId)
                .FirstOrDefaultAsync();

            return await Task.FromResult(boardId.ToString());
        }
    }
}
