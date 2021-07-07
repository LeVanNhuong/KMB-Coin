using KMB_Coin.Models;
using System;
using System.Collections.Generic;

namespace KMB_Coin.Services.Interfaces
{
    public interface IWallet
    {
        long Balance { get; }
        string PublicKey { get; }

        Transaction CreateTransaction(string recipient, long amount, IReadOnlyCollection<Block> chain);
        string Sign(string data);
        long CalculateBalance(IReadOnlyCollection<Block> chain, string address,DateTime timestamp);
    }
}