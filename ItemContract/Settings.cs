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
        public static byte[] Set(string key, object value)
        {
            if (key.Equals(GeneralContract.FEE_HERO_CREATION))
            {
                byte[] b = GeneralContract.FEE_HERO_CREATION.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.FEE_REFERAL))
            {
                byte[] b = GeneralContract.FEE_REFERAL.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.FEE_8_HOURS))
            {
                byte[] b = GeneralContract.FEE_8_HOURS.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.FEE_12_HOURS))
            {
                byte[] b = GeneralContract.FEE_12_HOURS.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.FEE_24_HOURS))
            {
                byte[] b = GeneralContract.FEE_24_HOURS.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.FEE_PVC))
            {
                byte[] b = GeneralContract.FEE_PVC.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.FEE_PVE))
            {
                byte[] b = GeneralContract.FEE_PVE.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.FEE_PVP))
            {
                byte[] b = GeneralContract.FEE_PVP.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_PURCHACE))
            {
                byte[] b = GeneralContract.PERCENTS_PURCHACE.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_LORD))
            {byte[] b = GeneralContract.PERCENTS_LORD.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_SELLER_COFFER))
            {
                byte[] b = GeneralContract.PERCENTS_SELLER_COFFER.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_PVC_COFFER))
            {
                byte[] b = GeneralContract.PERCENTS_PVC_COFFER.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.PERCENTS_COFFER_PAY))
            {
                byte[] b = GeneralContract.PERCENTS_COFFER_PAY.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }

            else if (key.Equals(GeneralContract.INTERVAL_COFFER))
            {
                byte[] b = GeneralContract.INTERVAL_COFFER.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else if (key.Equals(GeneralContract.INTERVAL_STRONGHOLD_REWARD))
            {
                byte[] b = GeneralContract.INTERVAL_STRONGHOLD_REWARD.AsByteArray();
                byte[] v = value.Serialize();
                Storage.Put(Storage.CurrentContext, b, v);
            }
            else
            {
                Runtime.Log("Failed to set setting");
                return new BigInteger(0).AsByteArray();
            }

            return new BigInteger(1).AsByteArray();
        }

    }
}


