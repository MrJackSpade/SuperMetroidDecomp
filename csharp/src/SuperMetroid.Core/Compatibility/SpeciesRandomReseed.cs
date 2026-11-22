namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A species' reseed of the shared random numbers (#2374): a point in a species' placement at which it
/// overwrites the shared random state (<see cref="SharedRandom"/>) with its own seed. The species never reads the seed
/// back; every later random consumer, of any species, effect or frontend, draws its values on from the replacement, so a
/// room's random choices after the reseed follow the seed rather than the sequence before it.
/// </summary>
/// <remarks>
/// Applied by <see cref="SharedRandom.Reseed"/> at the moment named by each reseed, during the room
/// placement that reaches it; several placements of one species reseed once each, so the last placed decides the value the
/// first gameplay update draws on from. Evidence: the original's placement routines for the Sidehoppers and Dessgeegas,
/// the Polyp and the Beetom each store their seed into the random value, and the SMV replays compare the random value on
/// every update.
/// </remarks>
[CompatibilityService("a species' placement overwrites the shared random numbers with its own seed, which every later random consumer draws on from")]
public sealed partial class SpeciesRandomReseed
{
    /// <summary>Holds a reseed to <paramref name="seed"/>.</summary>
    private SpeciesRandomReseed(int seed) => Seed = new SharedRandomState(seed);

    /// <summary>The state the reseed leaves the shared random numbers in.</summary>
    public SharedRandomState Seed { get; }

    /// <summary>
    /// Every Sidehopper's and Dessgeega's placement reseeds with 37, then draws one value it never reads, before it sets
    /// up its hop; the draw is the placement's own and leaves the state one advance past 37.
    /// </summary>
    public static SpeciesRandomReseed HopperPlacement { get; } = new(37);

    /// <summary>
    /// Every Polyp's placement reseeds with 17 after it starts its stationary pose. In Norfair's volcano room the room's
    /// last-placed Polyp leaves this value for the first gameplay update, which decides the rocks' launch sequence.
    /// </summary>
    public static SpeciesRandomReseed PolypPlacement { get; } = new(17);

    /// <summary>
    /// Every Beetom's placement reseeds with 23, after it records the controller and before it picks its facing, even
    /// when several Beetoms share a room.
    /// </summary>
    public static SpeciesRandomReseed BeetomPlacement { get; } = new(23);
}
