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

        private static readonly string _defaultMagic = "majikGuzlayar";

        /// <summary>
        ///main method is runnable only by Smartcontract owner.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool Main(string operation, BigInteger[] args, byte[] signature, byte[] owner)
        {
            if (operation.Equals("goy"))    //goy means 'put' in turkmenian langauge
            {
                if (Runtime.CheckWitness(_gameOwner))
                {
                    Storage.Put(Storage.CurrentContext, _key, signature);   //signature comes as a magic word
                    return true;
                }
            } else if (operation.Equals("isValidLog"))
            {
                return IsValidLog(signature, args);
            } else if (operation.Equals("isValidHeroData"))
            {
                return IsValidHeroCreation(signature, owner, args);
            }
            return false;
        }

        /// <summary>
        ///puts magic key into the smartcontract.
        ///
        ///accepts 20 integers
        /// </summary>
        /// <returns></returns>
        public static bool IsValidLog(byte[] incomingSignature, BigInteger[] args)
        {
            //prepare combination of log arguments
            byte[] argsCombination = args[0].AsByteArray();
            argsCombination = argsCombination.Concat(args[1].AsByteArray());
            argsCombination = argsCombination.Concat(args[2].AsByteArray());
            argsCombination = argsCombination.Concat(args[3].AsByteArray());
            argsCombination = argsCombination.Concat(args[4].AsByteArray());
            argsCombination = argsCombination.Concat(args[5].AsByteArray());

            argsCombination = argsCombination.Concat(args[6].AsByteArray());
            argsCombination = argsCombination.Concat(args[7].AsByteArray());
            argsCombination = argsCombination.Concat(args[8].AsByteArray());
            argsCombination = argsCombination.Concat(args[9].AsByteArray());
            argsCombination = argsCombination.Concat(args[10].AsByteArray());

            argsCombination = argsCombination.Concat(args[11].AsByteArray());
            argsCombination = argsCombination.Concat(args[12].AsByteArray());
            argsCombination = argsCombination.Concat(args[13].AsByteArray());
            argsCombination = argsCombination.Concat(args[14].AsByteArray());
            argsCombination = argsCombination.Concat(args[15].AsByteArray());
            argsCombination = argsCombination.Concat(args[16].AsByteArray());
            argsCombination = argsCombination.Concat(args[17].AsByteArray());
            argsCombination = argsCombination.Concat(args[18].AsByteArray());
            argsCombination = argsCombination.Concat(args[19].AsByteArray());

            return _VerifySignature(argsCombination, incomingSignature);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="incomingSignature"></param>
        /// <param name="owner"></param>
        /// <param name="args">12 arguments</param>
        /// <returns></returns>
        public static bool IsValidHeroCreation(byte[] incomingSignature, byte[] owner, BigInteger[] args)
        {

            byte[] argsCombination = owner;
            Runtime.Notify("Length1", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[0].AsByteArray());    // Hero ID
            Runtime.Notify("Length2", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[1].AsByteArray());    // Troops Cap
            Runtime.Notify("Length3", argsCombination.Length);

            argsCombination = argsCombination.Concat(args[2].AsByteArray());    // Stat 1
            Runtime.Notify("Length4", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[3].AsByteArray());    // Stat 2
            Runtime.Notify("Length5", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[4].AsByteArray());    // Stat 3
            Runtime.Notify("Length6", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[5].AsByteArray());    // Stat 4
            Runtime.Notify("Length7", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[6].AsByteArray());    // Stat 5
            Runtime.Notify("Length8", argsCombination.Length);

            argsCombination = argsCombination.Concat(args[7].AsByteArray());    // Item 1
            Runtime.Notify("Length9", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[8].AsByteArray());    // Item 2
            Runtime.Notify("Length10", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[9].AsByteArray());    // Item 3
            Runtime.Notify("Length11", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[10].AsByteArray());    // Item 4
            Runtime.Notify("Length12", argsCombination.Length);
            argsCombination = argsCombination.Concat(args[11].AsByteArray());    // Item 5
            Runtime.Notify("Length13", argsCombination.Length);

            Runtime.Notify("Owner", owner, "ID", args[0], "Cap", args[1]);
            Runtime.Notify("Stats", args[2], args[3], args[4], args[5], args[6]);

            Runtime.Notify("Items", args[7], args[8], args[9], args[10], args[11]);

            return _VerifySignature(argsCombination, incomingSignature);
        }

        private static bool _VerifySignature(byte[] argsCombination, byte[] incomingSignature)
        {
            byte[] magic = Storage.Get(Storage.CurrentContext, _key);
            if (magic.Length == 0)
            {
                magic = _defaultMagic.AsByteArray();
                Runtime.Notify("Use default magic word");
            }else
            {
                Runtime.Notify("Magic word is set");
            }

            Runtime.Notify("Magic word is", magic);

            byte[] signature = argsCombination.Concat(magic);
            signature = Hash256(signature);

            Runtime.Notify("Generated at contract", signature, "Incoming", incomingSignature);

            return signature == incomingSignature;
        }
    }
}
