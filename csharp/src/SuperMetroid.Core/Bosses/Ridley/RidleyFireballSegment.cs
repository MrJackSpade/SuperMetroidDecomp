namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's typed fireball and afterburn animations (#641), in authored order.</summary>
internal enum RidleyFireballSegment
{
    /// <summary>The launching fireball: it stops, shows its two launch poses and starts flying between them.</summary>
    Fireball,
    /// <summary>The flying fireball's four poses, looping (#2235).</summary>
    FireballFlight,
    /// <summary>A directional afterburn's dying puff after it hits a wall: it stops and sparks, then puffs.</summary>
    AfterburnFinal,
    /// <summary>The impact center that emits a horizontal pair: it stops, sparks and spawns the pair, then puffs.</summary>
    HorizontalCenter,
    /// <summary>The impact center that emits a vertical pair: it stops, sparks and spawns the pair, then puffs.</summary>
    VerticalCenter,
    /// <summary>A directional afterburn that extends its chain: it stops, sparks and spawns the next, then puffs.</summary>
    DirectionalAfterburn,
    /// <summary>
    /// The fire cloud every afterburn run puffs after its spark (#2331): it spreads, breaks and dies down to embers, and the
    /// afterburn is deleted.
    /// </summary>
    AfterburnPuff,
}
