namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>Which airborne arc a Beetom is flying, which sets its horizontal speed.</summary>
internal enum BeetomArcKind
{
    /// <summary>A short or long hop, drifting slowly sideways.</summary>
    Hop,

    /// <summary>A lunge at Samus, moving fast sideways.</summary>
    Lunge,
}
