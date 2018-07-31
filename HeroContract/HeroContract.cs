using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System.Numerics;

/**
 *  HeroContract deals with the Heroes of the Blocklords Game on the Neo Blockchain.
 *  Heroes can be put or not.
 *  
 *  Version: 1.0
 *  Author: Medet Ahmetson
 *  Date: 20 Jul. 2017
 *    
 *  Hero ID is the Key at the Storage, and the Hero Parameters is a value at the Storage
 *  
 *  Hero Parameters are:
 *  Leadership Stat (4) Strength Stat (4) Speed Stat (4) Intelligence Stat (4) Defense Stat (4) Hero Nation (1) Hero Class (1) Optional Data (1)
 *  
 *  20+3 = Total length of Hero Parameters
 *  
 *  
 *  POSSIBLE COMMANDS OVER THE HEROES ON THE BLOCKCHAIN:
 *  
 *  Create First Hero - 
 *  Allocates the memory for the User at the Storage.
 *  Puts the first Hero at the Storage
 *  
 *  Create Hero (Requires the Transaction fee) -
 *  Puts the Hero parameters at the Storage
 *   
 */
namespace Blocklords
{
    public class HeroContract : Neo.SmartContract.Framework.SmartContract
    {
        /*private static readonly int leadershipIndex     = 0;
        private static readonly int strengthIndex       = 4;
        private static readonly int speedIndex          = 8;
        private static readonly int intelligenceIndex   = 12;
        private static readonly int defenceStatIndex    = 16;
        private static readonly int nationIndex         = 20;
        private static readonly int classIndex          = 21;
        private static readonly int addressIndex        = 23;*/
        private static readonly int optionalDataIndex   = 22;

        /*private static readonly int statLength          = 4;
        private static readonly int nationLength        = 1;
        private static readonly int classLength         = 1;*/
        private static readonly int optionalDataLength = 1;/*
        private static readonly int heroParametersLength = 23;
        private static readonly int addressLength       = 20;*/

        // Hero Class is located at the of Hero Parameters
        //private static readonly int parametersLength= HeroContract.classIndex + HeroContract.classLength;
        private static readonly int idLength        = 13;

        private static readonly decimal fee = 0.01m;      // 0.01 GAS
        
        private static byte[] GetFalseByte(string message)
        {
            Runtime.Log(message);
            return new BigInteger(0).AsByteArray();
        }
        private static byte[] GetTrueByte(string message)
        {
            Runtime.Log("success!" + message);
            return new BigInteger(1).AsByteArray();
        }

        public static object Main(string operation, object[] args)
        {
            Runtime.Log("version:0.1.9");

            bool checkWitness = false;
            if (operation.Equals("putFirstHero") )
            {
                checkWitness = true;
                
            }
            if (operation.Equals("putHero"))
            {
                checkWitness = true;
            }
            checkWitness = false;
            if (checkWitness)
            {
                if (!Runtime.CheckWitness((byte[])args[0]))
                {
                    return GetFalseByte("auth_fail");
                }
            }

            if (operation.Equals("getStorage")) return Storage.CurrentContext;
            Runtime.Log("auth_success");
            // @Param Owner Address, Hero ID, Hero Params
            if (operation.Equals("putFirstHero")) return PutFirst((byte[])args[0], (string)args[1], (string)args[2]);

            // @Param Hero ID
            if (operation.Equals("putHero")) return Put((byte[])args[0], (string)args[1], (string)args[2]);

            if (operation.Equals("get")) return Get((string)args[0]);

            return GetFalseByte("invalid_operation");
        }

        // OPERATIONS
        private static byte[] Get(string heroId)
        {
            byte[] data = Storage.Get(Storage.CurrentContext, heroId);
            Runtime.Log("Data from Storage: <"+data.AsString()+">");
            return data;
        }
        private static byte[] PutFirst(byte[] address, string heroId, string heroParameters)
        {
            Runtime.Log("put_first_beginning");
            // Validate input
            if (!IsValidHeroId(heroId))
            {
                return GetFalseByte("invalid_hero_id");
            }
            if (!IsValidHeroParameters(heroParameters))
            {
                return GetFalseByte("invalid_hero_parameters");
            }
            Runtime.Log("put_first_validated");

            //StorageMap player = Storage.CurrentContext.CreateMap(address); // 'Player' Prefix, holds all Heroes of Player
            //player.Put(heroId, heroParameters);

            return PutHero(address, heroId, heroParameters);
        }
        private static byte[] Put(byte[] address, string heroId, string heroParameters)
        {
            Runtime.Log("put_beginning");
            // Validate input
            if (!IsValidHeroId(heroId))
            {
                Runtime.Log("Invalid Hero ID!");
                return GetFalseByte("invalid_hero_id");
            }
            if (!IsValidHeroParameters(heroParameters))
            {
                Runtime.Log("Invalid Hero Parameters!");
                return GetFalseByte("invalid_hero_parameters");
            }
            if (!IsTransactionFeeIncluded())
            {
                return GetFalseByte("no_fee_included");
            }
            Runtime.Log("put_validated");

            //StorageMap player = Storage.CurrentContext.CreateMap(address); // 'Player' Prefix, holds all Heroes of Player
            //player.Put(heroId, heroParameters);
            return PutHero(address, heroId, heroParameters);
        }

        // VALIDATORS
        private static bool IsValidHeroId(string heroId)
        {
            return heroId.Length.Equals(HeroContract.idLength);
        }
        private static bool IsValidHeroParameters(string heroParameters)
        {
            int parametersLength = HeroContract.optionalDataIndex + HeroContract.optionalDataLength;
            int length = heroParameters.AsByteArray().Length;
            // Hero Class is located at the of Hero Parameters
            return length.Equals(parametersLength);
        }
        private static bool IsTransactionFeeIncluded()
        {
            TransactionOutput[] outputs = ((Transaction)ExecutionEngine.ScriptContainer).GetOutputs();
            foreach (TransactionOutput output in outputs)
            {
                if (output.ScriptHash.Equals(ExecutionEngine.EntryScriptHash))
                {
                    long value = output.Value;
                    if (value.Equals(HeroContract.fee))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Helpers
        private static byte[] PutHero(byte[] address, string heroId, string heroParameters)
        {
            string value = heroParameters + (address.AsString());
            Storage.Put(Storage.CurrentContext, heroId, value);

            return GetTrueByte("hero_put");
        }
    }
}
