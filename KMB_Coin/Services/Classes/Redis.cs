﻿using KMB_Coin.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KMB_Coin.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using KMB_Coin.Utility;
using System.Collections.ObjectModel;

namespace KMB_Coin.Services.Classes
{
    public class Redis : Interfaces.IRedis
    {
        private readonly Lazy<ConnectionMultiplexer> LazyConnection;

        private readonly ILogger<Redis> _ILogger;
        private readonly IBlockChain _IBlockChain;
        private readonly ITransactionPool _ITransactionPool;
        

        public Redis(ILogger<Redis> logger, IBlockChain blockChain, ITransactionPool transactionPool)//, IClock clock)
        {
            this._ILogger = logger;
            this._IBlockChain = blockChain;
            this._ITransactionPool = transactionPool;
            ConfigurationOptions options = new ConfigurationOptions()
            {
                ClientName = Constants.APP_NANE,
                SyncTimeout = Constants.REDIS_TIMEOUT_IN_MILLISEC,
                AbortOnConnectFail = Constants.REDIS_ABORT_ON_CONNECT_FAIL,
                ConnectRetry = Constants.REDIS_CONNECT_RETRY_LIMIT,
                EndPoints = { { Constants.REDIS_SERVER, Constants.REDIS_PORT } },
                ReconnectRetryPolicy = new ExponentialRetry(5000),
                Password = Constants.REDIS_PASSWORD
       
            };
            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));

            SubscribeToChannels();
        }
        private ConnectionMultiplexer Connection => LazyConnection.Value;

        private ISubscriber Subscriber => Connection.GetSubscriber();

        public IDatabase DB0 => Connection.GetDatabase(0);
        public IDatabase GetDB(int db = 0) => Connection.GetDatabase(db);

        private void PublishToChannel(string channelName, string message)
        {
            this.Subscriber.Publish(channelName, (RedisValue)message);
        }

        private async Task PublishToChannelAsync(string channelName, string message)
        {
            await this.Subscriber.PublishAsync(string.Concat(Constants.PUBSUB_CHANNEL_PPREFIX,"-", channelName) , message);
        }
        private void SubscribeToChannels()
        {
            foreach (var channelInfo in typeof(Constants.PUBSUB_CHANNEL).GetProperties())
            {
                _ILogger.LogInformation($"Subscribing to Channel : {channelInfo.Name}");

                this.Subscriber.Subscribe(string.Concat("*","-",channelInfo.Name)).OnMessage(channelMessage =>
                {

                    if (((string)channelMessage.Channel).StartsWith(Constants.PUBSUB_CHANNEL_PPREFIX))
                    {
                        _ILogger.LogInformation($"Discarded Self Published Message from Channel : {(string)channelMessage.Channel}");
                        return;
                    }
                    _ILogger.LogInformation($"Channel : {(string)channelMessage.Channel} => {(string)channelMessage.Message} ");
                    
                    switch (((string)channelMessage.Channel).Substring(37, ((string)channelMessage.Channel).Length - 37))
                    {

                        case "BLOCKCHAIN":
                            try
                            {
                                var newChain = JsonConvert.DeserializeObject<ReadOnlyCollection<Block>>(channelMessage.Message);
                                if (this._IBlockChain.ReplaceChain(newChain))
                                    _ITransactionPool.ClearBlockchainTransaction(this._IBlockChain.LocalChain);

                            }
                            catch (Exception ex)
                            {
                                _ILogger.LogError(ex, $"Error while processing a message from Channel : BLOCKCHAIN");
                            }
                            break;
                        case "TRANSACTION":
                            try
                            {
                                var newTransaction = JsonConvert.DeserializeObject<Transaction>(channelMessage.Message);
                                this._ITransactionPool.SetTransaction(newTransaction); 
                            }
                            catch (Exception ex)
                            {
                                _ILogger.LogError(ex, $"Error while processing a message from Channel : TRANSACTION");
                            }
                            break;
                        default:
                            _ILogger.LogInformation($"Discarded a message from Channel : {(string)channelMessage.Channel}");
                            break;
                    }

                });
            }
        }
        public async Task BroadcastChain()
        {
            await this.PublishToChannelAsync(Constants.PUBSUB_CHANNEL.BLOCKCHAIN, this._IBlockChain.LocalChain.SerializeObject());
        }
        public async Task BroadcastTransaction(Transaction transaction)
        {
            await this.PublishToChannelAsync(Constants.PUBSUB_CHANNEL.TRANSACTION, transaction.SerializeObject());
        }
    }
}
