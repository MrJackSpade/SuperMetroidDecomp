namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's shared neck math. Both the live room-enemy actor
/// and the later rainbow-beam sequence use these exact angle steps; keeping them here
/// prevents the two encounter halves from slowly acquiring different articulation rules.
/// </summary>
internal static class MotherBrainNeckKinematics
{
    /// <summary>Lowest angle the lower neck bobs down to before reversing upward.</summary>
    private static readonly RotationAngle LowerBobFloor = RotationAngle.FromSteps(40);

    /// <summary>Highest angle the lower neck reaches, both bobbing up and raising one way.</summary>
    private static readonly RotationAngle LowerCeiling = RotationAngle.FromSteps(144);

    /// <summary>Angle at which a one-way lowering of the lower neck stops.</summary>
    private static readonly RotationAngle LowerFloor = RotationAngle.FromSteps(48);

    /// <summary>Lowest angle the upper neck reaches, both bobbing down and lowering one way.</summary>
    private static readonly RotationAngle UpperFloor = RotationAngle.FromSteps(32);

    /// <summary>How far the upper neck's raised limit leads the lower neck's angle: a thirty-second of a turn.</summary>
    private static readonly RotationAngle UpperLeadOverLower = RotationAngle.FromSteps(8);

    /// <summary>
    /// Advances the lower and upper neck angles through bobbing or one-way raise/lower movement, each turning by
    /// <paramref name="turn"/> an update.
    /// </summary>
    /// <remarks>The upper step runs second and observes the lower angle and movement state changed in the same frame.</remarks>
    internal static void StepAngles(
        ref RotationAngle lowerAngle,
        ref RotationAngle upperAngle,
        ref MotherBrainNeckMovement lowerMovement,
        ref MotherBrainNeckMovement upperMovement,
        RotationAngle turn,
        int brainY,
        int samusY)
    {
        if (lowerMovement == MotherBrainNeckMovement.BobDown)
        {
            // Bob down to the bob's floor, then reverse.
            RotationAngle candidate = lowerAngle - turn;
            if (candidate.CompareTo(LowerBobFloor) < 0)
            {
                candidate = LowerBobFloor;
                lowerMovement = MotherBrainNeckMovement.BobUp;
            }
            lowerAngle = candidate;
        }
        else if (lowerMovement == MotherBrainNeckMovement.BobUp)
        {
            // Bob up to the lower neck's ceiling unless the brain is already above its highest bob.
            if (brainY < MotherBrainNeckJointDefinitions.BobUpHighestBrainY)
            {
                lowerMovement = MotherBrainNeckMovement.BobDown;
            }
            else
            {
                RotationAngle candidate = lowerAngle + turn;
                if (candidate.CompareTo(LowerCeiling) >= 0)
                {
                    candidate = LowerCeiling;
                    lowerMovement = MotherBrainNeckMovement.BobDown;
                }
                lowerAngle = candidate;
            }
        }
        else if (lowerMovement == MotherBrainNeckMovement.Lower)
        {
            // One-way lower used by beam setup and the resurrection stretch.
            RotationAngle candidate = lowerAngle - turn;
            if (candidate.CompareTo(LowerFloor) < 0)
            {
                candidate = LowerFloor;
                lowerMovement = MotherBrainNeckMovement.None;
            }
            lowerAngle = candidate;
        }
        else if (lowerMovement == MotherBrainNeckMovement.Raise)
        {
            // One-way raise used by fake-death ascent and Baby interruptions.
            RotationAngle candidate = lowerAngle + turn;
            if (candidate.CompareTo(LowerCeiling) >= 0)
            {
                candidate = LowerCeiling;
                lowerMovement = MotherBrainNeckMovement.None;
            }
            lowerAngle = candidate;
        }

        // Runs the upper step after the lower step, so it observes the
        // lower angle and movement-index mutations made immediately above on this frame.
        if (upperMovement == MotherBrainNeckMovement.BobDown)
        {
            // Bob down; Samus below the brain forces both halves back upward.
            if (brainY + MotherBrainNeckJointDefinitions.BobDownSamusBelowBrain >= samusY)
            {
                lowerMovement = MotherBrainNeckMovement.BobUp;
                upperMovement = MotherBrainNeckMovement.BobUp;
            }
            else
            {
                RotationAngle candidate = upperAngle - turn;
                if (candidate.CompareTo(UpperFloor) < 0)
                {
                    candidate = UpperFloor;
                    upperMovement = MotherBrainNeckMovement.BobUp;
                }
                upperAngle = candidate;
            }
        }
        else if (upperMovement == MotherBrainNeckMovement.BobUp)
        {
            // Follow the upper neck's lead above the newly updated lower segment.
            RotationAngle target = lowerAngle + UpperLeadOverLower;
            RotationAngle candidate = upperAngle + turn;
            if (candidate.CompareTo(target) >= 0)
            {
                candidate = target;
                upperMovement = MotherBrainNeckMovement.BobDown;
            }
            upperAngle = candidate;
        }
        else if (upperMovement == MotherBrainNeckMovement.Lower)
        {
            // One-way lower to the upper neck's floor.
            RotationAngle candidate = upperAngle - turn;
            if (candidate.CompareTo(UpperFloor) < 0)
            {
                candidate = UpperFloor;
                upperMovement = MotherBrainNeckMovement.None;
            }
            upperAngle = candidate;
        }
        else if (upperMovement == MotherBrainNeckMovement.Raise)
        {
            // One-way raise to the lower angle plus the upper neck's lead.
            RotationAngle target = lowerAngle + UpperLeadOverLower;
            RotationAngle candidate = upperAngle + turn;
            if (candidate.CompareTo(target) >= 0)
            {
                candidate = target;
                upperMovement = MotherBrainNeckMovement.None;
            }
            upperAngle = candidate;
        }
    }

    /// <summary>
    /// Places every joint of <paramref name="neck"/>: each lower joint at its distance from the lower neck's root on
    /// <paramref name="lowerAngle"/>, then each upper joint at its distance from the last lower joint on
    /// <paramref name="upperAngle"/>.
    /// </summary>
    /// <param name="neck">The neck whose joints are placed.</param>
    /// <param name="body">The body's position.</param>
    /// <param name="lowerAngle">The angle the lower joints hang at.</param>
    /// <param name="upperAngle">The angle the upper joints hang at.</param>
    internal static void PlaceJoints(MotherBrainNeck neck, Position16<RoomPixels> body, RotationAngle lowerAngle, RotationAngle upperAngle)
    {
        Position16<RoomPixels> root = body + MotherBrainNeckJointDefinitions.LowerNeckRoot;
        TrigonometryAngle lower = lowerAngle.ToTrigonometryAngle();
        TrigonometryAngle upper = upperAngle.ToTrigonometryAngle();
        foreach (MotherBrainNeckJoint joint in neck.Lower)
            joint.Position = root + CalculatePolarOffset(lower, joint.Distance);
        Position16<RoomPixels> upperRoot = neck.UpperAnchor.Position;
        foreach (MotherBrainNeckJoint joint in neck.Upper)
            joint.Position = upperRoot + CalculatePolarOffset(upper, joint.Distance);
    }

    /// <summary>
    /// One joint's displacement from its anchor: its distance along the angle's sine across (X) and along the sine of the
    /// angle a quarter turn on down (Y).
    /// </summary>
    private static Offset16<RoomPixels> CalculatePolarOffset(TrigonometryAngle angle, int distance) =>
        new(CalculateSignedComponent(angle, distance),
            CalculateSignedComponent(angle.TurnedBy(TrigonometryAngle.QuarterTurn), distance));

    /// <summary>
    /// One axis of a joint's displacement: its distance times the angle's sine, in whole pixels rounded down.
    /// </summary>
    private static int CalculateSignedComponent(TrigonometryAngle angle, int distance) =>
        UnitComponent.SineOf(angle).ReachRoundedDownOf(distance);
}
