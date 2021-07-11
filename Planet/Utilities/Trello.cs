using System;
using System.Threading.Tasks;
using Discord;
using Infrastructure;

namespace Planet.Utilities
{
    public class Trello
    {
        private readonly Trellos _trellos;

        public Trello(Trellos trellos, Servers servers)
        {
            _trellos = trellos;
        }

        public async Task AddTokenAsync(IUser user, string token)
        {
            await _trellos.AddToken(user.Id, token);
        }

        public async Task<string> GetTokenAsync(IUser user)
        {
            return await _trellos.GetToken(user.Id);
        }

        public async Task SetDefaultBoardAsync(IUser user, string boardId)
        {
            await _trellos.SetDefaultBoard(user.Id, boardId);
        }

        public async Task<string> GetDefaultBoardIdAsync(IUser user)
        {
            return await _trellos.GetDefaultBoardId(user.Id);
        }
    }
}
