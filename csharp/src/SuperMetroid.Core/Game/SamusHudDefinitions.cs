using static SuperMetroid.Core.Game.SamusHudRomData;

namespace SuperMetroid.Core.Game;

/// <summary>Compiled weapon-admission policy; HUD artwork and labels are separate assets.</summary>
internal static class SamusHudDefinitions
{
    /// <summary>$90:DD05 Samus_HandleHudSpecificBehaviorAndProjs.pointers: all 28 movement-handler identities.</summary>
    private static ReadOnlySpan<ushort> MovementHandlers =>
    [
        StandardHandler, StandardHandler, StandardHandler, JumpHandler,
        MorphBallHandler, StandardHandler, StandardHandler, MorphBallHandler,
        MorphBallHandler, MorphBallHandler, JumpHandler, GrappleHandler,
        GrappleHandler, JumpHandler, TurningHandler, TransitionHandler,
        StandardHandler, MorphBallHandler, MorphBallHandler, MorphBallHandler,
        JumpHandler, StandardHandler, GrappleHandler, TurningHandler,
        TurningHandler, JumpHandler, DraygonHeldHandler, JumpHandler,
    ];

    /// <summary>$90:DDAA HUDSelectionHandler_TransitionPoses.flags: twelve authored pose-$35..$40 cancellation flags.</summary>
    private static ReadOnlySpan<byte> PostureFlags => [0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1];

    internal static ushort MovementHandler(SamusMovementType movement) => MovementHandlers[(byte)movement];

    internal static bool TryGetPostureFlag(byte pose, out byte flag)
    {
        int index = pose - FirstTransitionPose;
        if ((uint)index >= PostureFlags.Length)
        {
            flag = default;
            return false;
        }
        flag = PostureFlags[index];
        return true;
    }
}
