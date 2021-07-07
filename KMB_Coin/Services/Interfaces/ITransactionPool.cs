using KMB_Coin.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KMB_Coin.Services.Interfaces
{
    public interface ITransactionPool
    {
        void SetTransaction(Transaction transaction);
        Transaction ExistingTransaction(string inputAddress);
        ReadOnlyDictionary<string, Transaction> TransactionMap { get; }
        List<Transaction> ValidTransactions();
        void ClearBlockchainTransaction(ReadOnlyCollection<Block> chain);
    }
}