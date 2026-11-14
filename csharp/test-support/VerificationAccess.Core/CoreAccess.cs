using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Map;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core;

/// <summary>
/// Verification views of private Core state. Each member reads exactly the private state its
/// test observes; production types expose nothing for tests.
/// </summary>
internal static class CoreAccess
{
    /// <summary>Kraid's mouth hitbox test: the production collision-scratch load followed by its mouth overlap.</summary>
    internal static bool KraidMouthHitboxOverlapsShot(RoomEnemySlot body, ushort hitboxPointer, SamusProjectile shot)
    {
        KraidPassBounds bounds = KraidBehaviorConstants.PlaceKraidPassBounds(body, hitboxPointer);
        return KraidCombat.OverlapsKraidMouth(bounds, new KraidBehavior.KraidCollisionShot(shot.Position.Whole, shot.Radius, shot.Type, shot.Damage, IsPlacedBomb: false));
    }

    /// <summary>Exposes frontend-owned gameplay runtime, attract timing, audio, and Samus observations for verification.</summary>
    extension(SuperMetroidGame game)
    {
        /// <summary>Reads the gameplay runtime currently owned by the frontend state machine.</summary>
        internal SuperMetroidRuntime? RuntimeForVerification => game.Session.Runtime;
        /// <summary>The gameplay runtime a test requires: the frontend must be in a loaded game.</summary>
        internal SuperMetroidRuntime GameplayRuntimeForVerification => game.Session.GameplayRuntime;
        /// <summary>Remaining authored hold frames before the attract demo advances.</summary>
        internal int AttractDemoHoldFramesRemaining => game.Session.Demo.HoldFramesRemaining;
        /// <summary>Whether the current runtime permits ordinary grounded Samus movement.</summary>
        internal bool GameplayMovementEnabled => game.RuntimeForVerification?.Player.GroundedSamusMovementEnabled ?? false;
        /// <summary>Reads the frontend audio queue owner across gameplay and menus.</summary>
        internal AudioQueues AudioForVerification => game.Session.audio;
        /// <summary>The acknowledgements the session's SPC port model published for the next update.</summary>
        internal SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> AudioAcknowledgementsForVerification => game.Session.audioSession.Acknowledgements;
        /// <summary>Reads the session frame clock's 16-bit counter, which the frontend advances while no runtime exists.</summary>
        internal int MenuNmiFrameCounterForVerification => game.Session.frameClock.Counter;
        /// <summary>Reads the active Ceres destruction cinematic, if that phase owns the frontend.</summary>
        internal CeresDestructionCinematicState? CeresDestructionForVerification =>
            game.Session.ScreenAs<CeresDestructionCinematicState>();

        /// <summary>
        /// Applies the controller read of a native NMI accepted during the door music-wait APU
        /// upload, which the lag-free port does not spend as an update. Only that proven wait
        /// is accepted; replays of any other hardware stall must establish their own contract.
        /// </summary>
        internal void AcceptDoorMusicWaitControllerRead(ControllerButton controllerInput)
        {
            SuperMetroidRuntime? runtime = game.RuntimeForVerification;
            if (game.GameState != SuperMetroidGameState.DoorTransition || runtime is null)
                throw new InvalidOperationException(
                    $"A door music-wait controller read arrived in game state {game.GameState}.");
            runtime.Controller1.Latch(runtime.Options.ControllerBindings.Normalize(controllerInput));
        }
        /// <summary>Live Samus pose in gameplay, or zero before a runtime is available.</summary>
        internal SamusPoseId GameplaySamusPose => game.RuntimeForVerification?.Player.Samus?.Pose ?? 0;
        /// <summary>Whole-pixel gameplay X position, defaulting to zero outside gameplay.</summary>
        internal ushort GameplaySamusX => game.RuntimeForVerification?.Player.Samus?.Position.X.Whole ?? 0;
        /// <summary>Whole-pixel gameplay Y position, defaulting to zero outside gameplay.</summary>
        internal ushort GameplaySamusY => game.RuntimeForVerification?.Player.Samus?.Position.Y.Whole ?? 0;
    }

    /// <summary>Reads private projectile, Samus, and Mother Brain state during the intro flashback.</summary>
    extension(IntroCinematicState intro)
    {
        /// <summary>Native projectile counter for the intro flashback's private shot system.</summary>
        internal int ActiveFlashbackProjectileCount =>
            intro.flashbackProjectiles.SlotTable.ProjectileCounter;
        /// <summary>Flashback Samus's whole-pixel X coordinate when that actor exists.</summary>
        internal ushort FlashbackSamusX => intro.CurrentFlashbackSamus?.Position.X.Whole ?? 0;
        /// <summary>Flashback Samus's whole-pixel Y coordinate when that actor exists.</summary>
        internal ushort FlashbackSamusY => intro.CurrentFlashbackSamus?.Position.Y.Whole ?? 0;
        /// <summary>Hits recorded by the intro's flashback Mother Brain sprite.</summary>
        internal int MotherBrainHitCount =>
            intro.CurrentFlashbackMotherBrain?.HitCount ?? 0;
    }

    /// <summary>Observes credits brightness, cinematic frame, and vertical crawl position.</summary>
    extension(EndingCreditsState ending)
    {
        /// <summary>Current credits fade brightness, 0 (black) through 15 (full), from the private cinematic state.</summary>
        internal int Brightness => ending.Stage.Brightness;
        /// <summary>Cinematic calls the ending has made so far.</summary>
        internal int CinematicFrame => ending.Flyaway.CinematicFrame;
        /// <summary>Whole-pixel vertical credits scroll position, or zero before credits exist.</summary>
        internal int CreditsVerticalScroll => ending.InstalledArtwork.CurrentCredits?.ScrolledPixels ?? 0;
    }

    /// <summary>Exposes pause page transition, map scroll, and displayed map cell state.</summary>
    extension(PauseMenuState pause)
    {
        /// <summary>True while an L/R page switch is fading out, loading or fading in.</summary>
        internal bool PageTransitionActive => pause.Transition != PauseMenuTransition.None;
        /// <summary>Map-space scroll point currently displayed by the pause menu.</summary>
        internal Position16<MapPixels> MapScroll => pause.MapScrollPosition;

        /// <summary>The map cell the pause screen's map shows at <paramref name="cell"/>, in the shade it shows it in (#1513).</summary>
        internal ShownMapCell ReadDisplayedMapCell(Position16<MapTiles> cell) =>
            pause.ShownMap.Cells[cell.Y * AreaMapSize.WidthInMapTiles + cell.X];
    }

    /// <summary>Reads the shooting-star array in its native update order.</summary>
    extension(EndingShootingStars shootingStars)
    {
        /// <summary>Current private star array in its native update order.</summary>
        internal ReadOnlySpan<EndingShootingStar> Stars => shootingStars.stars;
    }

    /// <summary>Observes canonical sample totals and source mappings after audio extraction.</summary>
    extension(ExtractedAudioAssetCatalog catalog)
    {
        /// <summary>Number of unique samples retained after canonicalization.</summary>
        internal int CanonicalSampleCount => catalog.canonicalSampleCount;
        /// <summary>Total source-sample mappings across installed audio banks.</summary>
        internal int SourceMappingCount => catalog.sampleBanks
            .Values.Sum(bank => bank.Samples.Count);
    }


    /// <summary>Builds the exact OAM upload byte payload from a prepared sprite buffer.</summary>
    extension(OamBuffer oam)
    {
        /// <summary>Concatenates OAM low and high tables into the native DMA upload order.</summary>
        internal byte[] CreateUploadPayload()
        {
            var payload = new byte[OamBufferConstants.UploadByteCount];
            oam.LowTable.CopyTo(payload);
            oam.HighTable.CopyTo(payload.AsSpan(OamBuffer.LowTableByteCount));
            return payload;
        }
    }

    /// <summary>Counts the room's running colour effects.</summary>
    extension(ColourEffectSystem fx)
    {
        /// <summary>The colour effects running.</summary>
        internal int ActiveCount => fx.effects.Count;
    }



    /// <summary>Invokes the internal accepted-grapple connection path with its anchor-owner arguments.</summary>
    extension(SamusGrappleMovement)
    {
        /// <summary>Accepted grapple firing with its connection pose applied immediately, as ordinary firing does.</summary>
        internal static GrappleMovementResult ConnectAcceptedFiring(SamusState samus,
            SamusGrappleState grapple, Position16<RoomPixels> previousPosition,
            GrappleAnchorKind anchor = GrappleAnchorKind.Block) =>
            SamusGrappleMovement.ConnectAcceptedFiringCore(samus, grapple, previousPosition, anchor, GrapplePoseChange.Immediate);
    }

    /// <summary>Rebinds enemy fixtures and inspects Kraid collision, statue requests, and pirate slot state.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>The room objects the Chozo statue has asked for and the room has still to spawn, in the order it asked.</summary>
        internal IReadOnlyList<RoomObjectSpawnRequest<ChozoStatueRoomObject>> ChozoStatueObjectRequests =>
            enemies.ChozoStatueBehavior.ChozoStatueObjectRequests;
        /// <summary>Per-slot Chozo statue states, including unused physical enemy slots.</summary>
        internal IReadOnlyList<ChozoStatueState?> ChozoStatueStates =>
            [.. enemies.SlotTable.Slots.Select(slot => slot.SpeciesState<ChozoStatueState>())];
        /// <summary>Per-slot ninja pirate states for native slot-index verification.</summary>
        internal IReadOnlyList<NinjaSpacePirateEnemyState?> NinjaSpacePirateStates =>
            [.. enemies.SlotTable.Slots.Select(slot => slot.EntityState<NinjaSpacePirateEntity, NinjaSpacePirateEnemyState>())];
    }

    /// <summary>Reads Ceres pad and concealer positions and their projectile-slot activity.</summary>
    extension(CeresElevatorArrivalState arrival)
    {
        /// <summary>Whole room position of the moving pad; its X is the X word both projectiles copy from Samus at $86:A313.</summary>
        internal Position16<RoomPixels> PadPosition => arrival.pad.Position;
        /// <summary>Whole room position of the Ceres elevator's level-data platform.</summary>
        internal Position16<RoomPixels> PlatformPosition => arrival.platform.Position;
        /// <summary>True while the moving pad occupies native enemy-projectile slot $22.</summary>
        internal bool PadActive => arrival.pad.Active;
        /// <summary>True while the level-data concealer occupies native enemy-projectile slot $20.</summary>
        internal bool PlatformActive => arrival.platform.Active;
    }

    /// <summary>Observes Samus's room-load appearance timer before ordinary movement resumes.</summary>
    extension(SuperMetroidRuntime runtime)
    {
        /// <summary>Remaining frames of Samus's room-load appearance state.</summary>
        internal ushort SamusLoadAppearanceFramesRemaining => checked((ushort)runtime.Player.LoadAppearanceFramesRemaining);
        /// <summary>Whether the room-load appearance timer still owns Samus.</summary>
        internal bool SamusLoadAppearanceActive => runtime.SamusLoadAppearanceFramesRemaining != 0;
    }

    /// <summary>Snapshots active stations in processing order.</summary>
    extension(RoomObjectSystem roomObjects)
    {

        /// <summary>Active station PLMs, in processing order.</summary>
        internal IReadOnlyList<StationObjectSnapshot> StationSnapshots
        {
            get
            {
                bool lockedOut = roomObjects.Stations.SaveStationLockedOut;
                return roomObjects.SlotTable.InProcessingOrder
                    .Select((slot, position) => new IndexedObjectSlot(slot, position))
                    .Where(entry => entry.Slot.Active)
                    .SelectMany(entry => entry.Slot.Behaviour is StationObject station
                        ? [new StationObjectSnapshot(entry.Index, entry.Slot.Block, entry.Slot.NativeRoomArgument,
                            station.Kind, station.Triggered, station.SavePhase, lockedOut)]
                        : Array.Empty<StationObjectSnapshot>())
                    .ToArray();
            }
        }
    }

    /// <summary>Every room-FX record the compiled catalog defines, in pointer order.</summary>
    internal static IEnumerable<RoomEffectDefinition> AllRoomFxRecords()
    {
        return RoomFxDefinitions.AllRecords;
    }

    /// <summary>Keeps per-game projectile observations without extending frontend lifetime.</summary>
    private static readonly ConditionalWeakTable<SuperMetroidGame, GameplayProjectileFireObserver>
        GameplayProjectileFireObservers = new();

    /// <summary>Captures ordinary projectile slot occupancy and families around an alpha pass.</summary>
    extension(SamusProjectileSystem projectiles)
    {
        /// <summary>Captures slot occupancy and families before an alpha pass.</summary>
        internal SamusProjectileSlotObservation ObserveSlots() => new(projectiles);
    }

    /// <summary>Attributes a newly fired ordinary projectile to the latest consecutively observed frontend frame.</summary>
    extension(SuperMetroidGame game)
    {
        /// <summary>
        /// The ordinary projectile slot allocated during the most recent <c>Step</c>, observed
        /// by comparing live slot occupancy with the previous frame's observation. Callers
        /// must observe every frame (repeat calls within one frame return the same answer);
        /// a skipped frame or a first observation with shots already live fails loudly
        /// because no allocation can be attributed to it.
        /// </summary>
        internal int? GameplayLastFiredProjectileSlot =>
            GameplayProjectileFireObservers.GetValue(game, _ => new GameplayProjectileFireObserver())
                .Observe(game);
    }

    /// <summary>Attributes an ordinary projectile allocation to exactly one observed frontend frame.</summary>
    private sealed class GameplayProjectileFireObserver
    {
        /// <summary>Projectile owner observed in the preceding frame.</summary>
        private SamusProjectileSystem? _projectiles;
        /// <summary>Frontend frame number associated with the preceding slot snapshot.</summary>
        private int _frameNumber;
        /// <summary>Prior occupancy and projectile-family snapshot.</summary>
        private SamusProjectileSlotObservation? _slots;
        /// <summary>Allocation attributed to the most recently observed frame.</summary>
        private int? _firedSlot;

        /// <summary>Rejects skipped frames and infers a newly allocated slot from consecutive snapshots.</summary>
        internal int? Observe(SuperMetroidGame game)
        {
            SamusProjectileSystem? projectiles = game.RuntimeForVerification?.Room.Scope?.Projectiles;
            if (projectiles is null)
            {
                _projectiles = null;
                _slots = null;
                _firedSlot = null;
                return null;
            }

            if (ReferenceEquals(projectiles, _projectiles) && game.FrameNumber == _frameNumber)
                return _firedSlot;

            var current = new SamusProjectileSlotObservation(projectiles);
            if (!ReferenceEquals(projectiles, _projectiles))
            {
                if (current.AnyOccupied)
                    throw new InvalidOperationException(
                        $"First projectile observation at frame {game.FrameNumber} already has live " +
                        "ordinary projectiles; no prior frame exists to attribute their allocation.");
                _firedSlot = null;
            }
            else if (game.FrameNumber != _frameNumber + 1)
            {
                throw new InvalidOperationException(
                    $"Projectile observation skipped from frame {_frameNumber} to {game.FrameNumber}; " +
                    "allocations in the unobserved frames cannot be attributed.");
            }
            else
            {
                _firedSlot = (_slots ?? throw new InvalidOperationException("Verification requires _slots.")).FiredSlot(projectiles);
            }

            _projectiles = projectiles;
            _frameNumber = game.FrameNumber;
            _slots = current;
            return _firedSlot;
        }
    }

    /// <summary>Reads the contact latch of the unique live scroll PLM at a room block.</summary>
    extension(RoomObjectSystem roomObjects)
    {
        /// <summary>
        /// Whether the live scroll trigger at <paramref name="block"/> has been touched and has its writes still to
        /// run. Throws unless exactly one live scroll trigger owns that block.
        /// </summary>
        internal bool ScrollObjectTriggered(Position16<RoomBlocks> block)
        {
            ScrollTriggerObject[] owners = [.. roomObjects.SlotTable.InProcessingOrder
                .Where(slot => slot.Active && slot.Block == block)
                .Select(slot => slot.Behaviour)
                .OfType<ScrollTriggerObject>()];
            if (owners.Length != 1)
                throw new InvalidOperationException(
                    $"Expected one live scroll PLM at block {block}; found {owners.Length}.");
            return owners[0].Triggered;
        }
    }

    /// <summary>Exposes the two byte-sized timer operands following Crateria lightning SetTimer instructions.</summary>
    extension(CrateriaLightningPaletteFxColorDefinition self)
    {
        /// <summary>
        /// The two timer-duration bytes that follow each <c>SetTimer</c> instruction: duration
        /// two after the second-timer instruction and duration one after the first-timer one.
        /// </summary>
        internal IReadOnlyList<TimerByte> TimerMechanicsBytes =>
        [
            new TimerByte(unchecked((ushort)(self.TimerTwoPointer + 2)), 2),
            new TimerByte(unchecked((ushort)(self.TimerOnePointer + 2)), 1),
        ];
    }

    /// <summary>Lists active PLM block positions and collectible phases and artwork in slot order.</summary>
    extension(RoomObjectSystem self)
    {
        /// <summary>The room block of every active PLM, by slot ordinal.</summary>
        internal IReadOnlyList<Position16<RoomBlocks>> ActiveObjectBlocks =>
        [
            .. Group4ActiveObjectSlots(self).Select(slot => slot.Block),
        ];

        /// <summary>Phase of every permanent-item PLM, in the same order as <see cref="CollectibleObjects.Collectibles"/>.</summary>
        internal IReadOnlyList<CollectiblePhase> CollectiblePhases =>
        [
            .. Group4CollectibleItems(self).Select(item => item.Phase),
        ];

        /// <summary>Presentation of every permanent-item PLM, in the same order as <see cref="CollectibleObjects.Collectibles"/>.</summary>
        internal IReadOnlyList<CollectiblePresentation> CollectiblePresentations =>
        [
            .. Group4CollectibleItems(self).Select(item => item.Presentation),
        ];
    }


    /// <summary>The room's occupied places, the last in update order first, as the checks have always listed them.</summary>
    private static IEnumerable<RoomObjectSlot> Group4ActiveObjectSlots(RoomObjectSystem roomObjects) =>
        roomObjects.SlotTable.FromLastPlace.Where(slot => slot.Active);

    /// <summary>Extracts live collectible state from the active PLM slots.</summary>
    private static IEnumerable<CollectibleObject> Group4CollectibleItems(RoomObjectSystem roomObjects) =>
        Group4ActiveObjectSlots(roomObjects).Select(slot => slot.Behaviour).OfType<CollectibleObject>();
}
