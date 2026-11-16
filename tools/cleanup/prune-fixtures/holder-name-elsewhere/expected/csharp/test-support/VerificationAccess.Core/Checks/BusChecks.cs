using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Ending;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Checks;

/// <summary>The bus values and ending text the checks compare.</summary>
internal static class BusChecks
{
    /// <summary>The first reserved register.</summary>
    internal static string Reserved => BusLayoutConstants.ReservedStart;

    /// <summary>The first work memory bank.</summary>
    internal static string WorkBank => BusLayoutConstants2.FirstWorkBank;

    /// <summary>The typed ending and its closing line.</summary>
    internal static string Typed => new EndingText("END").Typewriter() + EndingTextAccess.Closing;
}