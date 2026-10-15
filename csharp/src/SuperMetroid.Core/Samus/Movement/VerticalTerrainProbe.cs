namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// One vertical terrain scan: what <see cref="SamusBlockCollision"/>'s column probe reads from a
/// vertical move. Every member is explicit; the scan has no default side-effect policy (#633).
/// </summary>
internal readonly record struct VerticalTerrainProbe
{
    /// <summary>Signed vertical displacement to probe.</summary>
    public required int Displacement { get; init; }

    /// <summary>Whether the probed columns are scanned left to right.</summary>
    public required bool ScanLeftToRight { get; init; }

    /// <summary>Whether bomb blocks along the probe break.</summary>
    public required bool CanBreakBombBlocks { get; init; }

    /// <summary>Whether touched door blocks publish their side effects.</summary>
    public required bool PublishDoorSideEffects { get; init; }

    /// <summary>The direction reported to block reactions, or none.</summary>
    public required SamusCollisionDirection? BlockReactionDirection { get; init; }
}
