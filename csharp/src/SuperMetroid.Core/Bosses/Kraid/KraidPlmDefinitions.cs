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
    public static readonly PlmSpawnRequest<KraidRoomMutation> LiveDeathSpikes = new(new(5, 27), KraidRoomMutation.CrumbleSpikes);

    /// <summary>SpawnPLMToClearTheCeiling: clear ceiling blocks from (2,18).</summary>
    private static PlmSpawnRequest<KraidRoomMutation> ClearCeiling => new(new(2, 18), KraidRoomMutation.ClearCeiling);

    /// <summary>SpawnPLMToClearTheSpikes: clear spike blocks from (5,27).</summary>
    private static PlmSpawnRequest<KraidRoomMutation> ClearSpikes => new(new(5, 27), KraidRoomMutation.ClearSpikes);

    /// <summary>
    /// The nine calls of Kraid's ceiling crumble, indexed by Kraid variable F / 2: the ceiling callbacks
    /// it selects. Each block lies directly above its paired rock center on row 18; column 2 uses the
    /// left-edge background, then odd and even columns alternate variants 2 and 3.
    /// </summary>
    public static IReadOnlyList<PlmSpawnRequest<KraidRoomMutation>> GrowthCeiling { get; } = new List<PlmSpawnRequest<KraidRoomMutation>>
    {
        new(new(6, 18), KraidRoomMutation.CrumbleCeilingIntoBackground3),
        new(new(13, 18), KraidRoomMutation.CrumbleCeilingIntoBackground2),
        new(new(2, 18), KraidRoomMutation.CrumbleCeilingIntoBackground1),
        new(new(10, 18), KraidRoomMutation.CrumbleCeilingIntoBackground3),
        new(new(5, 18), KraidRoomMutation.CrumbleCeilingIntoBackground2),
        new(new(12, 18), KraidRoomMutation.CrumbleCeilingIntoBackground3),
        new(new(3, 18), KraidRoomMutation.CrumbleCeilingIntoBackground2),
        new(new(11, 18), KraidRoomMutation.CrumbleCeilingIntoBackground2),
        new(new(4, 18), KraidRoomMutation.CrumbleCeilingIntoBackground3),
    }.AsReadOnly();

    /// <summary>
    /// The hardcoded PLMs spawned by defeated-room initialization, in call order.
    /// </summary>
    public static IReadOnlyList<PlmSpawnRequest<KraidRoomMutation>> DefeatedRoom { get; } = [ClearCeiling, ClearSpikes];

    /// <summary>
    /// The platform mutation paired with each sinking-table callback, or none for the no-operation callback. The
    /// surviving platforms share the ceiling row and alternate background blocks by column parity, as in the
    /// sinking callbacks' inline arguments.
    /// </summary>
    public static IReadOnlyDictionary<KraidSinkCallback, PlmSpawnRequest<KraidRoomMutation>?> SinkMutations { get; } =
        new Dictionary<KraidSinkCallback, PlmSpawnRequest<KraidRoomMutation>?>
        {
            [KraidSinkCallback.NoOperation] = null,
            [KraidSinkCallback.CrumbleLeftPlatformLeft] = new PlmSpawnRequest<KraidRoomMutation>(new(7, 18), KraidRoomMutation.CrumblePlatformVariant1),
            [KraidSinkCallback.CrumbleLeftPlatformMiddle] = new PlmSpawnRequest<KraidRoomMutation>(new(8, 18), KraidRoomMutation.CrumblePlatformVariant2),
            [KraidSinkCallback.CrumbleLeftPlatformRight] = new PlmSpawnRequest<KraidRoomMutation>(new(9, 18), KraidRoomMutation.CrumblePlatformVariant1),
            [KraidSinkCallback.CrumbleRightPlatformLeft] = new PlmSpawnRequest<KraidRoomMutation>(new(14, 18), KraidRoomMutation.CrumblePlatformVariant2),
            [KraidSinkCallback.CrumbleRightPlatformMiddle] = new PlmSpawnRequest<KraidRoomMutation>(new(15, 18), KraidRoomMutation.CrumblePlatformVariant1),
            [KraidSinkCallback.CrumbleRightPlatformRight] = new PlmSpawnRequest<KraidRoomMutation>(new(16, 18), KraidRoomMutation.CrumblePlatformVariant2),
        };
}
