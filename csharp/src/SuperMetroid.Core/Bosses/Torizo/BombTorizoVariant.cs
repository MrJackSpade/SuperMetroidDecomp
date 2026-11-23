using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Crateria's Bomb Torizo (#2410): it starts as the dormant statue in its own colours, drops its death's pickups over its
/// arena in the upper room, plays on from a landing past both of the choice's targets, and answers no secret code.
/// </summary>
internal sealed partial class BombTorizoVariant()
    : TorizoVariant(TorizoInitializationDefinitions.Bomb, TorizoWalkerDefinitions.BombTorizo, TorizoDeathDefinitions.BombTorizoArena,
        EncounterArtwork.BombTorizo, TorizoLookKind.BombStatue,
        secretCode: null, TorizoDecisionDefinitions.BombFirstAttackMissiles)
{
    /// <summary>Bomb Torizo plays on past both of the choice's targets.</summary>
    /// <param name="alternate">The choice's walk, which Bomb Torizo does not take.</param>
    internal override AnimationContinuation<TorizoBodySegment>? WalkOnFromLanding(TorizoAnimation alternate) => null;
}
