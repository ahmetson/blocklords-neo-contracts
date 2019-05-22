using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace LordsContract
{
    /// <summary>
    /// This class deals with SmartContract settings.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Set on Blockchain setting
        /// </summary>
        /// <param name="key">setting type</param>
        /// <param name="value">setting value</param>
        /// <returns></returns>
        public static byte[] Set(string key, BigInteger value)
        {
            if (key.Equals(GeneralContract.FEE_HERO_CREATION))
            {
                Storage.Put( Storage.CurrentContext, GeneralContract.FEE_HERO_CREATION, value);
            }
            else if (key.Equals(GeneralContract.FEE_REFERAL))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_REFERAL, v);
            }
            else if (key.Equals(GeneralContract.FEE_8_HOURS))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_8_HOURS, v);
            }
            else if (key.Equals(GeneralContract.FEE_12_HOURS))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_12_HOURS, v);
            }
            else if (key.Equals(GeneralContract.FEE_24_HOURS))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_24_HOURS, v);
            }
            else if (key.Equals(GeneralContract.FEE_PVC))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_PVC, v);
            }
            else if (key.Equals(GeneralContract.FEE_PVE))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_PVE, v);
            }
            else if (key.Equals(GeneralContract.FEE_PVP))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_PVP, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_PURCHACE))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.PERCENTS_PURCHACE, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_LORD))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.PERCENTS_LORD, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_SELLER_COFFER))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.PERCENTS_SELLER_COFFER, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_PVC_COFFER))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.PERCENTS_PVC_COFFER, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_COFFER_PAY))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.PERCENTS_COFFER_PAY, v);
            }

            else if (key.Equals(GeneralContract.INTERVAL_COFFER))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.INTERVAL_COFFER, v);
            }
            else if (key.Equals(GeneralContract.INTERVAL_STRONGHOLD_REWARD))
            {
                byte[] v = value.AsByteArray();
                Storage.Put(Storage.CurrentContext, GeneralContract.INTERVAL_STRONGHOLD_REWARD, v);
            }
            else
            {
                return new BigInteger(0).AsByteArray();
            }

            return new BigInteger(1).AsByteArray();
        }

    }
}


