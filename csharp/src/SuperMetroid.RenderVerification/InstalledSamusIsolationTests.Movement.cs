using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal sealed partial class InstalledSamusIsolationTests
{
    private void CheckMovement()
    {
        foreach (ushort medium in new[] { SamusLiquidPhysicsState.Air,
            SamusLiquidPhysicsState.Water, SamusLiquidPhysicsState.LavaAcid })
        foreach (bool left in new[] { false, true })
        {
            Pair pair = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose,
                medium: medium);
            string label = $"medium {medium}, facing {(left ? "left" : "right")}";
            pair.Apply(label + " enter run", actor =>
            {
                if (left) actor.Samus.ApplyStandingLeftToRunningLeft(actor.Memory);
                else actor.Samus.ApplyStandingRightToRunningRight(actor.Memory);
            });
            ushort initialX = pair.Stock.Samus.XPosition;
            for (ushort frame = 0; frame < 24; frame++)
                pair.Apply(label + $" run {frame}", actor =>
                {
                    actor.Samus.RefreshCollisionRadii(actor.Memory);
                    GroundedMovementResult result = left
                        ? SamusGroundedMovement.StepRunningLeft(actor.Memory, actor.Level, actor.Samus, frame,
                            (ushort)(SnesButton.Left | SnesButton.B))
                        : SamusGroundedMovement.StepRunningRight(actor.Memory, actor.Level, actor.Samus, frame,
                            (ushort)(SnesButton.Right | SnesButton.B));
                    Animate(actor, frame);
                    return result;
                });
            Require(pair.Stock.Samus.XPosition != initialX, label + ": run fixture never moved");
            pair.Apply(label + " turn admission", actor => actor.Samus.ApplyGroundedTurn(actor.Memory,
                left ? SamusPoseId.TurningLeftToRightPose : SamusPoseId.TurningRightToLeftPose));
            int turnFrames = 0;
            while (pair.Stock.Samus.ReadMovementKind(pair.Stock.Memory) == SamusMovementType.TurningOnGround && turnFrames < 120)
            {
                ushort frame = (ushort)turnFrames++;
                pair.Apply(label + $" turn {frame}", actor =>
                {
                    var result = SamusGroundedMovement.StepTurningOnGround(actor.Memory, actor.Level, actor.Samus, frame);
                    Animate(actor, frame);
                    return result;
                });
            }
            Require(pair.Stock.Samus.Pose == (left ? SamusPoseId.FacingRightNormalPose : SamusPoseId.FacingLeftNormalPose),
                label + ": authored turn did not complete");
            CheckJump(medium, left, highJump: false);
            CheckJump(medium, left, highJump: true);
            CheckPosture(medium, left);
            Pair hurt = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose, medium: medium);
            Require(hurt.Apply(label + " hit admission", actor => SamusKnockbackMovement.Start(actor.Memory,
                actor.Samus, 0, left ? (ushort)1 : (ushort)0, level: actor.Level)), label + ": hurt was not admitted");
            for (ushort frame = 0; frame < 8; frame++)
                hurt.Apply(label + $" knockback {frame}", actor =>
                {
                    actor.Samus.RefreshCollisionRadii(actor.Memory);
                    var result = SamusKnockbackMovement.Step(actor.Memory, actor.Level, actor.Samus, frame);
                    Animate(actor, frame);
                    return result;
                });
        }
    }

    private void CheckJump(ushort medium, bool left, bool highJump)
    {
        Pair pair = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose,
            equipment: highJump ? (ushort)SamusEquipmentFlags.HiJumpBoots : (ushort)0, medium: medium);
        string label = $"jump medium {medium}, left={left}, high={highJump}";
        ushort startY = pair.Stock.Samus.YPosition, minimumY = startY;
        pair.Apply(label + " admission", actor => actor.Samus.ApplyOrdinaryJumpTransition(actor.Memory,
            left ? SamusPoseId.NeutralJumpTransitionLeftPose : SamusPoseId.NeutralJumpTransitionRightPose));
        bool landed = false;
        for (ushort frame = 0; frame < 240 && !landed; frame++)
        {
            AerialMovementResult result = pair.Apply(label + $" frame {frame}", actor =>
            {
                actor.Samus.RefreshCollisionRadii(actor.Memory);
                var movement = SamusAerialMovement.StepNormalJump(actor.Memory, actor.Level, actor.Samus,
                    (ushort)SnesButton.A, frame);
                Animate(actor, frame);
                if (movement.Landed) Require(actor.Samus.TryApplyAerialLanding(actor.Memory,
                    actor.Level, wasSpinning: false, controllerInput: (ushort)SnesButton.A,
                    nmiFrameCounter: frame), label + ": landing rejected");
                return movement;
            });
            minimumY = Math.Min(minimumY, pair.Stock.Samus.YPosition);
            landed = result.Landed;
        }
        Require(landed && minimumY < startY, label + ": must rise and collide with the floor, not merely stay equal");
    }

    private void CheckPosture(ushort medium, bool left)
    {
        Pair pair = Create(left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose,
            (ushort)SamusEquipmentFlags.MorphBall, medium);
        string label = $"posture medium {medium}, left={left}";
        Require(pair.Apply(label + " crouch", actor => actor.Samus.TryApplyPostureTransition(actor.Memory,
            actor.Level, left ? SamusPoseId.CrouchingTransitionLeftPose : SamusPoseId.CrouchingTransitionRightPose, 0)),
            label + ": crouch not admitted");
        CompleteAnimation(pair, left ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose, label + " crouching");
        Require(pair.Apply(label + " morph", actor => actor.Samus.TryApplyMorphTransition(actor.Memory,
            actor.Level, left ? SamusPoseId.MorphingTransitionLeftPose : SamusPoseId.MorphingTransitionRightPose, 0)),
            label + ": morph not admitted");
        CompleteAnimation(pair, left ? SamusPoseId.MorphBallGroundLeftPose : SamusPoseId.MorphBallGroundRightPose, label + " morphing");
        Require(pair.Apply(label + " unmorph", actor => actor.Samus.TryApplyMorphTransition(actor.Memory,
            actor.Level, left ? SamusPoseId.UnmorphingTransitionLeftPose : SamusPoseId.UnmorphingTransitionRightPose, 0)),
            label + ": unmorph not admitted");
        CompleteAnimation(pair, left ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose, label + " unmorphing");
    }

    private void CheckPoseCollision()
    {
        foreach (bool blocked in new[] { false, true })
        foreach (bool left in new[] { false, true })
        {
            Pair pair = Create(left ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose,
                lowCeiling: blocked);
            bool accepted = pair.Apply($"expand pose left={left}, blocked={blocked}", actor =>
                actor.Samus.TryApplyDirectCrouchToStandingTransition(actor.Memory, actor.Level,
                    left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose, 0));
            Require(accepted != blocked, "Synthetic ceiling did not exercise the expected eligibility decision");
        }
    }

    private static void Animate(Actor actor, ushort frame)
    {
        actor.Samus.AnimateNoFx(actor.Memory, nmiFrameCounter: frame);
        if (actor.Samus.ApplyPendingVerifiedAnimationTransition(actor.Memory)) actor.Samus.CommitPoseHistory(actor.Memory);
    }

    private static void CompleteAnimation(Pair pair, SamusPoseId target, string context)
    {
        for (ushort frame = 0; frame < 120 && pair.Stock.Samus.Pose != target; frame++)
            pair.Apply(context + $" frame {frame}", actor =>
            {
                actor.Samus.RefreshCollisionRadii(actor.Memory);
                Animate(actor, frame);
            });
        Require(pair.Stock.Samus.Pose == target, context + ": authored animation transition did not finish");
    }
}
