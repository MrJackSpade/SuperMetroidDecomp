using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Samus;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;

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
            set => self.Pose = value;
        }

        /// <summary>
        /// Compatibility name for the `$13/$14` Fire subset. Keeping this narrow wrapper makes
        /// existing focused tests readable while all input-table exits share one native handler.
        /// </summary>
        internal bool TryApplySpinToNormalJumpFireTransition(
            RoomLevelData level,
            SamusPoseId targetPose,
            int frameCounter,
            ControllerButton controllerNewInput,
            RoomPlmSystem? plms = null)
        {
            if (targetPose is not (SamusPoseId.NormalJumpGunExtendedRightPose or SamusPoseId.NormalJumpGunExtendedLeftPose))
            {
                throw new InvalidOperationException(
                    $"Spin-fire compatibility route requires pose $13/$14, not ${(int)targetPose:X2}.");
            }
            return SamusAerialPoseTransitions.TryApplySpinOrWallJumpToNormalJumpTransition(self, 

                level,
                targetPose,
                frameCounter,
                controllerNewInput,
                plms);
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
                    $"The morphed bomb-jump fixture requires a stable ball pose, not ${(int)self.Pose:X2}.");
            }

            SamusPoseMetadata.PublishBombJumpDirection(self, direction);
            SamusPoseMetadata.ArmPublishedBombJump(self);
        }
    }
}
