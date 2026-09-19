using System;
using System.IO;
using MelonLoader;

// This is a SEPARATE diagnostic MelonMod, not the game's built-in Local Mod.
// It neither touches the affinity patch nor modifies any game state.
[assembly: MelonInfo(typeof(AffinityLoaderProbe.Probe), "Affinity Loader Probe", "0.0.1", "MakRatanakKh")]

namespace AffinityLoaderProbe
{
    public sealed class Probe : MelonMod
    {
        public override void OnInitializeMelon()
        {
            const string marker = "AffinityLoaderProbe: OnInitializeMelon reached";
            MelonLogger.Msg(marker);
            try
            {
                File.AppendAllText(
                    Path.Combine(Path.GetTempPath(), "AffinityLoaderProbe-diagnostic.log"),
                    DateTime.Now.ToString("O") + " " + marker + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // MelonLoader's own log remains the primary evidence if TEMP is unavailable.
                MelonLogger.Error("AffinityLoaderProbe: could not write TEMP marker: " + ex);
            }
        }
    }
}
