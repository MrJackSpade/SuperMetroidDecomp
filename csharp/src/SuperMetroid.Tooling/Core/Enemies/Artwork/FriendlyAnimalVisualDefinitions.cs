
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Ordinary OAM compositions the friendly animals' and their escape variants' typed programs show. Their route, jumps, echoes and escape timing remain mechanics.
/// </summary>
internal static class FriendlyAnimalVisualDefinitions
{
    /// <summary>Etecoon/Dachora ordinary animation and sprite bank $A7.</summary>
    internal const byte NormalBank = 0xa7;
    /// <summary>The rescue-sequence animals' ordinary animation and sprite bank $B3.</summary>
    internal const byte EscapeBank = 0xb3;
    /// <summary>Thirty-one selected Etecoon compositions at $A7:EEED..F20A.</summary>
    internal const int EtecoonFrameCount = 31;
    /// <summary>Twenty-nine selected Dachora body/echo compositions at $A7:F9C4..FF53.</summary>
    internal const int DachoraFrameCount = 29;
    /// <summary>
    /// Four rescue Etecoon compositions of their own at $B3:E8B0..E91F; their strides at $B3:E736..E77C store the Etecoon's
    /// again (#2327).
    /// </summary>
    internal const int EscapeEtecoonFrameCount = 4;
    /// <summary>
    /// No rescue Dachora composition of its own: its strides at $B3:EB1B..ED3E store the Dachora's again (#2327).
    /// </summary>
    internal const int EscapeDachoraFrameCount = 0;
}
