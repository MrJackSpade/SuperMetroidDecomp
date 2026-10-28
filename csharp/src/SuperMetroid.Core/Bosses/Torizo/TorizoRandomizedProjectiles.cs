using static SuperMetroid.Core.Bosses.Torizo.TorizoRandomizedProjectileDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Chooses the Torizos' randomized launch records by the direction they face or move.</summary>
internal static class TorizoRandomizedProjectiles
{
    /// <summary>Bomb Torizo's Chozo orb for its facing.</summary>
    internal static TorizoRandomizedProjectileDefinition BombChozoOrb(bool facingRight) =>
        facingRight ? BombChozoOrbRight : BombChozoOrbLeft;

    /// <summary>Golden Torizo's Chozo orb for its facing.</summary>
    internal static TorizoRandomizedProjectileDefinition GoldenChozoOrb(bool facingRight) =>
        facingRight ? GoldenChozoOrbRight : GoldenChozoOrbLeft;

    /// <summary>Golden Torizo's egg for its horizontal launch direction.</summary>
    internal static TorizoRandomizedProjectileDefinition GoldenEgg(bool movingRight) =>
        movingRight ? GoldenEggRight : GoldenEggLeft;

    /// <summary>Golden Torizo's eye beam for its facing.</summary>
    internal static TorizoRandomizedProjectileDefinition GoldenEyeBeam(bool facingRight) =>
        facingRight ? GoldenEyeBeamRight : GoldenEyeBeamLeft;
}
