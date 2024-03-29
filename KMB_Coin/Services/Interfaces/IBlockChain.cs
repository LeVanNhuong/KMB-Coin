﻿using KMB_Coin.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KMB_Coin.Services.Interfaces
{
    public interface IBlockChain
    {
        void AddBlock(List<Transaction> data);
        bool ReplaceChain(ReadOnlyCollection<Block> chain);
        bool IsValidChain(ReadOnlyCollection<Block> chain);
        ReadOnlyCollection<Block> LocalChain { get; }
    }
}