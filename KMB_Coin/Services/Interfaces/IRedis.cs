using KMB_Coin.Models;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace KMB_Coin.Services.Interfaces
{
    public interface IRedis
    {

        IDatabase DB0 { get; }

        IDatabase GetDB(int db = 0);
        Task BroadcastChain();
        Task BroadcastTransaction(Transaction transaction);
    }
}