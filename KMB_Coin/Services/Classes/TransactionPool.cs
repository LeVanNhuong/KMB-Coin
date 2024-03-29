﻿using KMB_Coin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KMB_Coin.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Net;
using Newtonsoft.Json;
using KMB_Coin.Utility;
using System.Collections.Concurrent;

namespace KMB_Coin.Services.Classes
{
    public class TransactionPool : ITransactionPool
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private ConcurrentDictionary<string, Transaction> _TransactionMap { get; set; }

        public TransactionPool()
        {
            this._TransactionMap = new ConcurrentDictionary<string, Transaction>();
            SyncTransactionPool();
        }

        private void SyncTransactionPool()
        {
            using var webClient = new WebClient();
            try
            {
                Logger.Info($"Getting latest Transaction Pool from peer node : {Constants.ROOT_NODE_URL}/api/transaction-pool.");
                var response = webClient.DownloadString($"{Constants.ROOT_NODE_URL}/api/transaction-pool");
                var newTransactionMap = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Transaction>>(response);
                this.SetTransactionMap(newTransactionMap);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error while getting latest Transaction Pool from peer node : {Constants.ROOT_NODE_URL}/api/transaction-pool.");
            }

        }

        public ReadOnlyDictionary<string, Transaction> TransactionMap { get { return new ReadOnlyDictionary<string, Transaction>(this._TransactionMap); } }

        public void SetTransaction(Transaction transaction)
        {
            this._TransactionMap[transaction.ID] = transaction;
        }
        private void SetTransactionMap(ConcurrentDictionary<string, Transaction> transactionMap)
        {
            this._TransactionMap = transactionMap;
        }

        public Transaction ExistingTransaction(string inputAddress)
        {
            return this._TransactionMap.Values.Where(x => x.Input.Address == inputAddress).FirstOrDefault();
        }
        public List<Transaction> ValidTransactions()
        {
            return this._TransactionMap.Values.Where(x => Transaction.Validate(x)).ToList();
        }

        public void ClearBlockchainTransaction(ReadOnlyCollection<Block> chain)
        {
            Parallel.ForEach(chain, parallelOptions: new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, block =>
              {
                  foreach (var transaction in block.Data)
                  {
                      this._TransactionMap.TryRemove(transaction.ID, out _);
                  }

              });
        }

    }
}
