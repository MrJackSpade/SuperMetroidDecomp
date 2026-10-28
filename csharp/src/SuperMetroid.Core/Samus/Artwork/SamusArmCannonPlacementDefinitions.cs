
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Body-sprite attachment geometry for the native arm-cannon overlay.</summary>
/// <remarks>
/// Named joins and phase choices are narrowly retained overlay composition. Body
/// coordinates remain owned by the independently editable spritemaps. A missing
/// body source leaves the supplied cover coordinate intact at installation.
/// </remarks>
internal static class SamusArmCannonPlacementDefinitions
{
    /// <summary>$90:C9DF/C9F3: the horizontal cover begins one pixel beyond the closed cannon-tip OBJ origin.</summary>
    internal const int OutwardJoin = 1;
    /// <summary>$90:CA17/CA1B: stationary diagonal-up covers move one pixel inward from the same body tip used by the upward transition.</summary>
    internal const int DiagonalInwardJoin = 1;
    /// <summary>$90:CA20 and CABA/CABC: selected diagonal-down and airborne/crouched upward joins sit one pixel above the body tip.</summary>
    internal const int RaisedJoin = 1;
    /// <summary>$90:CB34/CB3A: falling downward covers sit four pixels above the terminal downward tip.</summary>
    internal const int FallingDownRaise = 4;
    /// <summary>$90:CB2E/CB30: the left falling-up terminal overlay sits three pixels below its body-tip origin.</summary>
    internal const int FallingLeftUpDrop = 3;
    /// <summary>$90:CBA7/CBB5: reversed horizontal moonwalk covers move three pixels inward from the body tip.</summary>
    internal const int MoonwalkInwardJoin = 3;

    /// <summary>$90:CAD5/CAD6: first unused retracted descriptor tail, repeated at CBFF/CC00 and CC0D/CC0E.</summary>
    internal static Offset16<RoomPixels> UnusedFirstTail => new Offset16<RoomPixels>(6, 2);
    /// <summary>$90:CAD7/CAD8: later unused retracted descriptor tail, repeated at CC01..CC04 and CC0F..CC14.</summary>
    internal static Offset16<RoomPixels> UnusedLaterTail => new Offset16<RoomPixels>(-19, -2);
}
