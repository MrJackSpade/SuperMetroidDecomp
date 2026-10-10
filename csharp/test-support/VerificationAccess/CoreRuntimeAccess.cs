using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Verification access to <see cref="SuperMetroidRuntime"/> members production does not use.</summary>
internal static class SuperMetroidRuntimeAccess
{
    /// <summary>Provides verification-only setup and inspection operations for runtime state that production callers do not need.</summary>
    extension(SuperMetroidRuntime self)
    {

        /// <summary>
        /// Creates pose-$01 Samus on a supported Landing Site floor beneath world X=$0440 and
        /// repositions the room camera so both she and the selected terrain are visible.
        /// </summary>
        /// <remarks>
        /// This is an explicitly host-selected debugger scenario, not a claim that the landing
        /// cinematic door owns a gameplay spawn. The selected X and desired screen Y are host
        /// policy. The default minimum row $4D skips a valid but visually transparent collision
        /// slope and selects the first lower supported surface, a visibly rendered solid floor.
        /// Floor type/BTS, height, pose radius, resting world Y, camera clamps, movement,
        /// collision, animation, graphics, and OAM all come from translated cartridge data.
        /// Call only after the room camera and level data are initialized. Refill the visible
        /// tilemaps afterward so VRAM corresponds to the newly selected camera position.
        /// </remarks>
        internal DebugGroundedSamusPlacement InitializeDebugGroundedSamus(
            ushort xPosition = 0x0440,
            ushort desiredScreenY = 166,
            int minimumFloorBlockY = 0x4d)
        {
            if (self.Camera is null || self.LevelData is null)
            {
                throw new InvalidOperationException(
                    "Landing Site camera and level data must be initialized before grounding debug Samus.");
            }

            // Radius is pose data, so construct/refresh pose $01 before deriving the surface
            // center. The temporary Y=0 is never rendered or stepped.
            self.InitializeDebugSamus(xPosition, yPosition: 0);
            ushort yRadius = self.Samus!.Kinematics.YRadius;

            if ((uint)minimumFloorBlockY >= (uint)self.LevelData.HeightInBlocks)
                throw new ArgumentOutOfRangeException(nameof(minimumFloorBlockY));

            int blockX = xPosition >> 4;
            for (int blockY = minimumFloorBlockY; blockY < self.LevelData.HeightInBlocks; blockY++)
            {
                RoomCollisionBlock floor = self.LevelData.GetCollisionBlock(blockX, blockY);

                byte height;
                if (floor.CollisionType == RoomCollisionType.SolidBlock)
                {
                    // Solid block type $8 uses the block's top edge as its floor. A height of
                    // zero expresses that edge in the same block-local coordinate system used
                    // by slope profiles below.
                    height = 0;
                }
                else if (floor.CollisionType == RoomCollisionType.Slope &&
                         floor.Bts.IsNonSquareSlope &&
                         !floor.Bts.SlopeFlipsVertically)
                {
                    // Upright non-square slopes use the cartridge's sixteen-sample profile.
                    // Square slopes are translated too, but selecting one as a spawn surface
                    // would require choosing the occupied quadrant rather than one scalar Y.
                    height = SamusSlopePhysics.ReadAlignmentHeight(
                        PrivateState.Field<ISnesAddressSpace>(self, "_addressSpace"),
                        floor.Bts,
                        xPosition);
                }
                else
                {
                    continue;
                }

                ushort restingY = unchecked((ushort)(blockY * 16 + height - yRadius));

                // Normal right-facing distance slot zero targets layer1X = SamusX-$60. Seed the
                // camera at that exact target so the first moved frame follows smoothly instead
                // of spending several frames correcting the cutscene door's unrelated X=$400.
                // Vertical framing remains the explicit host stimulus supplied by the caller.
                self.Camera.SetPosition(xPosition - 0x60, restingY - desiredScreenY);
                self.Samus.YPosition = restingY;
                PrivateState.SetProperty(self, "GroundedSamusMovementEnabled", true);
                PrivateState.SetProperty(self, "LastGroundedSamusMovement", null);
                PrivateState.SetProperty(self, "LastAerialSamusMovement", null);
                PrivateState.SetProperty(self, "LastMorphBallMovement", null);
                PrivateState.SetProperty(self, "LastBombJumpMovement", null);
                PrivateState.SetProperty(self, "LastKnockbackMovement", null);
                self.BombProjectiles.Reset();
                self.Projectiles.Reset(self.Samus);
                return new DebugGroundedSamusPlacement(
                    xPosition,
                    restingY,
                    desiredScreenY,
                    blockX,
                    blockY,
                    floor,
                    height);
            }

            throw new InvalidOperationException(
                $"No supported solid or upright non-square floor exists beneath " +
                $"Landing Site X=${xPosition:X4} from block row ${minimumFloorBlockY:X2}.");
        }

        /// <summary>Loads the bound beam artwork's grapple-firing palette and flare into CGRAM through the runtime's normal loader.</summary>
        internal void LoadDebugGrapplePalette() => PrivateState.Invoke(self, "LoadGrapplePalette");

        /// <summary>Initializes the ROM-authored standing pose at an explicitly supplied point.</summary>
        internal void InitializeDebugSamus(ushort xPosition, ushort yPosition)
        {
            PrivateState.SetProperty(self, "Samus", new SamusState
            {
                Pose = SamusPoseIds.FacingRightNormalPose,
                AnimationFrame = 0,
                XPosition = xPosition,
                YPosition = yPosition,
            });
            PrivateState.Invoke(self, "BindSamusPalettePresentation");

            SamusState.LoadPowerSuitPalette(PrivateState.Field<ISnesAddressSpace>(self, "_addressSpace"), self.Cgram, PrivateState.Field<AreaMapPresentationCatalog?>(self, "mapPresentation")?.SamusSuitColors);
            self.Samus!.RefreshCollisionRadii(PrivateState.Field<ISnesAddressSpace>(self, "_addressSpace"));
            self.Samus.InitializeAnimation(PrivateState.Field<ISnesAddressSpace>(self, "_addressSpace"));
            if (self.LandingSiteEntry is not null)
            {
                // FootstepGraphics dispatches on the literal area and room-index bytes. Keep
                // those room-owned inputs beside the FX surface state instead of hard-coding
                // “Landing Site” behavior into the generic atmospheric renderer.
                self.Samus.LiquidPhysics.RoomIdentity = self.LandingSiteEntry.RoomIdentity;
            }
            PrivateState.SetProperty(self, "PreviousMovementTypeForXray", self.Samus.ReadMovementType(PrivateState.Field<ISnesAddressSpace>(self, "_addressSpace")));

            // StepFrame begins with NMI, so prime the definitions now. Otherwise frame one's
            // OAM would name Samus tiles before any corresponding graphics reached VRAM.
            self.Samus.PrimeGraphics(PrivateState.Field<ISnesAddressSpace>(self, "_addressSpace"));
        }

        /// <summary>
        /// Queues the two number/label tile transfers in the Ceres table at
        /// <c>$A6:C4CB-$A6:C4D8</c>. The following entries are typewriter BG text and are not
        /// needed by the OAM escape-timer renderer.
        /// </summary>
        internal void QueueEscapeTimerSpriteTiles()
        {
            if (self.MapPresentation is null)
            {
                self.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.FirstByteCount,
                    EscapeTimerTileRomData.FirstSourceAddress,
                    EscapeTimerTileAtlasFormat.FirstDestinationWord);
                self.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.SecondByteCount,
                    EscapeTimerTileRomData.SecondSourceAddress,
                    EscapeTimerTileAtlasFormat.SecondDestinationWord);
            }
            else
                self.MapPresentation.EscapeTimerTiles.QueueTo(self.VramWrites);
        }
    }
}
