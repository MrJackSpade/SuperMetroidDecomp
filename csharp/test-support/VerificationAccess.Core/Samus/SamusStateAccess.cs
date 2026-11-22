using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Core.Samus;

using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusState"/> members production does not use.</summary>
internal static class SamusStateAccess
{
    /// <summary>Exposes pose identity and focused collision or transition hooks on the live Samus actor.</summary>
    extension(SamusState self)
    {
        /// <summary>Typed view of <see cref="SamusState.Pose"/>; casting preserves undefined cartridge bytes.</summary>
        internal SamusPoseId PoseId
        {
            get => self.Pose;
            set => self.ChangePoseKeepingAnimation(value);
        }

        /// <summary>
        /// Changes her pose keeping her animation (<see cref="SamusState.ChangePoseKeepingAnimation"/>), as a check that builds
        /// her in a pose sets it, and returns her.
        /// </summary>
        internal SamusState InPose(SamusPoseId pose)
        {
            self.ChangePoseKeepingAnimation(pose);
            return self;
        }

        /// <summary>Whether her bomb jump has begun, its movement starting on her next movement update.</summary>
        internal bool BombJumpStarting => self.BombJumpPhase == SamusBombJumpPhase.Starting;

        /// <summary>Whether her bomb jump carries her up.</summary>
        internal bool BombJumpActive => self.BombJumpPhase == SamusBombJumpPhase.Rising;

        /// <summary>Whether her knockback moves her: the knockback movement runs.</summary>
        internal bool KnockbackActive => self.Knockback is SamusKnockback.Moving;

        /// <summary>The direction her knockback holds, moving her or handed back; null with no knockback.</summary>
        internal SamusKnockbackDirection? KnockbackDirection => self.Knockback switch
        {
            SamusKnockback.Moving moving => moving.Direction,
            SamusKnockback.HandedBack handedBack => handedBack.Direction,
            _ => null,
        };

        /// <summary>Enables her Hyper Beam when <paramref name="enabled"/>, as the Baby's gift does, and returns her.</summary>
        internal SamusState WithHyperBeam(bool enabled)
        {
            if (enabled)
                self.EnableHyperBeam();
            return self;
        }

        /// <summary>
        /// Compatibility name for the `$13/$14` Fire subset. Keeping this narrow wrapper makes
        /// existing focused tests readable while all input-table exits share one native handler.
        /// </summary>
        internal void ApplySpinToNormalJumpFireTransition(
            SamusTerrain terrain,
            SamusPoseId targetPose,
            int frameCounter,
            GameplayAction controllerNewInput)
        {
            if (targetPose is not (SamusPoseId.NormalJumpGunExtendedRightPose or SamusPoseId.NormalJumpGunExtendedLeftPose))
            {
                throw new InvalidOperationException(
                    $"Spin-fire compatibility route requires pose $13/$14, not ${(int)SamusPoseNumbers.Number(targetPose):X2}.");
            }
            SamusAerialPoseTransitions.ApplySpinOrWallJumpToNormalJumpTransition(self,
                terrain,
                targetPose,
                frameCounter,
                controllerNewInput);
        }

        /// <summary>
        /// Convenience debugger/test entry that performs both native phases: publishing the
        /// bank-$A0 timer-eight overlap direction, then consuming it through $90:DF99 and
        /// special command three $91:EE80. Live runtime code calls those phases on either
        /// side of movement through <see cref="SamusPoseMetadata.PublishBombJumpDirection(SamusState, SamusBombJumpDirection)"/> and
        /// <see cref="SamusPoseMetadata.TrySetupPublishedBombJump"/>.
        /// </summary>
        internal void RequestMorphedBombJump(SamusBombJumpDirection direction)
        {
            if (!SamusPoseMetadata.IsStableBallPose(self.Pose))
            {
                throw new InvalidOperationException(
                    $"The morphed bomb-jump fixture requires a stable ball pose, not ${(int)SamusPoseNumbers.Number(self.Pose):X2}.");
            }

            SamusPoseMetadata.PublishBombJumpDirection(self, direction);
            SamusPoseMetadata.ArmPublishedBombJump(self);
        }
    }
}
