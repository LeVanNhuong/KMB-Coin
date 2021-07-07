using System.Threading.Tasks;

namespace KMB_Coin.Services.Interfaces
{
    public interface ITransactionMiner
    {
        Task MineTransaction();
    }
}