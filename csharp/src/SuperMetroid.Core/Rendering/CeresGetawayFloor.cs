namespace SuperMetroid.Core.Rendering;

/// <summary>The BG2 floor shown in the last sixteen scanlines of Ceres Ridley's getaway: its scroll and character base (#633).</summary>
public readonly record struct CeresGetawayFloor
{
    /// <summary>Bg2 scroll.</summary>
    public required Position16<Bg2Pixels> Bg2Scroll { get; init; }

    /// <summary>Bg2 character base word.</summary>
    public required ushort Bg2CharacterBaseWord { get; init; }
}
