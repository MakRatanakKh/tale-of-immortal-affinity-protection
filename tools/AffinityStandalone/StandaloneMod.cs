using System;
using MelonLoader;

// Standalone MelonLoader 0.7.x entry point. OnInitializeMelon was verified on
// the user's game, but this cap-aware revision still needs compilation and testing.
[assembly: MelonInfo(typeof(AffinityProtectionStandalone.StandaloneMod), "Partner Affinity Protection (Experimental Standalone)", "0.2.4-test", "MakRatanakKh")]

namespace AffinityProtectionStandalone
{
    public sealed class StandaloneMod : MelonMod
    {
        private readonly MOD_Rk7Qp2.ModMain implementation = new MOD_Rk7Qp2.ModMain();

        public override void OnInitializeMelon()
        {
            try
            {
                MelonLogger.Msg("AffinityProtectionStandalone: OnInitializeMelon ENTERED; initializing shared affinity patches.");
                implementation.Init();
            }
            catch (Exception ex)
            {
                MelonLogger.Error("AffinityProtectionStandalone: initialization FAILED: " + ex);
            }
        }

        public override void OnDeinitializeMelon()
        {
            try
            {
                implementation.Destroy();
                MelonLogger.Msg("AffinityProtectionStandalone: deinitialized.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("AffinityProtectionStandalone: deinitialization FAILED: " + ex);
            }
        }
    }
}
