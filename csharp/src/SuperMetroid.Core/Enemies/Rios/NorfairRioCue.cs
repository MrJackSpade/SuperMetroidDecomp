namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One cue of the Norfair Rio's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="FlamesBelow">
/// For <see cref="NorfairRioCueKind.PlaceFlames"/>, how many pixels below the Norfair Rio its flames go, negative above it;
/// zero for any other cue.
/// </param>
internal readonly record struct NorfairRioCue(NorfairRioCueKind Kind, int FlamesBelow = 0)
{
    /// <summary>Places the flames <paramref name="below"/> pixels below the Norfair Rio, negative above it.</summary>
    internal static NorfairRioCue PlaceFlames(int below) => new(NorfairRioCueKind.PlaceFlames, below);
}
