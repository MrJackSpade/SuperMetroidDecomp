namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The bounded nine-block vertical wall-clear draw at $84:930F.</summary>
internal static class BotwoonWallPlmDrawDefinitions
{
    /// <summary>The nine cleared wall blocks of an already-defeated room; the crumble frames are shot-block breakup draws.</summary>
    internal static PlmFrameDraw<BotwoonWallDraw> Clear { get; } = PlmDraw.Of(BotwoonWallDraw.Clear);

    /// <summary>Number of air blocks written by the single vertical clear-wall run.</summary>
    internal const int BlockCount = 9;
}
