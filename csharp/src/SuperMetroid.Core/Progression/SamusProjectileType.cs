
namespace SuperMetroid.Core.Progression;

/// <summary>
/// What a Samus projectile is: its family, the beams it carries, whether it was charged and, for an echo, which echo.
/// Everything that reacts to a projectile (enemies, blocks, doors, bosses) reads this. The default value is
/// <see cref="None"/>: an empty slot, and what the grapple beam presents to the blocks it touches.
/// </summary>
public readonly record struct SamusProjectileType
{
    /// <summary>No projectile: an empty slot, or the grapple beam's touch.</summary>
    public static SamusProjectileType None => default;

    /// <summary>Whether this names a projectile; false for <see cref="None"/>.</summary>
    private bool Exists { get; }

    /// <summary>Creates a projectile identity.</summary>
    private SamusProjectileType(SamusProjectileFamily family, SamusBeamCombination beams, bool charged, SamusBeamEcho? echo)
    {
        Exists = true;
        Family = family;
        BeamCombination = beams;
        IsChargedBeam = charged;
        Echo = echo;
    }

    /// <summary>The projectile's family; <see cref="SamusProjectileFamily.Beam"/> for <see cref="None"/>.</summary>
    public SamusProjectileFamily Family { get; }

    /// <summary>
    /// The beams the projectile carries. A missile or bomb carries none (<see cref="SamusBeamCombination.Power"/>); an
    /// explosion keeps the beams of the shot that made it, and an echo carries the beams its damage is read as.
    /// </summary>
    public SamusBeamCombination BeamCombination { get; }

    /// <summary>Whether the projectile is a charged beam or a special-attack particle fired from a full charge.</summary>
    public bool IsChargedBeam { get; }

    /// <summary>The echo this beam projectile is, or null for a fired shot or special-attack particle.</summary>
    public SamusBeamEcho? Echo { get; }

    /// <summary>Whether this is <see cref="None"/>.</summary>
    public bool IsNone => !Exists;

    /// <summary>A beam shot of <paramref name="beams"/>, charged or not.</summary>
    internal static SamusProjectileType Beam(SamusBeamCombination beams, bool charged) =>
        new(SamusProjectileFamily.Beam, beams, charged, echo: null);

    /// <summary>A projectile of <paramref name="family"/> carrying no beams.</summary>
    internal static SamusProjectileType Of(SamusProjectileFamily family) =>
        new(family, SamusBeamCombination.Power, charged: false, echo: null);

    /// <summary>An echo beam projectile; it carries the beams its damage and reactions are read as.</summary>
    internal static SamusProjectileType EchoOf(SamusBeamEcho echo) => new(
        SamusProjectileFamily.Beam,
        echo switch
        {
            SamusBeamEcho.SpazerComboTrail => SamusBeamCombination.Spazer,
            SamusBeamEcho.ShinesparkEcho => SamusBeamCombination.PlasmaWave,
            _ => throw new ArgumentOutOfRangeException(nameof(echo), echo, "Undefined beam echo."),
        },
        charged: false,
        echo);

    /// <summary>The same projectile after it becomes <paramref name="family"/>, as a shot becomes its explosion: its beams stay.</summary>
    internal SamusProjectileType AsFamily(SamusProjectileFamily family) =>
        new(family, BeamCombination, IsChargedBeam, Echo);

    /// <summary>Whether the projectile is of <paramref name="family"/>.</summary>
    public bool IsFamily(SamusProjectileFamily family) => Family == family;

    /// <summary>Whether the projectile's family comes at or after <paramref name="family"/> in the game's family order.</summary>
    public bool IsFamilyAtOrAfter(SamusProjectileFamily family) => Family.IsAtOrAfter(family);

    /// <summary>Whether the projectile carries any of the beam components in <paramref name="beams"/>.</summary>
    public bool HasAnyBeam(SamusBeamFlags beams) => (BeamCombination.Components & beams) != SamusBeamFlags.None;
}
