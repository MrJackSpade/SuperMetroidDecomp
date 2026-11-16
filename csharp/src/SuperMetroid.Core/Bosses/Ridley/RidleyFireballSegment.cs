namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's typed fireball and afterburn animations (#641), in authored order.</summary>
internal enum RidleyFireballSegment
{
    /// <summary>The launching fireball: it stops, shows its two launch poses and starts flying between them.</summary>
    Fireball,
    /// <summary>The flying fireball's four poses, looping (#2235).</summary>
    FireballFlight,
    /// <summary>A directional afterburn's dying puff after it hits a wall.</summary>
    AfterburnFinal,
    /// <summary>The impact center that emits a horizontal pair.</summary>
    HorizontalCenter,
    /// <summary>The impact center that emits a vertical pair.</summary>
    VerticalCenter,
    /// <summary>A directional afterburn that extends its chain.</summary>
    DirectionalAfterburn,
}
