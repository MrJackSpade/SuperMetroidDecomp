namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>What a cue of the Norfair Rio's animations does: place the flames, or tell main AI the swoop's start has finished.</summary>
internal enum NorfairRioCueKind
{
    /// <summary>Places the flames the cue's <see cref="NorfairRioCue.FlamesBelow"/> pixels below the Norfair Rio.</summary>
    PlaceFlames,
    /// <summary>Tells main AI the swoop's start animation has finished.</summary>
    SwoopStartFinished,
}
