using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Ridley data extraction reads (#1413): the import pipeline's private knowledge of the
/// cartridge, decoded once into the installed colour documents.
/// </summary>
[CpuBusBoundary("catalog: Ridley cartridge sources")]
internal static class RidleySourceDefinitions
{
    /// <summary>Ceres Ridley's bank-$A6 colour sources.</summary>
    internal static class Ceres
    {
        /// <summary>$A6:E16F-$E1AE: the door, Baby container and initial Baby colours Ceres Ridley's initialization loads.</summary>
        internal const int StartColors = 0xa6e16f;

        /// <summary>$A6:E1F1-$E268: the Baby's four fifteen-colour rows.</summary>
        internal const int BabyColors = 0xa6e1f1;

        /// <summary>$A6:E30A-$E469: the sixteen eleven-colour body-fade rows.</summary>
        internal const int BodyFadeColors = 0xa6e30a;

        /// <summary>$A6:E46A: the three fourteen-colour damage shades shared by Ceres and Norfair Ridley.</summary>
        internal const int HealthColors = 0xa6e46a;

        /// <summary>$A6:C1DF: the sixteen three-colour self-destruct alarm shades.</summary>
        internal const int AlarmColors = 0xa6c1df;

        /// <summary>$A6:A9E3: the retreat background's colours one to fifteen.</summary>
        internal const int RetreatBgColors = 0xa6a9e3;

        /// <summary>$A6:AA01: the eight retreat colours copied to both the background and the sprite palette.</summary>
        internal const int RetreatSharedColors = 0xa6aa01;

        /// <summary>$A6:B107: the nine getaway zoom shades.</summary>
        internal const int GetawayZoomColors = 0xa6b107;

        /// <summary>The bytes between consecutive getaway zoom shades: sixteen stored colours, of which the last is not copied.</summary>
        internal const int GetawayZoomRowBytes = 32;
    }
}
