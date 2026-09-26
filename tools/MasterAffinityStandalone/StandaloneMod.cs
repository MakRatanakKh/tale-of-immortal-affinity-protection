using System;
using MelonLoader;

[assembly: MelonInfo(
    typeof(MasterAffinityProtectionStandalone.StandaloneMod),
    "Master Affinity Protection",
    "0.1.0-test",
    "MakRatanakKh")]

namespace MasterAffinityProtectionStandalone
{
    public sealed class StandaloneMod : MelonMod
    {
        private readonly MasterAffinityProtection.MasterAffinityProtectionCore implementation =
            new MasterAffinityProtection.MasterAffinityProtectionCore();

        public override void OnInitializeMelon()
        {
            try
            {
                MelonLogger.Msg("MasterAffinityProtection: OnInitializeMelon ENTERED; initializing master affinity patches.");
                implementation.Init();
            }
            catch (Exception ex)
            {
                MelonLogger.Error("MasterAffinityProtection: initialization FAILED: " + ex);
            }
        }

        public override void OnDeinitializeMelon()
        {
            try
            {
                implementation.Destroy();
                MelonLogger.Msg("MasterAffinityProtection: deinitialized.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("MasterAffinityProtection: deinitialization FAILED: " + ex);
            }
        }
    }
}