namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>What a cue of the Norfair Rio's animations does: place the flames, or tell main AI the swoop's start has finished.</summary>
internal enum NorfairRioCueKind
{
    /// <summary>Places the flames level with the Norfair Rio.</summary>
    FlamesAt0,
    /// <summary>Places the flames 12 pixels below the Norfair Rio.</summary>
    FlamesAt12,
    /// <summary>Places the flames 4 pixels below the Norfair Rio.</summary>
    FlamesAt4,
    /// <summary>Places the flames 8 pixels below the Norfair Rio.</summary>
    FlamesAt8,
    /// <summary>Places the flames 12 pixels above the Norfair Rio.</summary>
    FlamesAtMinus12,
    /// <summary>Places the flames 16 pixels above the Norfair Rio.</summary>
    FlamesAtMinus16,
    /// <summary>Places the flames 4 pixels above the Norfair Rio.</summary>
    FlamesAtMinus4,
    /// <summary>Tells main AI the swoop's start animation has finished.</summary>
    SwoopStartFinished,
}
