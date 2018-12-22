using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace VerifierContract
{
    /// <summary>
    /// @author Medet Ahmetson at Troll Games
    /// @summary Private Contract that verifies signature's of given items
    /// </summary>
    public class Contract1 : SmartContract
    {
        /// <summary>
        ///latest key to get magic key
        /// </summary>
        private static readonly string _key = "asdakwefldmqwmsa";

        /// <summary>
        ///smartcontract Owner's Wallet Address.
        /// </summary>
        private static readonly byte[] _gameOwner = "AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();

        /// <summary>
        ///main method is runnable only by Smartcontract owner.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static byte[] Main(string operation, object[] args)
        {
            if (Runtime.CheckWitness(_gameOwner))
            {
                if (operation.Equals("goy"))    //goy means 'put' in turkmenian langauge
                    Storage.Put(Storage.CurrentContext, _key, (string)args[0]);
            }
            return new BigInteger(1).ToByteArray();
        }

        /// <summary>
        ///puts magic key into the smartcontract.
        /// </summary>
        /// <returns></returns>
        public static bool IsValidLog(byte[] incomingSignature, object[] args)
        {
            //prepare combination of log arguments
            byte[] argsCombination = new byte[] { 0x01 };

            return _VerifySignature(argsCombination, incomingSignature);
        }

        public static bool IsValidHeroCreation(byte[] incomingSignature, object[] args)
        {
            byte[] argsCombination = new byte[] { 0x01 };

            return _VerifySignature(argsCombination, incomingSignature);
        }

        private static bool _VerifySignature(byte[] bytes, byte[] incomingSignature)
        {
            byte[] magic = Storage.Get(Storage.CurrentContext, _key);

            byte[] signature = bytes.Concat(magic);
            signature = Hash256(signature);

            return signature == incomingSignature;
        }
    }
}
