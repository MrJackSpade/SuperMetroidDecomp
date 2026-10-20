using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Runtime;

/// <summary>Verification access to <see cref="SuperMetroidRuntime"/> members production does not use.</summary>
internal static class SuperMetroidRuntimeAccess
{
    /// <summary>Provides debugger-only Samus placement, palette setup, and escape-timer tile upload through an initialized runtime.</summary>
    extension(SuperMetroidRuntime self)
    {
        /// <summary>The loaded room's PLM system, which each room builds with its scope.</summary>
        internal RoomPlmSystem Plms => self.Room.LoadedScope.Plms;

        /// <summary>The loaded room's Samus projectiles.</summary>
        internal SamusProjectileSystem Projectiles => self.Room.LoadedScope.Projectiles;

        /// <summary>The loaded room's bombs.</summary>
        internal SamusBombProjectileSystem BombProjectiles => self.Room.LoadedScope.BombProjectiles;

        /// <summary>The loaded room's enemies.</summary>
        internal RoomEnemySystem Enemies => self.Room.LoadedScope.Enemies;

        /// <summary>
        /// Gives the loaded room a new enemy system loaded from <paramref name="request"/>, as a room load builds one, and
        /// returns it. Fixtures use it to place a constructed population into a room that is already running.
        /// </summary>
        internal RoomEnemySystem ReplaceRoomEnemiesForVerification(IRoomEnemyDefinitionSource definitions,
            IRoomEnemyServices services, IRoomEnemyLoad request)
        {
            var enemies = new RoomEnemySystem(self.EnemySession, definitions: definitions, services: services);
            enemies.Load(request);
            self.Room.Adopt(self.Room.LoadedScope with { Enemies = enemies });
            return enemies;
        }

        /// <summary>The loaded room's layer-3 FX.</summary>
        internal RoomLayer3FxState RoomLayer3Fx => self.Room.LoadedScope.Layer3Fx;

        /// <summary>The loaded room's palette FX.</summary>
        internal RoomPaletteFxSystem RoomPaletteFx => self.Room.LoadedScope.PaletteFx;

        /// <summary>The loaded room's Ceres haze.</summary>
        internal CeresHazeState CeresHaze => self.Room.LoadedScope.CeresHaze;

        /// <summary>The loaded room's sand tiles.</summary>
        internal RoomSandAnimatedTilesState SandAnimatedTiles => self.Room.LoadedScope.SandTiles;

        /// <summary>The loaded room's spike tiles.</summary>
        internal RoomSpikeAnimatedTilesState RoomSpikes => self.Room.LoadedScope.SpikeTiles;

        /// <summary>The loaded room's treadmill tiles.</summary>
        internal RoomTreadmillAnimatedTilesState RoomTreadmills => self.Room.LoadedScope.TreadmillTiles;

        /// <summary>The loaded room's Wrecked Ship treadmill tiles.</summary>
        internal WreckedShipTreadmillAnimatedTilesState WreckedShipTreadmill => self.Room.LoadedScope.WreckedShipTreadmill;

        /// <summary>The loaded room's Maridia elevatube room main.</summary>
        internal MaridiaElevatubeRoomMainState MaridiaElevatube => self.Room.LoadedScope.MaridiaElevatube;

        /// <summary>The loaded room's Ceres elevator shaft room main.</summary>
        internal CeresElevatorShaftRoomMainState CeresElevatorShaft => self.Room.LoadedScope.CeresElevatorShaft;

        /// <summary>The loaded room's Tourian statue sequence.</summary>
        internal TourianStatueSequence TourianStatues => self.Room.LoadedScope.TourianStatues;


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

            if ((uint)minimumFloorBlockY >= self.Room.LevelData.Size.Y)
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

            SamusPresentation.LoadPowerSuitPalette(self.Cgram, self.Artwork.MapPresentation?.SamusSuitColors);
            self.Player.Samus.PoseMetadata.RefreshCollisionRadii();
            self.Player.Samus.Presentation.InitializeAnimation();
            if (self.Room.LandingSiteEntry is not null)
            {
                // FootstepGraphics dispatches on the literal area and room-index bytes. Keep
                // those room-owned inputs beside the FX surface state instead of hard-coding
                // “Landing Site” behavior into the generic atmospheric renderer.
                self.Player.Samus.LiquidPhysics.RoomIdentity = self.Room.LandingSiteEntry.RoomIdentity;
            }
            self.Player.PreviousMovementTypeForXray = self.Player.Samus.PoseMetadata.ReadMovementType();

            // StepFrame begins with NMI, so prime the definitions now. Otherwise frame one's
            // OAM would name Samus tiles before any corresponding graphics reached VRAM.
            self.Player.Samus.Presentation.PrimeGraphics();
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
