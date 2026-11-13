namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's typed fireball and afterburn programs (#641), in the authored order of their lists.</summary>
internal enum RidleyFireballSegment
{
    /// <summary>The launching fireball.</summary>
    Fireball,
    /// <summary>The flying fireball's loop.</summary>
    FireballLoop,
    /// <summary>A directional afterburn's dying puff after it hits a wall.</summary>
    AfterburnFinal,
    /// <summary>The impact center that emits a horizontal pair.</summary>
    HorizontalCenter,
    /// <summary>The impact center that emits a vertical pair.</summary>
    VerticalCenter,
    /// <summary>A directional afterburn that extends its chain.</summary>
    DirectionalAfterburn,
}
