using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Poses;

/// <summary>
/// Checks the setups Samus's pose transitions name (#2506): the pose update installs each chosen pose by the setup its
/// transition names, and runs the setup her pose, or else the movement it was stored over, names when her input chooses no
/// new pose.
/// </summary>
internal static class PoseTransitionSetupVerification
{
    /// <summary>The actions the input transitions' chords read.</summary>
    private static readonly GameplayAction[] ChordActions =
    [
        GameplayAction.Up, GameplayAction.Down, GameplayAction.Left, GameplayAction.Right,
        GameplayAction.AimUp, GameplayAction.AimDown, GameplayAction.Shoot, GameplayAction.Jump,
    ];

    /// <summary>
    /// Every transition any pose's input chooses, under every held chord and every part of it newly pressed, names a setup
    /// that installs a pose: a transition keeping the pose never reaches the pose update, and the Draygon-held stand-up's
    /// setup is chosen only from the Draygon-held ball's Up. Every pose then resolves a lookup-failure setup, and a pose
    /// stored over the ground turn or the normal jump keeps theirs.
    /// </summary>
    internal static void VerifyPoseTransitionSetups()
    {
        int transitions = 0;
        foreach (SamusPoseId pose in SamusPoseDefinitions.Poses.Keys)
        {
            for (int heldBits = 1; heldBits < 1 << ChordActions.Length; heldBits++)
            {
                GameplayAction held = Chord(heldBits);
                // Every newly pressed part of the held chord, the empty one included.
                for (int pressedBits = heldBits; ; pressedBits = (pressedBits - 1) & heldBits)
                {
                    if (SamusPoseInputTransitions.Lookup(pose, held, Chord(pressedBits)).Transition is { } transition)
                    {
                        transitions++;
                        bool standUp = pose == SamusPoseId.DraygonHeldBallLeftPose &&
                            transition.ProspectivePose == SamusPoseId.DraygonHeldStandingUpLeftPose;
                        // The messages are built only for a failing transition: the sweep looks up millions of chords.
                        if (!Enum.IsDefined(transition.Setup) || transition.Setup == SamusPoseTransitionSetup.KeepPose ||
                            standUp != (transition.Setup == SamusPoseTransitionSetup.StandUpFromDraygonHold))
                        {
                            AssertTrue(false, $"{pose} -> {transition.ProspectivePose} under {held} (newly {Chord(pressedBits)}) names " +
                                $"{transition.Setup}: a pose-changing transition names a defined setup other than the kept pose's, " +
                                "and the Draygon-held stand-up's only for the Draygon-held ball's Up");
                        }
                    }
                    if (pressedBits == 0)
                        break;
                }
            }

            AssertTrue(Enum.IsDefined(SamusLookupFailureSetups.For(pose, SamusPoseMetadata.ReadMovementType(pose))),
                $"{pose} resolves a defined lookup-failure setup");
        }
        AssertTrue(transitions > 0, "The input transitions choose poses");

        foreach (SamusPoseId drained in SamusPoseDefinitions.Poses.Keys.Where(SamusPoseMetadata.IsDrainedPose))
        {
            AssertEqual(SamusLookupFailureSetup.StopTurn, SamusLookupFailureSetups.For(drained, SamusMovementType.TurningOnGround),
                $"{drained} stored over a ground turn keeps the turn's stop");
            AssertEqual(SamusLookupFailureSetup.ContinueNormalJump, SamusLookupFailureSetups.For(drained, SamusMovementType.NormalJumping),
                $"{drained} stored over a normal jump keeps the jump's fallback");
            AssertEqual(SamusLookupFailureSetup.None, SamusLookupFailureSetups.For(drained, SamusMovementType.TurningWhileFalling),
                $"{drained} stored over a turn in a fall sets nothing up, the aerial turns naming theirs on the pose");
        }
        Console.WriteLine($"Pose transition setups: {transitions} chosen transitions each name a setup that installs their pose.");
    }

    /// <summary>The chord <paramref name="bits"/> selects from <see cref="ChordActions"/>.</summary>
    private static GameplayAction Chord(int bits)
    {
        GameplayAction chord = GameplayAction.None;
        for (int i = 0; i < ChordActions.Length; i++)
        {
            if ((bits & (1 << i)) != 0)
                chord |= ChordActions[i];
        }
        return chord;
    }
}
