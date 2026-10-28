using static SuperMetroid.Core.Enemies.Corpses.DeathExplosionScatterDefinitions;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Picks a point of the small-explosion scatter shared by Ridley's and the Baby Metroid's deaths.</summary>
internal static class DeathExplosionScatter
{
    /// <summary>Selects an authored scatter point and rejects indices outside the ten-point list.</summary>
    internal static Offset16<RoomPixels> Offset(int index) => (uint)index < Count
        ? Offsets[index]
        : throw new ArgumentOutOfRangeException(nameof(index));
}
