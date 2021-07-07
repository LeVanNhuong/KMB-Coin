using KMB_Coin.Services.Classes;
using KMB_Coin.Services.Interfaces;
using KMB_Coin.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KMB_Coin.Models
{
    public class Transaction
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly Clock clock = new Clock();

        public string ID { get; set; }
        public Dictionary<string, long> OutputMap { get; set; }
        public Input Input { get; set; }

        public Transaction()
        {

        }
        public Transaction(IWallet senderWallet, string recipient, long amount)
        {
            //Console.WriteLine("Transaction1");
            this.ID = Guid.NewGuid().ToString();
            this.OutputMap = this.CreateOutputMap(senderWallet, recipient, amount);
            this.Input = this.CreateInput(senderWallet, this.OutputMap);
        }
        public Transaction(Dictionary<string, long> outputMap, Input input)
        {
            this.ID = Guid.NewGuid().ToString();
            this.OutputMap = outputMap;
            this.Input = input;

        }

        private Dictionary<string, long> CreateOutputMap(IWallet senderWallet, string recipient, long amount)
        {
            var outputMap = new Dictionary<string, long>();
            outputMap[recipient] = amount;
            outputMap[senderWallet.PublicKey] = senderWallet.Balance - amount;
            return outputMap;
        }
        private Input CreateInput(IWallet senderWallet, Dictionary<string, long> outputMap)
        {
            Console.WriteLine("CreateInput");
            Input input = new Input
            {
                Timestamp = clock.UtcNow,
                Amount = senderWallet.Balance,
                Address = senderWallet.PublicKey,
                Signature = senderWallet.Sign(outputMap.SerializeObject())

            };
            Console.WriteLine("outputMap.SerializeObject()=" + outputMap.SerializeObject().ToString());
            Console.WriteLine("input.Signature="+input.Signature);

            return input;
        }

        public static bool Validate(Transaction transaction)
        {
            var outputTotal = transaction.OutputMap.Values.Sum();
            if (transaction.Input.Amount != outputTotal)
            {
                Logger.Info($"Invalid transaction from {transaction.Input.Address}");
                return false;
            }
            if (!EllipticCurve.VerifySignature(publicKey: transaction.Input.Address, data: transaction.OutputMap.SerializeObject(), signature: transaction.Input.Signature))
            {
                Logger.Info($"Invalid signature from {transaction.Input.Address}");
                return false;
            }
            return true;
        }
        public void Update(IWallet senderWallet, string recipient, long amount)
        {
            if (amount <= 0) throw new InvalidOperationException("Invalid transaction amount.");

            if (this.OutputMap[senderWallet.PublicKey] < amount) throw new InvalidOperationException("Transaction amount exceeds balance.");

            if (this.OutputMap.TryGetValue(recipient, out var existingAmt))
            {
                this.OutputMap[recipient] = existingAmt + amount;
            }
            else
            {
                this.OutputMap[recipient] = amount;
            }

            this.OutputMap[senderWallet.PublicKey] -= amount;

            this.Input = this.CreateInput(senderWallet: senderWallet, outputMap: this.OutputMap);
        }


        public static Transaction RewardTransaction(IWallet minerWallet)
        {
            var outputMap = new Dictionary<string, long> { { minerWallet.PublicKey, Constants.MINING_REWARD } };
            return new Transaction(input: CreateRewardInput(outputMap.SerializeObject(), minerWallet), outputMap: outputMap);

        }

        private static Input CreateRewardInput(string outputMapObj, IWallet minerWallet)
        {
            return new Input
            {
                Timestamp = clock.UtcNow,
                Amount = Constants.MINING_REWARD,
                Address = Constants.MINING_REWARD_INPUT,
                Signature = minerWallet.Sign(outputMapObj)
            };
        }

    }
}
