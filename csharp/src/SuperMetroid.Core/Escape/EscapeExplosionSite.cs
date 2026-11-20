using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// Where an escape room's random explosions go off (#2287): a behaviour object (#2485) holding the cadence they go off at,
/// whether they go off only over scenery, the kinds an explosion whose random draw leaves its kind open shows by its
/// block's column, and what the room's quake does beside them. Outdoors they go off at the outdoor cadence and only over
/// scenery, the room shaking on with them; indoors at the slower indoor cadence anywhere on the screen. It replaces the outdoors
/// test the escape rooms' mains made.
/// </summary>
public abstract class EscapeExplosionSite
{
    /// <summary>Keeps the site's cadence and its kinds left open.</summary>
    /// <param name="cadence">The updates on which the site sets off an explosion.</param>
    /// <param name="kindsLeftOpenByColumnPhase">
    /// The kinds an explosion whose random draw leaves its kind open shows, by its block's column in turn.
    /// </param>
    private protected EscapeExplosionSite(UpdateCadence cadence, IReadOnlyList<RoomEffectActorKind> kindsLeftOpenByColumnPhase)
    {
        Cadence = cadence;
        _kindsLeftOpenByColumnPhase = kindsLeftOpenByColumnPhase;
    }

    /// <summary>The kinds an explosion whose random draw leaves its kind open shows, by its block's column in turn.</summary>
    private readonly IReadOnlyList<RoomEffectActorKind> _kindsLeftOpenByColumnPhase;

    /// <summary>The updates of the frame clock's counter on which the site sets off an explosion.</summary>
    internal UpdateCadence Cadence { get; }

    /// <summary>Whether an explosion goes off only over a block that shows scenery.</summary>
    private protected abstract bool OnlyOverScenery { get; }

    /// <summary>Whether an explosion placed over <paramref name="block"/> of <paramref name="level"/> goes off.</summary>
    internal bool GoesOffOver(RoomLevelData level, Position16<RoomBlocks> block) =>
        !OnlyOverScenery || !Ensure.NotNull(level).GetObjectCollisionBlock(block).Terrain.Appearance.ShowsNothing;

    /// <summary>The kind an explosion over <paramref name="block"/> shows when its random draw leaves the kind open.</summary>
    internal RoomEffectActorKind KindLeftOpen(Position16<RoomBlocks> block) =>
        _kindsLeftOpenByColumnPhase[block.X % _kindsLeftOpenByColumnPhase.Count];

    /// <summary>What the room's quake does on every update the site's room main runs, after the explosions.</summary>
    /// <param name="earthquake">The room's earthquake.</param>
    internal virtual void AfterExplosions(RoomEarthquake earthquake) => Ensure.NotNull(earthquake);

    /// <summary>Under Zebes's sky: an explosion at the outdoor cadence over scenery, the room's quake kept shaking.</summary>
    public static EscapeExplosionSite Outdoors { get; } = new OutdoorSite();

    /// <summary>Inside: an explosion at the indoor cadence anywhere on the screen.</summary>
    public static EscapeExplosionSite Indoors { get; } = new IndoorSite();

    /// <summary>
    /// The outdoor site: an explosion goes off only over scenery, and one whose random draw leaves its kind open takes the
    /// kind its block's column phase gives it; the room's quake shakes on beside them.
    /// </summary>
    private sealed class OutdoorSite() : EscapeExplosionSite(ZebesEscapeDefinitions.OutdoorExplosion,
        ZebesEscapeExplosionDefinitions.OutdoorKindsLeftOpenByColumnPhase)
    {
        /// <inheritdoc/>
        private protected override bool OnlyOverScenery => true;

        /// <inheritdoc/>
        internal override void AfterExplosions(RoomEarthquake earthquake) => Ensure.NotNull(earthquake).KeepShaking();
    }

    /// <summary>The indoor site: an explosion goes off anywhere, and one whose random draw leaves its kind open is a small explosion.</summary>
    private sealed class IndoorSite() : EscapeExplosionSite(ZebesEscapeDefinitions.IndoorExplosion,
        [ZebesEscapeExplosionDefinitions.IndoorKindLeftOpen])
    {
        /// <inheritdoc/>
        private protected override bool OnlyOverScenery => false;
    }
}
