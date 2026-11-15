using SuperMetroid.Core.Samus.Movement;
using GrabPolicy = SuperMetroid.Core.Bosses.Ridley.RidleySamusInteractionDefinitions.GrabPolicy;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Whether Ridley may grab Samus in her current movement, and how long she stays intangible after release.</summary>
internal static class RidleyGrabs
{
    /// <summary>Whether Ridley may grab Samus: every movement but those that make her immune to his grab.</summary>
    internal static bool CanGrab(SamusMovementType movement) => Policy(movement) != GrabPolicy.Immune;

    /// <summary>How long Samus stays out of Ridley's reach after he releases her: six frames in ball form, otherwise ten.</summary>
    internal static int ReleaseIntangibilityFrames(SamusMovementType movement) =>
        Policy(movement) == GrabPolicy.Morphed
            ? RidleySamusInteractionDefinitions.MorphedReleaseIntangibilityFrames
            : RidleySamusInteractionDefinitions.OrdinaryReleaseIntangibilityFrames;

    /// <summary>The policy of <paramref name="movement"/>; no pose selects any other value, so one fails loudly.</summary>
    private static GrabPolicy Policy(SamusMovementType movement) =>
        RidleySamusInteractionDefinitions.GrabPolicies.TryGetValue(movement, out GrabPolicy policy)
            ? policy
            : throw new InvalidOperationException($"Undefined SamusMovementType {movement}.");
}
