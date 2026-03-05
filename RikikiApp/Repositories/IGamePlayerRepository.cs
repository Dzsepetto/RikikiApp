using RikikiApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikikiApp.Repositories
{
    public interface IGamePlayerRepository
    {
        Task AddAsync(GamePlayer gp);
        Task<List<GamePlayer>> GetByGameIdAsync(int gameId);
        Task UpdateAsync(GamePlayer gp);
        Task DeleteAsync(int id);
    }
}
