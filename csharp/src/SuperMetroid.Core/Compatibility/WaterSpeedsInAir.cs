namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Samus held by Draygon, or in special movement (a shinespark, Crystal Flash, X-ray, her death and the other scripted
/// poses), moves in air by the speeds water gives standing and running (#1734). The game gives these two movements their
/// own speeds in water and in lava or acid, but none in air: held by Draygon in air she speeds up and slows as she
/// would standing in water, held to no speed, and in special movement in air she speeds up, is held and slows as she
/// would running in water. Everywhere else a movement's speeds are its medium's own.
/// </summary>
/// <remarks>
/// A rule with no state beyond the speeds it hands out, created with each Samus (<c>SamusCompatibility</c>) from her
/// speed catalog and given to her horizontal speeds. Consulted during the update's movement pass, whenever a movement
/// step calculates her base horizontal speed for her movement while she moves by air's speeds. Evidence: the
/// original selects a movement's speeds by stepping from the start of the medium's speed list by the movement's number,
/// with no bound, and the air list ends after the damage boost, so Draygon's hold and special movement read the first
/// two entries of the water list stored after it; the indexed speeds check (<c>VerifySamusIndexedSpeeds</c>) compares
/// every medium's and movement's speeds with the cartridge entry that selection reads.
/// </remarks>
/// <typeparam name="TMovement">A movement of Samus's.</typeparam>
/// <typeparam name="TSpeeds">A movement's horizontal speeds.</typeparam>
[CompatibilityService("Draygon's hold and special movement in air move by the speeds water gives standing and running")]
public sealed class WaterSpeedsInAir<TMovement, TSpeeds>
    where TMovement : notnull
    where TSpeeds : notnull
{
    /// <summary>The water speeds each movement without speeds of its own in air takes there.</summary>
    private readonly IReadOnlyDictionary<TMovement, TSpeeds> _speeds;

    /// <summary>Keeps the water speeds each movement without speeds of its own in air takes there.</summary>
    /// <param name="speeds">The speeds each such movement takes in air.</param>
    /// <exception cref="ArgumentException">No movement was given.</exception>
    public WaterSpeedsInAir(IReadOnlyDictionary<TMovement, TSpeeds> speeds)
    {
        Ensure.NotNull(speeds);
        if (speeds.Count == 0)
            throw new ArgumentException("At least one movement takes water's speeds in air.", nameof(speeds));
        _speeds = speeds;
    }

    /// <summary>
    /// The water speeds <paramref name="movement"/> takes in air; false for a movement with speeds of its own in air.
    /// </summary>
    /// <param name="movement">Samus's movement.</param>
    /// <param name="speeds">The speeds the movement takes in air, when it has none of its own there.</param>
    public bool TryRead(TMovement movement, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TSpeeds speeds) =>
        _speeds.TryGetValue(movement, out speeds);
}
