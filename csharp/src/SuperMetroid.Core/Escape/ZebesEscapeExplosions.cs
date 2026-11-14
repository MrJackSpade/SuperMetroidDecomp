using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Escape;

/// <summary>Chooses a Zebes escape explosion from its random draw and its place (<see cref="ZebesEscapeExplosionDefinitions"/>).</summary>
internal static class ZebesEscapeExplosions
{
    /// <summary>
    /// The explosion the random sample <paramref name="random"/> draws, or null when the draw names none and leaves the
    /// kind to the explosion's place: half the draws name one of <see cref="ZebesEscapeExplosionDefinitions.Drawn"/>.
    /// </summary>
    internal static ZebesEscapeExplosionDefinition? Drawn(int random) =>
        ZebesEscapeDefinitions.ExplosionChoice.Of(random) is var choice && choice < ZebesEscapeExplosionDefinitions.Drawn.Length
            ? ZebesEscapeExplosionDefinitions.Drawn[choice]
            : null;

    /// <summary>The kind an outdoor explosion over <paramref name="block"/> shows when its draw leaves the kind open.</summary>
    internal static RoomSpriteObjectKind OutdoorKindLeftOpen(Position16<RoomBlocks> block) =>
        ZebesEscapeExplosionDefinitions.OutdoorKindsLeftOpenByColumnPhase[
            block.X % ZebesEscapeExplosionDefinitions.OutdoorKindsLeftOpenByColumnPhase.Length];
}
