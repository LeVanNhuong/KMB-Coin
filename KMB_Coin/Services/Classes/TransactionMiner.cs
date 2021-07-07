using KMB_Coin.Models;
using KMB_Coin.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KMB_Coin.Services.Classes
{
    public class TransactionMiner : ITransactionMiner
    {
        private readonly IBlockChain _IBlockChain;
        private readonly IRedis _IRedis;
        private readonly IWallet _IWallet;
        private readonly ITransactionPool _ITransactionPool;


        public TransactionMiner(IBlockChain blockChain, IRedis redis, IWallet wallet, ITransactionPool transactionPool)
        {  
            this._IBlockChain = blockChain;
            this._IRedis = redis;
            this._IWallet = wallet;
            this._ITransactionPool = transactionPool;
        }

        public async Task MineTransaction()
        {

            // lấy các giao dịch hợp lệ của nhóm giao dịch
            // tạo phần thưởng cho người khai thác
            // thêm một khối bao gồm các giao dịch này vào blockchain
            // broadcast block được cập nhật
            // xóa pool

            var validTransactions = _ITransactionPool.ValidTransactions();
            var minerReward = Transaction.RewardTransaction(this._IWallet);
            validTransactions.Add(minerReward);
            _IBlockChain.AddBlock(validTransactions);
            await _IRedis.BroadcastChain();
            _ITransactionPool.ClearBlockchainTransaction(_IBlockChain.LocalChain);

        }
    }
}
