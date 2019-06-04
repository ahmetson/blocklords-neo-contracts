using Neo.SmartContract.Framework.Services.Neo;

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
        public static void Set(string key, byte[] value)
        {
            if (key.Equals(GeneralContract.FEE_HERO_CREATION))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_HERO_CREATION, value);
            }
            else if (key.Equals(GeneralContract.FEE_REFERAL))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_REFERAL, value);
            }
            else if (key.Equals(GeneralContract.FEE_8_HOURS))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_8_HOURS, value);
            }
            else if (key.Equals(GeneralContract.FEE_12_HOURS))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_12_HOURS, value);
            }
            else if (key.Equals(GeneralContract.FEE_24_HOURS))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_24_HOURS, value);
            }
            else if (key.Equals(GeneralContract.FEE_PVC))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_PVC, value);
            }
            else if (key.Equals(GeneralContract.FEE_PVE))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_PVE, value);
            }
            else if (key.Equals(GeneralContract.FEE_PVP))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.FEE_PVP, value);
            }
            else if (key.Equals(GeneralContract.PERCENTS_GAME_OWNER))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.PERCENTS_GAME_OWNER, value);
            }
            else if (key.Equals(GeneralContract.PERCENTS_LORD))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.PERCENTS_LORD, value);
            }
            else if (key.Equals(GeneralContract.MARKET_COFFER_ADDITION_8_HOURS))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.MARKET_COFFER_ADDITION_8_HOURS, value);
            }
            else if (key.Equals(GeneralContract.MARKET_COFFER_ADDITION_12_HOURS))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.MARKET_COFFER_ADDITION_12_HOURS, value);
            }
            else if (key.Equals(GeneralContract.MARKET_COFFER_ADDITION_24_HOURS))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.MARKET_COFFER_ADDITION_24_HOURS, value);
            }
            else if (key.Equals(GeneralContract.PVC_COFFER_ADDITION_AMOUNT))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.PVC_COFFER_ADDITION_AMOUNT, value);
            }
            else if (key.Equals(GeneralContract.PERCENTS_COFFER_PAY))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.PERCENTS_COFFER_PAY, value);
            }

            else if (key.Equals(GeneralContract.INTERVAL_COFFER))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.INTERVAL_COFFER, value);
            }
            else if (key.Equals(GeneralContract.INTERVAL_DROP))
            {
                Storage.Put(Storage.CurrentContext, GeneralContract.INTERVAL_DROP, value);
            }
        }

    }
}


