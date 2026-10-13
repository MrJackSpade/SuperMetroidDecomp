using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core;
using System.Runtime.CompilerServices;

/// <summary>
/// Verification views of private Core state. Each member reads exactly the private state its
/// test observes; production types expose nothing for tests.
/// </summary>
internal static class CoreAccess
{
    extension(SuperMetroidGame game)
    {
        internal SuperMetroidRuntime? RuntimeForVerification => game.runtime;
        internal int AttractDemoHoldFramesRemaining => game.demoHoldFramesRemaining;
        internal bool GameplayMovementEnabled => game.RuntimeForVerification?.Player.GroundedSamusMovementEnabled ?? false;
        internal CartridgeAudioState AudioForVerification => game.audio;
        /// <summary>The acknowledgements the session's SPC port model published for the next update.</summary>
        internal CartridgeAudioAcknowledgements AudioAcknowledgementsForVerification => game.audioSession.Acknowledgements;
        internal ushort MenuNmiFrameCounterForVerification => game.menuNmi.FrameCounter;
        internal CeresDestructionCinematicState? CeresDestructionForVerification =>
            game.ceresDestruction;

        /// <summary>
        /// Applies the controller read of a native NMI accepted during the door music-wait APU
        /// upload, which the lag-free port does not spend as an update. Only that proven wait
        /// is accepted; replays of any other hardware stall must establish their own contract.
        /// </summary>
        internal void AcceptDoorMusicWaitControllerRead(ushort controllerInput)
        {
            SuperMetroidRuntime? runtime = game.RuntimeForVerification;
            if (game.GameState != SuperMetroidGameState.LoadingNextRoomB || runtime is null)
                throw new InvalidOperationException(
                    $"A door music-wait controller read arrived in game state {game.GameState}.");
            runtime.Controller1.Latch(runtime.Options.ControllerBindings.Normalize(controllerInput));
        }
        internal SamusPoseId GameplaySamusPose => game.RuntimeForVerification?.Player.Samus?.Pose ?? 0;
        internal ushort GameplaySamusX => game.RuntimeForVerification?.Player.Samus?.Position.X.Whole ?? 0;
        internal ushort GameplaySamusY => game.RuntimeForVerification?.Player.Samus?.Position.Y.Whole ?? 0;
    }

    extension(IntroCinematicState intro)
    {
        internal ushort ActiveFlashbackProjectileCount =>
            intro.flashbackProjectiles.SlotTable.ProjectileCounter;
        internal ushort FlashbackSamusX => intro.flashbackSamus?.Position.X.Whole ?? 0;
        internal ushort FlashbackSamusY => intro.flashbackSamus?.Position.Y.Whole ?? 0;
        internal ushort MotherBrainHitCount =>
            intro.flashbackMotherBrain?.HitCount ?? 0;
    }

    extension(EndingCreditsState ending)
    {
        internal byte Brightness => ending.brightness;
        internal ushort CinematicFrame => ending.cinematicFrame;
        internal ushort CreditsVerticalScroll => ending.credits?.VerticalScroll ?? 0;
    }

    extension(PauseMenuState pause)
    {
        internal Position16<OamPixels> LastIndicatorOrigin => pause.lastIndicatorOrigin;
        /// <summary>True while an L/R page switch is fading out, loading or fading in.</summary>
        internal bool PageTransitionActive => pause.transition != PauseMenuTransition.None;
        internal Position16<MapPixels> MapScroll => pause.mapScrollPosition;

        [PpuLayoutBoundary("rendering view: the pause map BG1 tilemap word displayed at a map cell")]
        internal MapTileWord ReadDisplayedMapTile(Position16<MapTiles> cell)
        {
            var vram = pause.vram;
            return new MapTileWord(vram.ReadWord(PauseMenuLayout.Bg1TilemapWord + AreaMapLayout.GetTilemapWordIndex(cell)));
        }
    }

    extension(EndingShootingStars shootingStars)
    {
        internal ReadOnlySpan<EndingShootingStar> Stars => shootingStars.stars;
    }

    extension(ExtractedAudioAssetCatalog catalog)
    {
        internal int CanonicalSampleCount => catalog.canonicalSampleCount;
        internal int SourceMappingCount => catalog.sampleBanks
            .Values.Sum(bank => bank.Samples.Count);
    }

    extension(FrontendFrame frame)
    {
        internal FrontendFrame WithCopiedPixels() => frame with { Pixels = frame.Pixels.ToArray() };
    }

    extension(CapturedFrontendFrame frame)
    {
        internal bool UsedLegacyRaster => frame.Snapshot is null;
    }

    extension(GameplayBasePaletteCatalog palettes)
    {
        internal ReadOnlySpan<Bgr555> Initial => palettes.initial;
    }

    extension(HudState hud)
    {
        internal ReadOnlySpan<ushort> Tiles => hud._tiles;
    }

    extension(MapTileWord tile)
    {
        internal SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(tile.Raw & 0xc000);
    }

    extension(OamBuffer oam)
    {
        [PpuLayoutBoundary("codec: OAM low/high-table DMA upload payload")]
        internal byte[] CreateUploadPayload()
        {
            var payload = new byte[OamBuffer.UploadByteCount];
            oam._lowTable.CopyTo(payload, 0);
            oam._highTable.CopyTo(payload, OamBuffer.LowTableByteCount);
            return payload;
        }
    }

    extension(RenderPublicationProfile profile)
    {
        internal void Reset()
        {
            profile.VerifyOwner();
            profile.Ticks = 0L;
            profile.AllocatedBytes = 0L;
            profile.Publications = (int)0L;
        }
    }

    extension(RoomLevelData level)
    {
        internal ReadOnlyMemory<ushort> BackgroundEntries => level._backgroundEntries;
    }

    extension(RoomPaletteFxSystem fx)
    {
        internal int ActiveCount => fx.slots.Count(slot => slot.Id != 0);
    }

    extension(SuperMetroidAddressSpace bus)
    {
        internal Span<byte> WorkRam => bus._workRam;
        internal Span<byte> SaveRam => bus._saveRam;
    }

    extension(VramWriteQueue queue)
    {
        internal IReadOnlyList<VramWriteEntry> Entries => queue._entries;
    }

    extension(SamusGrappleMovement)
    {
        /// <summary>Accepted grapple firing with its connection pose applied immediately, as ordinary firing does.</summary>
        internal static GrappleMovementResult ConnectAcceptedFiring(ISnesAddressSpace bus, SamusState samus,
            SamusGrappleState grapple, Position16<RoomPixels> previousPosition,
            bool validateAnchorBlock = true, bool validateAnchorEnemy = false) =>
            SamusGrappleMovement.ConnectAcceptedFiringCore(bus, samus, grapple, previousPosition, validateAnchorBlock, validateAnchorEnemy, false);
    }

    extension(EnemyDrawList drawing)
    {
        /// <summary>The per-layer native draw queues the last enemy pass built, in layer order.</summary>
        internal IReadOnlyList<List<ushort>> QueuesForVerification => drawing._drawQueues;
    }

    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Binds a fixture's address space without loading a population, so a focused check can run one enemy
        /// routine against constructed memory.
        /// </summary>
        internal void BindFixtureBus(ISnesAddressSpace bus) => enemies.Room.Bus = Ensure.NotNull(bus);

        /// <summary>Kraid's mouth hitbox test: the production collision-scratch load followed by its mouth overlap.</summary>
        internal bool KraidMouthHitboxOverlapsShot(RoomEnemySlot body, ushort hitboxPointer, SamusProjectileSlot shot)
        {
            KraidBehavior.KraidCollisionScratch scratch = enemies.KraidBehavior.LoadKraidCollisionScratch(body, hitboxPointer);
            return scratch.OverlapsMouth(new KraidBehavior.KraidCollisionShot(shot.Position.Whole, shot.Radius, shot.Type, shot.Damage));
        }

        internal IReadOnlyList<ChozoStatuePlmRequest> ChozoStatuePlmRequests =>
            enemies.ChozoStatueBehavior._chozoStatuePlmRequests;
        internal IReadOnlyList<ChozoStatueState?> ChozoStatueStates =>
            enemies.ChozoStatueBehavior._chozoStatueStates;
        internal IReadOnlyList<NinjaSpacePirateEnemyState?> NinjaSpacePirateStates =>
            enemies.NinjaSpacePirateBehavior._ninjaSpacePirateStates;
    }

    extension(CeresElevatorArrivalState arrival)
    {
        /// <summary>Whole room position of the moving pad; its X is the X word both projectiles copy from Samus at $86:A313.</summary>
        internal Position16<RoomPixels> PadPosition => arrival.pad.Position;
        internal Position16<RoomPixels> PlatformPosition => arrival.platform.Position;
        /// <summary>True while the moving pad occupies native enemy-projectile slot $22.</summary>
        internal bool PadActive => arrival.pad.Active;
        /// <summary>True while the level-data concealer occupies native enemy-projectile slot $20.</summary>
        internal bool PlatformActive => arrival.platform.Active;
    }

    extension(SuperMetroidRuntime runtime)
    {
        internal ushort SamusLoadAppearanceFramesRemaining => runtime.Player._samusLoadAppearanceFramesRemaining.RawValue;
        internal bool SamusLoadAppearanceActive => runtime.SamusLoadAppearanceFramesRemaining != 0;
    }

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
                    .SelectMany(entry => entry.slot.Station is { } station
                        ? [new StationPlmSnapshot(entry.index, entry.slot.BlockIndex, entry.slot.RoomArgument,
                            station.Kind, station.Triggered, station.SavePhase, lockedOut)]
                        : Array.Empty<StationPlmSnapshot>())
                    .ToArray();
            }
        }
    }

    private static PlmSlot[] PlmSlots(RoomPlmSystem plms) => plms.SlotTable._slots;

    /// <summary>Every room-FX record the compiled catalog defines, in pointer order.</summary>
    internal static IEnumerable<RoomFxRecordDefinition> AllRoomFxRecords()
    {
        for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
            if (RoomFxRecordDefinitions.SelectRecord((ushort)pointer) is RoomFxRecordDefinition record)
                yield return record;
    }

    private static readonly ConditionalWeakTable<SuperMetroidGame, GameplayProjectileFireObserver>
        GameplayProjectileFireObservers = new();

    extension(SamusProjectileSystem projectiles)
    {
        /// <summary>Captures slot occupancy and families before an alpha pass.</summary>
        internal SamusProjectileSlotObservation ObserveSlots() => new(projectiles);
    }

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

    private sealed class GameplayProjectileFireObserver
    {
        private SamusProjectileSystem? _projectiles;
        private ushort _frameNumber;
        private SamusProjectileSlotObservation? _slots;
        private int? _firedSlot;

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
                _firedSlot = _slots!.FiredSlot(projectiles);
            }

            _projectiles = projectiles;
            _frameNumber = game.FrameNumber;
            _slots = current;
            return _firedSlot;
        }
    }

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
                .Select(slot => slot.Scroll)
                .OfType<ScrollPlms.ScrollPlmState>()];
            if (owners.Length != 1)
                throw new InvalidOperationException(
                    $"Expected one live scroll PLM at block {blockIndex}; found {owners.Length}.");
            return owners[0].Triggered;
        }
    }

    extension(CrateriaLightningPaletteFxProgramDefinition self)
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

    extension(SamusShinesparkState self)
    {
        /// <summary>Angular-travel word of departing crash-echo projectile slot three.</summary>
        internal SnesAngle FirstReleasedCrashEchoAngle =>
            self._firstReleasedCrashEcho.Angle;

        /// <summary>Angular-travel word of departing crash-echo projectile slot four.</summary>
        internal SnesAngle SecondReleasedCrashEchoAngle =>
            self._secondReleasedCrashEcho.Angle;

        /// <summary>Departing radius (projectile X velocity) of crash-echo projectile slot three.</summary>
        internal ushort FirstReleasedCrashEchoRadius =>
            self._firstReleasedCrashEcho.Radius;
    }

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

    extension(SuperMetroidRuntime self)
    {
        /// <summary>Cached gameplay TS byte the last accepted NMI uploaded with the window registers.</summary>
        [PpuLayoutBoundary("exact hardware test: reads the cached TS register byte")]
        internal byte CachedGameplaySubscreen =>
            (self.GameplayWindows.Cache
                ?? throw new InvalidOperationException("Runtime has no gameplay window register cache."))
            .ReadByte(GameplayWindowRegisterAddresses.Subscreen);
    }

    private static IEnumerable<PlmSlot> Group4ActivePlmSlots(RoomPlmSystem plms) =>
        plms.SlotTable._slots.Where(slot => slot.Active);

    private static IEnumerable<CollectiblePlms.CollectiblePlmState> Group4CollectibleItems(RoomPlmSystem plms) =>
        Group4ActivePlmSlots(plms).Select(slot => slot.Item).OfType<CollectiblePlms.CollectiblePlmState>();

    extension(LatestRenderFrameMailbox mailbox)
    {
        /// <summary>
        /// The mailbox's current load/reset generation: publications must carry it, and a
        /// consumer-held packet is current only while its identity matches it.
        /// </summary>
        internal long Generation => mailbox.generation;
    }
}

/// <summary>One active station PLM as a verification snapshot.</summary>
internal readonly record struct StationPlmSnapshot(
    int NativeSlotIndex,
    int BlockIndex,
    ushort RoomArgument,
    StationKind Kind,
    bool Triggered,
    SaveStationPhase SavePhase,
    bool SaveStationLockedOut);

/// <summary>Verification access to <see cref="CartridgeAudioState"/> queues, which production reads only to dispatch.</summary>
internal static class CartridgeAudioStateQueueAccess
{
    extension(CartridgeAudioState self)
    {
        /// <summary>Music queue read/write indices, current timer and slot delays ($063B/$0639/$063F/$0629).</summary>
        internal string MusicQueueForVerification() =>
            $"{self._musicReadPosition:X}/{self._musicWritePosition:X} " +
            $"t={self._musicTimer:X} d=" +
            string.Join(",", self._musicDelays.Select(delay => $"{delay.Frames:X}"));

        /// <summary>One entry of an SFX library's queue ($0643+).</summary>
        internal byte SoundQueueEntryForVerification(int queue, int index) =>
            self._soundQueues[queue, index];

        /// <summary>Queue start/next indices and dispatcher state of one SFX library ($0643+/$0646+/$0649+).</summary>
        internal (byte Start, byte Next, byte State) SoundQueueForVerification(int queue) =>
            (self._soundReadPositions[queue], self._soundWritePositions[queue],
             self._soundStates[queue]);
    }
}
internal static class SuperMetroidRuntimeFrameAccess
{
    extension(SuperMetroid.Core.Runtime.SuperMetroidRuntime self)
    {
        /// <summary>One ordinary state-eight frame: departure allowed, game time advancing, deferred audio (#633).</summary>
        internal SuperMetroid.Core.Runtime.RuntimeFrameResult StepFrame(ushort controller1Input) =>
            self.StepFrame(controller1Input, new SuperMetroid.Core.Runtime.RuntimeFrameRequest());
    }
}
internal static class FixedPositionAccess
{
    extension<TSpace>(SuperMetroid.Core.Spatial.FixedPosition<TSpace>) where TSpace : SuperMetroid.Core.Spatial.ISpatialSpace
    {
        /// <summary>Assembles both axes from native whole and subpixel words, as verifier fixtures state them.</summary>
        [SuperMetroid.Core.Spatial.SpatialComponents("Verifier codec: the four WRAM words of one 16.16 position.")]
        internal static SuperMetroid.Core.Spatial.FixedPosition<TSpace> FromWords(ushort xWhole, ushort xFraction, ushort yWhole, ushort yFraction) =>
            new(new SuperMetroid.Core.Hardware.SnesFixedPosition(xWhole, xFraction), new SuperMetroid.Core.Hardware.SnesFixedPosition(yWhole, yFraction));
    }
}


