namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Where both Ridleys breathe their fireballs from and how far the afterburn of one spreads (#2232).</summary>
internal static class RidleyFireballDefinitions
{
    /// <summary>His mouth, where a fireball leaves and is aimed from, while he faces left; facing right mirrors it.</summary>
    internal static readonly Offset16<RoomPixels> MuzzleFacingLeft = new(-25, -43);

    /// <summary>The afterburn stages a fireball breathed with its afterburn spawns on impact.</summary>
    internal const int AfterburnStages = 3;
}
