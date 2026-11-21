using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Norfair's Golden Torizo (#2410): it starts in its golden colours, drops its death's pickups over its arena low in its room,
/// walks on from a landing by the choice's walk, and its appearance grants Samus its loadout while the player holds its
/// secret chord.
/// </summary>
internal sealed class GoldenTorizoVariant()
    : TorizoVariant(TorizoInitializationDefinitions.Golden, TorizoWalkerDefinitions.GoldenTorizo, TorizoDeathDefinitions.GoldenTorizoArena,
        EncounterArtwork.GoldenTorizo, TorizoLookKind.GoldenInitial,
        GoldenTorizoCodeDefinitions.Code, TorizoDecisionDefinitions.GoldenFirstAttackMissiles)
{
    /// <summary>Golden Torizo walks on by the choice's walk.</summary>
    /// <param name="alternate">The choice's walk.</param>
    internal override AnimationContinuation<TorizoBodySegment>? WalkOnFromLanding(TorizoAnimation alternate) =>
        TorizoAnimations.ContinueWith(alternate);
}
