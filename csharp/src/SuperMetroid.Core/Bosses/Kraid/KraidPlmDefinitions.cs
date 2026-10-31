using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Literal hardcoded PLM arguments embedded in Kraid's growth and sinking callbacks.
/// Their irregular order is visible: the ceiling breaks from scattered points as the
/// body rises, rather than sweeping monotonically across the room.
/// </summary>
public static class KraidPlmDefinitions
{
    /// <summary>
    /// The final SpawnHardcodedPLM call in KraidDeath_Initialisation:
    /// start the animated spike sweep at (5,27), independently of defeated-room clearing.
    /// </summary>
    public static readonly PlmSpawnRequest LiveDeathSpikes = new(new(5, 27), PlmHeaderId.CrumbleKraidSpikes);

    /// <summary>SpawnPLMToClearTheCeiling: clear ceiling blocks from (2,18).</summary>
    private static PlmSpawnRequest ClearCeiling => new(new(2, 18), PlmHeaderId.ClearKraidCeiling);

    /// <summary>SpawnPLMToClearTheSpikes: clear spike blocks from (5,27).</summary>
    private static PlmSpawnRequest ClearSpikes => new(new(5, 27), PlmHeaderId.ClearKraidSpikes);

    /// <summary>
    /// The nine calls of Kraid's ceiling crumble, indexed by Kraid variable F / 2: the ceiling callbacks
    /// it selects. Each block lies directly above its paired rock center on row 18; column 2 uses the
    /// left-edge background, then odd and even columns alternate variants 2 and 3.
    /// </summary>
    public static IReadOnlyList<PlmSpawnRequest> GrowthCeiling { get; } = new List<PlmSpawnRequest>
    {
        new(new(6, 18), PlmHeaderId.CrumbleKraidCeilingIntoBackground3),
        new(new(13, 18), PlmHeaderId.CrumbleKraidCeilingIntoBackground2),
        new(new(2, 18), PlmHeaderId.CrumbleKraidCeilingIntoBackground1),
        new(new(10, 18), PlmHeaderId.CrumbleKraidCeilingIntoBackground3),
        new(new(5, 18), PlmHeaderId.CrumbleKraidCeilingIntoBackground2),
        new(new(12, 18), PlmHeaderId.CrumbleKraidCeilingIntoBackground3),
        new(new(3, 18), PlmHeaderId.CrumbleKraidCeilingIntoBackground2),
        new(new(11, 18), PlmHeaderId.CrumbleKraidCeilingIntoBackground2),
        new(new(4, 18), PlmHeaderId.CrumbleKraidCeilingIntoBackground3),
    }.AsReadOnly();

    /// <summary>
    /// The hardcoded PLMs spawned by defeated-room initialization, in call order.
    /// </summary>
    public static IReadOnlyList<PlmSpawnRequest> DefeatedRoom { get; } = [ClearCeiling, ClearSpikes];

    /// <summary>
    /// The platform mutation paired with each sinking-table callback, or none for the no-operation callback. The
    /// surviving platforms share the ceiling row and alternate background blocks by column parity, as in the
    /// sinking callbacks' inline arguments.
    /// </summary>
    public static IReadOnlyDictionary<KraidSinkCallback, PlmSpawnRequest?> SinkMutations { get; } =
        new Dictionary<KraidSinkCallback, PlmSpawnRequest?>
        {
            [KraidSinkCallback.NoOperation] = null,
            [KraidSinkCallback.CrumbleLeftPlatformLeft] = new PlmSpawnRequest(new(7, 18), PlmHeaderId.CrumbleKraidPlatformVariant1),
            [KraidSinkCallback.CrumbleLeftPlatformMiddle] = new PlmSpawnRequest(new(8, 18), PlmHeaderId.CrumbleKraidPlatformVariant2),
            [KraidSinkCallback.CrumbleLeftPlatformRight] = new PlmSpawnRequest(new(9, 18), PlmHeaderId.CrumbleKraidPlatformVariant1),
            [KraidSinkCallback.CrumbleRightPlatformLeft] = new PlmSpawnRequest(new(14, 18), PlmHeaderId.CrumbleKraidPlatformVariant2),
            [KraidSinkCallback.CrumbleRightPlatformMiddle] = new PlmSpawnRequest(new(15, 18), PlmHeaderId.CrumbleKraidPlatformVariant1),
            [KraidSinkCallback.CrumbleRightPlatformRight] = new PlmSpawnRequest(new(16, 18), PlmHeaderId.CrumbleKraidPlatformVariant2),
        };
}
