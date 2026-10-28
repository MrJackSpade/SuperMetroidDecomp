using SuperMetroid.Core.Samus.Movement;
using GrabPolicy = SuperMetroid.Core.Bosses.Ridley.RidleySamusInteractionDefinitions.GrabPolicy;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Whether Ridley may grab Samus in her current movement, and how long she stays intangible after release.</summary>
internal static class RidleyGrabs
{
    /// <summary>$A6:BCFC transfers movement bit7 to carry, permitting Ridley's grab when set.</summary>
    internal static bool CanGrab(SamusMovementType movement) => Policy(movement) != GrabPolicy.Immune;

    /// <summary>$A6:BC98 loads6 frames for morphed overflow; $A6:BC9D otherwise loads10.</summary>
    internal static ushort ReleaseIntangibilityFrames(SamusMovementType movement) =>
        Policy(movement) == GrabPolicy.Morphed
            ? RidleySamusInteractionDefinitions.MorphedReleaseIntangibilityFrames
            : RidleySamusInteractionDefinitions.OrdinaryReleaseIntangibilityFrames;

    /// <summary>The policy of <paramref name="movement"/>; no pose selects any other value, so one fails loudly.</summary>
    private static GrabPolicy Policy(SamusMovementType movement) =>
        RidleySamusInteractionDefinitions.GrabPolicies.TryGetValue(movement, out GrabPolicy policy)
            ? policy
            : throw new InvalidOperationException($"Undefined SamusMovementType {movement}.");
}
