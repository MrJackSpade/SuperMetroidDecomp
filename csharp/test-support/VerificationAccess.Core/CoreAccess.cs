using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core;
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
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

namespace SuperMetroid.Core;

/// <summary>
/// Verification views of private Core state. Each member reads exactly the private state its
/// test observes; production types expose nothing for tests.
/// </summary>
internal static class CoreAccess
{
    /// <summary>Kraid's mouth hitbox test: the production collision-scratch load followed by its mouth overlap.</summary>
    internal static bool KraidMouthHitboxOverlapsShot(RoomEnemySlot body, ushort hitboxPointer, SamusProjectileSlot shot)
    {
        KraidBehavior.KraidCollisionScratch scratch = KraidBehavior.LoadKraidCollisionScratch(body, hitboxPointer);
        return scratch.OverlapsMouth(new KraidBehavior.KraidCollisionShot(shot.Position.Whole, shot.Radius, shot.Type, shot.Damage));
    }

    /// <summary>Exposes frontend-owned gameplay runtime, attract timing, audio, and Samus observations for verification.</summary>
    extension(SuperMetroidGame game)
    {
        /// <summary>Reads the gameplay runtime currently owned by the frontend state machine.</summary>
        internal SuperMetroidRuntime? RuntimeForVerification => game.Session.runtime;
        /// <summary>The gameplay runtime a test requires: the frontend must be in a loaded game.</summary>
        internal SuperMetroidRuntime GameplayRuntimeForVerification => game.Session.GameplayRuntime;
        /// <summary>Remaining authored hold frames before the attract demo advances.</summary>
        internal int AttractDemoHoldFramesRemaining => game.Session.demoHoldFramesRemaining;
        /// <summary>Whether the current runtime permits ordinary grounded Samus movement.</summary>
        internal bool GameplayMovementEnabled => game.RuntimeForVerification?.Player.GroundedSamusMovementEnabled ?? false;
        /// <summary>Reads the frontend audio queue owner across gameplay and menus.</summary>
        internal CartridgeAudioState AudioForVerification => game.Session.audio;
        /// <summary>The acknowledgements the session's SPC port model published for the next update.</summary>
        internal CartridgeAudioAcknowledgements AudioAcknowledgementsForVerification => game.Session.audioSession.Acknowledgements;
        /// <summary>The most recent update's ordered semantic audio events.</summary>
        internal IReadOnlyList<AudioEvent> AudioEventsForVerification => game.Session.lastAudioEvents;
        /// <summary>Reads the session frame clock's 16-bit counter, which the frontend advances while no runtime exists.</summary>
        internal int MenuNmiFrameCounterForVerification => game.Session.frameClock.Counter;
        /// <summary>Reads the active Ceres destruction cinematic, if that phase owns the frontend.</summary>
        internal CeresDestructionCinematicState? CeresDestructionForVerification =>
            game.Session.ceresDestruction;

        /// <summary>
        /// Applies the controller read of a native NMI accepted during the door music-wait APU
        /// upload, which the lag-free port does not spend as an update. Only that proven wait
        /// is accepted; replays of any other hardware stall must establish their own contract.
        /// </summary>
        internal void AcceptDoorMusicWaitControllerRead(SnesButton controllerInput)
        {
            SuperMetroidRuntime? runtime = game.RuntimeForVerification;
            if (game.GameState != SuperMetroidGameState.LoadingNextRoomB || runtime is null)
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
        internal ushort ActiveFlashbackProjectileCount =>
            intro.flashbackProjectiles.SlotTable.ProjectileCounter;
        /// <summary>Flashback Samus's whole-pixel X coordinate when that actor exists.</summary>
        internal ushort FlashbackSamusX => intro.flashbackSamus?.Position.X.Whole ?? 0;
        /// <summary>Flashback Samus's whole-pixel Y coordinate when that actor exists.</summary>
        internal ushort FlashbackSamusY => intro.flashbackSamus?.Position.Y.Whole ?? 0;
        /// <summary>Hits recorded by the intro's flashback Mother Brain sprite.</summary>
        internal int MotherBrainHitCount =>
            intro.flashbackMotherBrain?.HitCount ?? 0;
    }

    /// <summary>Observes credits brightness, cinematic frame, and vertical crawl position.</summary>
    extension(EndingCreditsState ending)
    {
        /// <summary>Current credits fade brightness, 0 (black) through 15 (full), from the private cinematic state.</summary>
        internal int Brightness => ending.brightness;
        /// <summary>Cinematic calls the ending has made so far.</summary>
        internal int CinematicFrame => ending.cinematicFrame;
        /// <summary>Whole-pixel vertical credits scroll position, or zero before credits exist.</summary>
        internal int CreditsVerticalScroll => ending.credits?.ScrollPosition.Y ?? 0;
    }

    /// <summary>Exposes pause indicator placement, page transition, and displayed map tile state.</summary>
    extension(PauseMenuState pause)
    {
        /// <summary>Last OAM origin used for the pause menu's selection indicator.</summary>
        internal Position16<OamPixels> LastIndicatorOrigin => pause.lastIndicatorOrigin;
        /// <summary>True while an L/R page switch is fading out, loading or fading in.</summary>
        internal bool PageTransitionActive => pause.transition != PauseMenuTransition.None;
        /// <summary>Map-space scroll point currently displayed by the pause menu.</summary>
        internal Position16<MapPixels> MapScroll => pause.mapScrollPosition;

        /// <summary>Reads the displayed BG1 map tile word from pause-menu VRAM.</summary>
        [PpuLayoutBoundary("rendering view: the pause map BG1 tilemap word displayed at a map cell")]
        internal MapTileWord ReadDisplayedMapTile(Position16<MapTiles> cell)
        {
            var vram = pause.vram;
            return new MapTileWord(vram.ReadWord(PauseMenuLayout.Bg1TilemapWord + AreaMapLayout.GetTilemapWordIndex(cell)));
        }
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

    /// <summary>Reads the installed initial gameplay palette for exact color comparisons.</summary>
    extension(GameplayBasePaletteCatalog palettes)
    {
        /// <summary>Initial gameplay CGRAM colors retained by the loaded palette catalog.</summary>
        internal ReadOnlySpan<Bgr555> Initial => palettes.initial;
    }

    /// <summary>Observes staged HUD tile words before their VRAM transfer.</summary>
    extension(HudState hud)
    {
        /// <summary>Live mutable HUD tilemap words before their next transfer.</summary>
        internal ReadOnlySpan<ushort> Tiles => hud._tiles;
    }

    /// <summary>Extracts the high-bit flip flags from a rendered map tile word.</summary>
    extension(MapTileWord tile)
    {
        /// <summary>Horizontal and vertical flip bits encoded in a map tile word.</summary>
        internal SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(tile.Raw & 0xc000);
    }

    /// <summary>Builds the exact OAM upload byte payload from a prepared sprite buffer.</summary>
    extension(OamBuffer oam)
    {
        /// <summary>Concatenates OAM low and high tables into the native DMA upload order.</summary>
        [PpuLayoutBoundary("codec: OAM low/high-table DMA upload payload")]
        internal byte[] CreateUploadPayload()
        {
            var payload = new byte[OamBuffer.UploadByteCount];
            oam._lowTable.CopyTo(payload, 0);
            oam._highTable.CopyTo(payload, OamBuffer.LowTableByteCount);
            return payload;
        }
    }

    /// <summary>Resets render publication counters to a baseline for frame-path verification.</summary>
    extension(RenderPublicationProfile profile)
    {
        /// <summary>Validates the profile owner and clears ticks, allocations, and publications.</summary>
        internal void Reset()
        {
            profile.VerifyOwner();
            profile.Ticks = 0L;
            profile.AllocatedBytes = 0L;
            profile.Publications = (int)0L;
        }
    }

    /// <summary>Reads the room's background tilemap entries independently of collision blocks.</summary>
    extension(RoomLevelData level)
    {
        /// <summary>Private background-entry array used when assembling room layers.</summary>
        internal ReadOnlyMemory<ushort> BackgroundEntries => level._backgroundEntries;
    }

    /// <summary>Counts occupied room palette-FX slots in the private effect pool.</summary>
    extension(RoomPaletteFxSystem fx)
    {
        /// <summary>Counts palette FX slots whose native ID word is nonzero.</summary>
        internal int ActiveCount => fx.slots.Count(slot => slot.Id != 0);
    }

    /// <summary>Exposes emulated WRAM and SRAM bytes for exact state assertions.</summary>
    extension(SuperMetroidAddressSpace bus)
    {
        /// <summary>Mutable emulated WRAM for focused verifier setup.</summary>
        internal Span<byte> WorkRam => bus._workRam;
    }

    /// <summary>Reads queued VRAM writes before the display upload consumes them.</summary>
    extension(VramWriteQueue queue)
    {
        /// <summary>Pending VRAM transfer entries in queue order.</summary>
        internal IReadOnlyList<VramWriteEntry> Entries => queue._entries;
    }

    /// <summary>Invokes the internal accepted-grapple connection path with its anchor-owner arguments.</summary>
    extension(SamusGrappleMovement)
    {
        /// <summary>Accepted grapple firing with its connection pose applied immediately, as ordinary firing does.</summary>
        internal static GrappleMovementResult ConnectAcceptedFiring(SamusState samus,
            SamusGrappleState grapple, Position16<RoomPixels> previousPosition,
            bool validateAnchorBlock = true, bool validateAnchorEnemy = false) =>
            SamusGrappleMovement.ConnectAcceptedFiringCore(samus, grapple, previousPosition, validateAnchorBlock, validateAnchorEnemy, false);
    }

    /// <summary>Observes per-priority enemy draw queues before OAM construction.</summary>
    extension(EnemyDrawList drawing)
    {
        /// <summary>The per-layer native draw queues the last enemy pass built, in layer order.</summary>
        internal IReadOnlyList<List<ushort>> QueuesForVerification => drawing._drawQueues;
    }

    /// <summary>Rebinds enemy fixtures and inspects Kraid collision, statue requests, and pirate slot state.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Binds an enemy definition source without loading a population, so a focused check can run one enemy
        /// routine against constructed records.
        /// </summary>
        internal void BindFixtureDefinitions(IRoomEnemyDefinitionSource definitions) =>
            enemies.Room.Definitions = Ensure.NotNull(definitions);

        /// <summary>Room PLM requests emitted by Chozo statue AI in their publication order.</summary>
        internal IReadOnlyList<ChozoStatuePlmRequest> ChozoStatuePlmRequests =>
            enemies.ChozoStatueBehavior._chozoStatuePlmRequests;
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
        internal ushort SamusLoadAppearanceFramesRemaining => runtime.Player._samusLoadAppearanceFramesRemaining.RawValue;
        /// <summary>Whether the room-load appearance timer still owns Samus.</summary>
        internal bool SamusLoadAppearanceActive => runtime.SamusLoadAppearanceFramesRemaining != 0;
    }

    /// <summary>Finds a unique population PLM and snapshots active save stations in descending native slot order.</summary>
    extension(RoomPlmSystem plms)
    {

        /// <summary>The one active slot running <paramref name="headerPointer"/>; throws unless exactly one does.</summary>
        internal RoomPlmSlotSnapshot SinglePopulationSlot(PlmHeaderId headerPointer)
        {
            RoomPlmSlotSnapshot[] matches = plms.PopulationSlots
                .Where(slot => slot.HeaderPointer == headerPointer).ToArray();
            return matches.Length switch
            {
                1 => matches[0],
                0 => throw new InvalidOperationException($"No active PLM uses header ${(ushort)headerPointer:X4}."),
                _ => throw new InvalidOperationException($"More than one active PLM uses header ${(ushort)headerPointer:X4}."),
            };
        }

        /// <summary>Active station PLMs, highest native slot first.</summary>
        internal IReadOnlyList<StationPlmSnapshot> StationSnapshots
        {
            get
            {
                bool lockedOut = plms.Stations._saveStationLockedOut;
                return PlmSlots(plms)
                    .Select((slot, index) => (slot, index))
                    .Where(entry => entry.slot.Active)
                    .OrderByDescending(entry => entry.index)
                    .SelectMany(entry => entry.slot.Family is StationPlms.StationPlmState station
                        ? [new StationPlmSnapshot(entry.index, entry.slot.BlockIndex, entry.slot.RoomArgument,
                            station.Kind, station.Triggered, station.SavePhase, lockedOut)]
                        : Array.Empty<StationPlmSnapshot>())
                    .ToArray();
            }
        }
    }

    /// <summary>Exposes physical PLM slots to the station snapshot reader.</summary>
    private static PlmSlot[] PlmSlots(RoomPlmSystem plms) => plms.SlotTable._slots;

    /// <summary>Every room-FX record the compiled catalog defines, in pointer order.</summary>
    internal static IEnumerable<RoomFxRecordDefinition> AllRoomFxRecords()
    {
        for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
            if (RoomFxRecordDefinitions.SelectRecord((ushort)pointer) is RoomFxRecordDefinition record)
                yield return record;
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
        private ushort _frameNumber;
        /// <summary>Prior occupancy and projectile-family snapshot.</summary>
        private SamusProjectileSlotObservation? _slots;
        /// <summary>Allocation attributed to the most recently observed frame.</summary>
        private int? _firedSlot;

        /// <summary>Rejects skipped frames and infers a newly allocated slot from consecutive snapshots.</summary>
        internal int? Observe(SuperMetroidGame game)
        {
            SamusProjectileSystem? projectiles = game.RuntimeForVerification?.Projectiles;
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
            else if (game.FrameNumber != unchecked((ushort)(_frameNumber + 1)))
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
    extension(RoomPlmSystem plms)
    {
        /// <summary>
        /// Native PLM_Vars bit fifteen of the live resident scroll trigger at
        /// <paramref name="blockIndex"/>: set by `$84:B393` contact, cleared when the
        /// bank-$8F program returns the object to sleep. Throws unless exactly one live
        /// scroll PLM owns that block.
        /// </summary>
        internal bool ScrollPlmTriggered(int blockIndex)
        {
            ScrollPlms.ScrollPlmState[] owners = [.. plms.SlotTable._slots
                .Where(slot => slot.Active && slot.BlockIndex == blockIndex)
                .Select(slot => slot.Family as ScrollPlms.ScrollPlmState)
                .OfType<ScrollPlms.ScrollPlmState>()];
            if (owners.Length != 1)
                throw new InvalidOperationException(
                    $"Expected one live scroll PLM at block {blockIndex}; found {owners.Length}.");
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
        internal IReadOnlyList<(ushort Pointer, byte Value)> TimerMechanicsBytes =>
        [
            (unchecked((ushort)(self.TimerTwoPointer + 2)), 2),
            (unchecked((ushort)(self.TimerOnePointer + 2)), 1),
        ];
    }

    /// <summary>Observes the two released crash echoes' travel angles and first echo radius.</summary>
    extension(SamusShinesparkState self)
    {
        /// <summary>Angular-travel word of departing crash-echo projectile slot three.</summary>
        internal SnesAngle FirstReleasedCrashEchoAngle =>
            self._firstReleasedCrashEcho.Angle;

        /// <summary>Angular-travel word of departing crash-echo projectile slot four.</summary>
        internal SnesAngle SecondReleasedCrashEchoAngle =>
            self._secondReleasedCrashEcho.Angle;

        /// <summary>Departing radius (projectile X velocity) of crash-echo projectile slot three.</summary>
        internal int FirstReleasedCrashEchoRadius =>
            self._firstReleasedCrashEcho.Radius;
    }

    /// <summary>Lists active PLM block positions and collectible phases and artwork in slot order.</summary>
    extension(RoomPlmSystem self)
    {
        /// <summary>Block index of every active PLM, in native slot order.</summary>
        internal IReadOnlyList<int> ActivePlmBlockIndices =>
        [
            .. Group4ActivePlmSlots(self).Select(slot => slot.BlockIndex),
        ];

        /// <summary>Phase of every permanent-item PLM, in the same order as <see cref="CollectiblePlms.Collectibles"/>.</summary>
        internal IReadOnlyList<CollectiblePhase> CollectiblePhases =>
        [
            .. Group4CollectibleItems(self).Select(item => item.Phase),
        ];

        /// <summary>Presentation of every permanent-item PLM, in the same order as <see cref="CollectiblePlms.Collectibles"/>.</summary>
        internal IReadOnlyList<CollectiblePresentation> CollectiblePresentations =>
        [
            .. Group4CollectibleItems(self).Select(item => item.Presentation),
        ];
    }

    /// <summary>Snapshots mailbox publication counters and pending-frame state under its synchronization lock.</summary>
    extension(LatestRenderFrameMailbox self)
    {
        /// <summary>Reads every private counter under the mailbox's own lock.</summary>
        internal RenderMailboxCounterView VerificationCounters
        {
            get
            {
                lock (self.sync)
                {
                    return new RenderMailboxCounterView(
                        self.generation,
                        self.published,
                        self.taken,
                        self.replaced,
                        self.invalidated,
                        self.pending is not null);
                }
            }
        }
    }

    /// <summary>Reads the gameplay window cache's last accepted TS subscreen byte.</summary>
    extension(SuperMetroidRuntime self)
    {
        /// <summary>Cached gameplay TS byte the last accepted NMI uploaded with the window registers.</summary>
        [PpuLayoutBoundary("exact hardware test: reads the cached TS register byte")]
        internal byte CachedGameplaySubscreen =>
            (self.GameplayWindows.Cache
                ?? throw new InvalidOperationException("Runtime has no gameplay window register cache."))
            .ReadByte(GameplayWindowRegisterAddresses.Subscreen);
    }

    /// <summary>Visits active physical PLM slots in native slot order.</summary>
    private static IEnumerable<PlmSlot> Group4ActivePlmSlots(RoomPlmSystem plms) =>
        plms.SlotTable._slots.Where(slot => slot.Active);

    /// <summary>Extracts live collectible state from the active PLM slots.</summary>
    private static IEnumerable<CollectiblePlms.CollectiblePlmState> Group4CollectibleItems(RoomPlmSystem plms) =>
        Group4ActivePlmSlots(plms).Select(slot => slot.Family).OfType<CollectiblePlms.CollectiblePlmState>();

    /// <summary>Reads the mailbox generation used to reject stale frame publications.</summary>
    extension(LatestRenderFrameMailbox mailbox)
    {
        /// <summary>
        /// The mailbox's current load/reset generation: publications must carry it, and a
        /// consumer-held packet is current only while its identity matches it.
        /// </summary>
        internal long Generation => mailbox.generation;
    }
}
