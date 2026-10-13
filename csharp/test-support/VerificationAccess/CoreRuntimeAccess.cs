using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Spatial;

/// <summary>Verification access to <see cref="SuperMetroidRuntime"/> members production does not use.</summary>
internal static class SuperMetroidRuntimeAccess
{
    /// <summary>Provides debugger-only Samus placement, palette setup, and escape-timer tile upload through an initialized runtime.</summary>
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
            if (self.Room.Camera is null || self.Room.LevelData is null)
            {
                throw new InvalidOperationException(
                    "Landing Site camera and level data must be initialized before grounding debug Samus.");
            }

            // Radius is pose data, so construct/refresh pose $01 before deriving the surface
            // center. The temporary Y=0 is never rendered or stepped.
            self.InitializeDebugSamus(new(xPosition, 0));
            ushort yRadius = self.Player.IntroducedSamus.Kinematics.Radius.Y;

            if ((uint)minimumFloorBlockY >= (uint)self.Room.LevelData.Size.Y)
                throw new ArgumentOutOfRangeException(nameof(minimumFloorBlockY));

            int blockX = xPosition >> 4;
            for (int blockY = minimumFloorBlockY; blockY < self.Room.LevelData.Size.Y; blockY++)
            {
                RoomCollisionBlock floor = self.Room.LevelData.GetCollisionBlock(new(checked((ushort)blockX), checked((ushort)blockY)));

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
                        self._addressSpace,
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
                self.Room.Camera.SetPosition(new Position16<RoomPixels>(
                    // Entry placement clamps a requested origin left of or above the room to zero.
                    (ushort)Math.Max(0, xPosition - 0x60),
                    (ushort)Math.Max(0, restingY - desiredScreenY)));
                self.Player.IntroducedSamus.Position = self.Player.IntroducedSamus.Position.WithWholeY(restingY);
                self.Player.GroundedSamusMovementEnabled = true;
                self.Player.LastGroundedSamusMovement = null;
                self.Player.LastAerialSamusMovement = null;
                self.Player.LastMorphBallMovement = null;
                self.Player.LastBombJumpMovement = null;
                self.Player.LastKnockbackMovement = null;
                self.BombProjectiles.Reset();
                self.Projectiles.Reset(self.Player.Samus);
                return new DebugGroundedSamusPlacement(
                    new(xPosition, restingY),
                    desiredScreenY,
                    new(checked((ushort)blockX), checked((ushort)blockY)),
                    floor,
                    height);
            }

            throw new InvalidOperationException(
                $"No supported solid or upright non-square floor exists beneath " +
                $"Landing Site X=${xPosition:X4} from block row ${minimumFloorBlockY:X2}.");
        }

        /// <summary>Installs the Grapple palette through the same Samus phase owner used by gameplay.</summary>
        internal void LoadDebugGrapplePalette() => self.SamusPhases.LoadGrapplePalette();

        /// <summary>Initializes the ROM-authored standing pose at an explicitly supplied point.</summary>
        internal void InitializeDebugSamus(Position16<RoomPixels> position)
        {
            self.Player.Samus = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose,
                AnimationFrame = 0,
                Position = FixedPosition<RoomPixels>.FromWhole(position),
            };
            self.Artwork.BindSamusPalettePresentation();

            SamusPresentation.LoadPowerSuitPalette(self._addressSpace, self.Cgram, self.Artwork.MapPresentation?.SamusSuitColors);
            self.Player.Samus.PoseMetadata.RefreshCollisionRadii(self._addressSpace);
            self.Player.Samus.Presentation.InitializeAnimation(self._addressSpace);
            if (self.Room.LandingSiteEntry is not null)
            {
                // FootstepGraphics dispatches on the literal area and room-index bytes. Keep
                // those room-owned inputs beside the FX surface state instead of hard-coding
                // “Landing Site” behavior into the generic atmospheric renderer.
                self.Player.Samus.LiquidPhysics.RoomIdentity = self.Room.LandingSiteEntry.RoomIdentity;
            }
            self.Player.PreviousMovementTypeForXray = self.Player.Samus.PoseMetadata.ReadMovementType(self._addressSpace);

            // StepFrame begins with NMI, so prime the definitions now. Otherwise frame one's
            // OAM would name Samus tiles before any corresponding graphics reached VRAM.
            self.Player.Samus.Presentation.PrimeGraphics(self._addressSpace);
        }

        /// <summary>
        /// Queues the two number/label tile transfers in the Ceres table at
        /// <c>$A6:C4CB-$A6:C4D8</c>. The following entries are typewriter BG text and are not
        /// needed by the OAM escape-timer renderer.
        /// </summary>
        [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
        internal void QueueEscapeTimerSpriteTiles()
        {
            if (self.Artwork.MapPresentation is null)
            {
                self.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.FirstByteCount,
                    EscapeTimerTileRomData.FirstSourceAddress,
                    VramDmaDestination.FromNative(EscapeTimerTileAtlasFormat.FirstDestinationWord));
                self.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.SecondByteCount,
                    EscapeTimerTileRomData.SecondSourceAddress,
                    VramDmaDestination.FromNative(EscapeTimerTileAtlasFormat.SecondDestinationWord));
            }
            else
                self.Artwork.MapPresentation.EscapeTimerTiles.QueueTo(self.VramWrites);
        }
    }
}
