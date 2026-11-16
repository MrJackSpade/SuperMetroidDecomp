using static SuperMetroid.Core.Bosses.Torizo.TorizoRandomizedProjectileDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Chooses the Torizos' randomized launches by the direction they face or move.</summary>
internal static class TorizoRandomizedProjectiles
{
    /// <summary>Bomb Torizo's Chozo orb for its facing.</summary>
    internal static TorizoRandomizedProjectileDefinition BombChozoOrb(HorizontalDirection facing) =>
        facing == HorizontalDirection.Right ? BombChozoOrbRight : BombChozoOrbLeft;

    /// <summary>Golden Torizo's Chozo orb for its facing.</summary>
    internal static TorizoRandomizedProjectileDefinition GoldenChozoOrb(HorizontalDirection facing) =>
        facing == HorizontalDirection.Right ? GoldenChozoOrbRight : GoldenChozoOrbLeft;

    /// <summary>Golden Torizo's egg for its horizontal launch direction.</summary>
    internal static TorizoRandomizedProjectileDefinition GoldenEgg(HorizontalDirection launch) =>
        launch == HorizontalDirection.Right ? GoldenEggRight : GoldenEggLeft;
}
