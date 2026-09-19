using System;
using MelonLoader;

// Standalone MelonLoader 0.7.x entry point: the separate LoaderProbe confirmed that
// OnInitializeMelon is invoked on the user's setup. This does NOT establish whether
// the shared Harmony patches can attach to this game's methods yet.
[assembly: MelonInfo(typeof(AffinityProtectionStandalone.StandaloneMod), "Partner Affinity Protection (Experimental Standalone)", "0.2.0-test", "MakRatanakKh")]

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
