using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Verification views of private Core state. Each member reads exactly the private state its
/// test observes; production types expose nothing for tests.
/// </summary>
internal static partial class CoreAccess
{
    /// <summary>Exposes selected live game state used to assert transitions and input handling.</summary>
    extension(SuperMetroidGame game)
    {
        /// <summary>The runtime attached to the game, if gameplay has started.</summary>
        internal SuperMetroidRuntime? RuntimeForVerification => PrivateState.Field<SuperMetroidRuntime?>(game, "runtime");
        /// <summary>Remaining hold frames before the attract demo advances.</summary>
        internal int AttractDemoHoldFramesRemaining => PrivateState.Field<int>(game, "demoHoldFramesRemaining");
        /// <summary>Whether the current runtime permits grounded Samus movement.</summary>
        internal bool GameplayMovementEnabled => game.RuntimeForVerification?.GroundedSamusMovementEnabled ?? false;
        /// <summary>The game's cartridge audio state.</summary>
        internal CartridgeAudioState AudioForVerification => PrivateState.Field<CartridgeAudioState>(game, "audio");
        /// <summary>Current menu NMI frame counter.</summary>
        internal ushort MenuNmiFrameCounterForVerification => PrivateState.Field<ushort>(game, "menuNmiFrameCounter");
        /// <summary>Ceres destruction cinematic state, when that sequence is active.</summary>
        internal CeresDestructionCinematicState? CeresDestructionForVerification =>
            PrivateState.Field<CeresDestructionCinematicState?>(game, "ceresDestruction");

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
            runtime.Controller1.Latch(runtime.ControllerBindings.Normalize(controllerInput));
        }
        /// <summary>Current Samus pose, or zero when no runtime Samus exists.</summary>
        internal byte GameplaySamusPose => game.RuntimeForVerification?.Samus?.Pose ?? 0;
        /// <summary>Current Samus X position, or zero when unavailable.</summary>
        internal ushort GameplaySamusX => game.RuntimeForVerification?.Samus?.XPosition ?? 0;
        /// <summary>Current Samus Y position, or zero when unavailable.</summary>
        internal ushort GameplaySamusY => game.RuntimeForVerification?.Samus?.YPosition ?? 0;
    }

    /// <summary>Exposes flashback actors and hit counts from the intro sequence.</summary>
    extension(IntroCinematicState intro)
    {
        /// <summary>Number of active projectiles in the intro flashback simulation.</summary>
        internal ushort ActiveFlashbackProjectileCount =>
            PrivateState.Field<SamusProjectileSystem>(intro, "flashbackProjectiles").ProjectileCounter;
        /// <summary>Flashback Samus X position, or zero when the actor is absent.</summary>
        internal ushort FlashbackSamusX => PrivateState.Field<SamusState?>(intro, "flashbackSamus")?.XPosition ?? 0;
        /// <summary>Flashback Samus Y position, or zero when the actor is absent.</summary>
        internal ushort FlashbackSamusY => PrivateState.Field<SamusState?>(intro, "flashbackSamus")?.YPosition ?? 0;
        /// <summary>Accumulated hits on the flashback Mother Brain actor.</summary>
        internal ushort MotherBrainHitCount =>
            PrivateState.Field<IntroMotherBrainSpriteState?>(intro, "flashbackMotherBrain")?.HitCount ?? 0;
    }

    /// <summary>Exposes active ending and credits timing state.</summary>
    extension(EndingCreditsState ending)
    {
        /// <summary>Current ending brightness register value.</summary>
        internal byte Brightness => PrivateState.Field<byte>(ending, "brightness");
        /// <summary>Current frame of the ending cinematic.</summary>
        internal ushort CinematicFrame => PrivateState.Field<ushort>(ending, "cinematicFrame");
        /// <summary>Credits object's vertical scroll, or zero before it exists.</summary>
        internal ushort CreditsVerticalScroll => PrivateState.Field<CreditsObjectState?>(ending, "credits")?.VerticalScroll ?? 0;
    }

    /// <summary>Exposes pause menu scroll and rendered tile state.</summary>
    extension(PauseMenuState pause)
    {
        /// <summary>Last recorded horizontal origin of a pause menu indicator.</summary>
        internal ushort LastIndicatorOriginX => PrivateState.Field<ushort>(pause, "lastIndicatorOriginX");
        /// <summary>Last recorded vertical origin of a pause menu indicator.</summary>
        internal ushort LastIndicatorOriginY => PrivateState.Field<ushort>(pause, "lastIndicatorOriginY");
        /// <summary>True while an L/R page switch is fading out, loading or fading in.</summary>
        internal bool PageTransitionActive => PrivateState.Field<PauseMenuTransition>(pause, "transition") != PauseMenuTransition.None;
        /// <summary>Current horizontal scroll of the pause map.</summary>
        internal ushort MapHorizontalScroll => PrivateState.Field<ushort>(pause, "mapHorizontalScroll");
        /// <summary>Current vertical scroll of the pause map.</summary>
        internal ushort MapVerticalScroll => PrivateState.Field<ushort>(pause, "mapVerticalScroll");

        /// <summary>Reads the displayed BG1 tile word at map coordinates.</summary>
        internal MapTileWord ReadDisplayedMapTile(int mapX, int mapY)
        {
            var vram = PrivateState.Field<SnesVram>(pause, "vram");
            int byteAddress = (PauseMenuLayout.Bg1TilemapWord + AreaMapLayout.GetTilemapWordIndex(mapX, mapY)) * 2;
            return new MapTileWord(unchecked((ushort)(vram.ReadByte(byteAddress) | (vram.ReadByte(byteAddress + 1) << 8))));
        }
    }

    /// <summary>Exposes the current shooting-star entries in the ending presentation.</summary>
    extension(EndingShootingStars shootingStars)
    {
        /// <summary>Read-only view of shooting star entries in stored order.</summary>
        internal ReadOnlySpan<EndingShootingStar> Stars => PrivateState.Field<EndingShootingStar[]>(shootingStars, "stars");
    }

    /// <summary>Exposes sample counts used to verify extracted audio catalog contents.</summary>
    extension(ExtractedAudioAssetCatalog catalog)
    {
        /// <summary>Number of canonical samples retained by the extracted catalog.</summary>
        internal int CanonicalSampleCount => PrivateState.Field<int>(catalog, "canonicalSampleCount");
        /// <summary>Total samples across the catalog's source mappings.</summary>
        internal int SourceMappingCount => PrivateState.Field<IReadOnlyDictionary<int, ManagedPcmSampleBank>>(catalog, "sampleBanks")
            .Values.Sum(bank => bank.Samples.Count);
    }

    /// <summary>Provides pixel-copy behavior for frontend frame checks.</summary>
    extension(FrontendFrame frame)
    {
        /// <summary>Returns a frame with an independent copy of its pixel buffer.</summary>
        internal FrontendFrame WithCopiedPixels() => frame with { Pixels = frame.Pixels.ToArray() };
    }

    /// <summary>Exposes whether a captured frame used the compatibility raster path.</summary>
    extension(CapturedFrontendFrame frame)
    {
        /// <summary>True when the frame has no captured rendering snapshot.</summary>
        internal bool UsedLegacyRaster => frame.Snapshot is null;
    }

    /// <summary>Exposes initial palette data for catalog installation checks.</summary>
    extension(GameplayBasePaletteCatalog palettes)
    {
        /// <summary>Initial color words in catalog order.</summary>
        internal ReadOnlySpan<ushort> Initial => PrivateState.Field<ushort[]>(palettes, "initial");
    }

    /// <summary>Exposes stored HUD tile words for rendering checks.</summary>
    extension(HudState hud)
    {
        /// <summary>Current HUD tile data in storage order.</summary>
        internal ReadOnlySpan<ushort> Tiles => PrivateState.Field<ushort[]>(hud, "_tiles");
    }

    /// <summary>Exposes encoded flip bits from a map tile word.</summary>
    extension(MapTileWord tile)
    {
        /// <summary>Horizontal and vertical flip flags encoded in the map tile word.</summary>
        internal SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(tile.Raw & 0xc000);
    }

    /// <summary>Exposes the packed OAM transfer buffer for upload verification.</summary>
    extension(OamBuffer oam)
    {
        /// <summary>Combines low and high OAM tables in their upload byte layout.</summary>
        internal byte[] CreateUploadPayload()
        {
            var payload = new byte[OamBuffer.UploadByteCount];
            PrivateState.Field<byte[]>(oam, "_lowTable").CopyTo(payload, 0);
            PrivateState.Field<byte[]>(oam, "_highTable").CopyTo(payload, OamBuffer.LowTableByteCount);
            return payload;
        }
    }

    /// <summary>Exposes reset behavior for rendering publication measurements.</summary>
    extension(RenderPublicationProfile profile)
    {
        /// <summary>Verifies ownership and clears the accumulated profile counters.</summary>
        internal void Reset()
        {
            PrivateState.Invoke(profile, "VerifyOwner");
            PrivateState.SetProperty(profile, "Ticks", 0L);
            PrivateState.SetProperty(profile, "AllocatedBytes", 0L);
            PrivateState.SetProperty(profile, "Publications", 0L);
        }
    }

    /// <summary>Exposes decoded BG2 background entries for room data checks.</summary>
    extension(RoomLevelData level)
    {
        /// <summary>Read-only background tile words loaded for this room level.</summary>
        internal ReadOnlyMemory<ushort> BackgroundEntries => PrivateState.Field<ushort[]>(level, "_backgroundEntries");
    }

    /// <summary>Exposes active palette effect slot counts.</summary>
    extension(RoomPaletteFxSystem fx)
    {
        /// <summary>Number of FX slots whose effect identifier is nonzero.</summary>
        internal int ActiveCount => PrivateState.Field<Array>(fx, "slots").Cast<object>()
            .Count(slot => PrivateState.Property<ushort>(slot, "Id") != 0);
    }

    /// <summary>Exposes the emulated work RAM backing store.</summary>
    extension(SuperMetroidAddressSpace bus)
    {
        /// <summary>Mutable span over the address space's work RAM bytes.</summary>
        internal Span<byte> WorkRam => PrivateState.Field<byte[]>(bus, "_workRam");
    }

    /// <summary>Exposes queued VRAM writes for transfer-order checks.</summary>
    extension(VramWriteQueue queue)
    {
        /// <summary>Queued VRAM write entries in insertion order.</summary>
        internal IReadOnlyList<VramWriteEntry> Entries => PrivateState.Field<List<VramWriteEntry>>(queue, "_entries");
    }

    /// <summary>Provides a focused entry point for accepted grapple firing behavior.</summary>
    extension(SamusGrappleMovement)
    {
        /// <summary>Accepted grapple firing with its connection pose applied immediately, as ordinary firing does.</summary>
        internal static GrappleMovementResult ConnectAcceptedFiring(ISnesAddressSpace bus, SamusState samus,
            SamusGrappleState grapple, ushort previousXPosition, ushort previousYPosition,
            bool validateAnchorBlock = true, bool validateAnchorEnemy = false) =>
            (GrappleMovementResult)PrivateState.InvokeStatic(typeof(SamusGrappleMovement), "ConnectAcceptedFiringCore",
                bus, samus, grapple, previousXPosition, previousYPosition, validateAnchorBlock, validateAnchorEnemy, false)!;
    }

    /// <summary>Exposes enemy collision and selected enemy state for verification.</summary>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>Kraid's mouth hitbox test: the production collision-scratch load followed by its mouth overlap.</summary>
        internal bool KraidMouthHitboxOverlapsShot(RoomEnemySlot body, ushort hitboxPointer, SamusProjectileSlot shot)
        {
            object scratch = PrivateState.Invoke(enemies, "LoadKraidCollisionScratch", body, hitboxPointer)!;
            object collision = PrivateState.Construct(PrivateState.Nested(typeof(RoomEnemySystem), "KraidCollisionShot"),
                shot.XPosition, shot.YPosition, shot.XRadius, shot.YRadius, shot.Type, shot.Damage);
            return (bool)PrivateState.Invoke(scratch, "OverlapsMouth", collision)!;
        }

        /// <summary>Pending PLM requests emitted by Chozo statues.</summary>
        internal IReadOnlyList<ChozoStatuePlmRequest> ChozoStatuePlmRequests =>
            PrivateState.Field<List<ChozoStatuePlmRequest>>(enemies, "_chozoStatuePlmRequests");
        /// <summary>Current state of each tracked Chozo statue.</summary>
        internal IReadOnlyList<ChozoStatueState?> ChozoStatueStates =>
            PrivateState.Field<ChozoStatueState?[]>(enemies, "_chozoStatueStates");
        /// <summary>Current state of each tracked ninja space pirate.</summary>
        internal IReadOnlyList<NinjaSpacePirateEnemyState?> NinjaSpacePirateStates =>
            PrivateState.Field<NinjaSpacePirateEnemyState?[]>(enemies, "_ninjaSpacePirateStates");
    }

    /// <summary>Exposes the positions and activity of the Ceres arrival projectiles.</summary>
    extension(CeresElevatorArrivalState arrival)
    {
        /// <summary>Current vertical position of the moving arrival pad.</summary>
        internal ushort PadYPosition => PrivateState.Property<ushort>(PrivateState.Field<object>(arrival, "pad"), "YPosition");
        /// <summary>Current vertical position of the arrival platform.</summary>
        internal ushort PlatformYPosition => PrivateState.Property<ushort>(PrivateState.Field<object>(arrival, "platform"), "YPosition");
        /// <summary>True while the moving pad occupies native enemy-projectile slot $22.</summary>
        internal bool PadActive => PrivateState.Property<bool>(PrivateState.Field<object>(arrival, "pad"), "Active");
        /// <summary>True while the level-data concealer occupies native enemy-projectile slot $20.</summary>
        internal bool PlatformActive => PrivateState.Property<bool>(PrivateState.Field<object>(arrival, "platform"), "Active");
        /// <summary>Shared X word of both projectiles, copied from Samus by $86:A313.</summary>
        internal ushort XPosition => PrivateState.Property<ushort>(PrivateState.Field<object>(arrival, "pad"), "XPosition");
    }

    /// <summary>Exposes Samus load-appearance timing from the active runtime.</summary>
    extension(SuperMetroidRuntime runtime)
    {
        /// <summary>Number of frames remaining in Samus's load appearance sequence.</summary>
        internal ushort SamusLoadAppearanceFramesRemaining => PrivateState.Field<ushort>(runtime, "_samusLoadAppearanceFramesRemaining");
        /// <summary>True while the load appearance sequence has frames remaining.</summary>
        internal bool SamusLoadAppearanceActive => runtime.SamusLoadAppearanceFramesRemaining != 0;
    }

    /// <summary>Exposes active PLM slot and station snapshots for room interaction checks.</summary>
    extension(RoomPlmSystem plms)
    {

        /// <summary>The one active slot running <paramref name="headerPointer"/>; throws unless exactly one does.</summary>
        internal RoomPlmSlotSnapshot SinglePopulationSlot(ushort headerPointer)
        {
            RoomPlmSlotSnapshot[] matches = plms.PopulationSlots
                .Where(slot => slot.HeaderPointer == headerPointer).ToArray();
            return matches.Length switch
            {
                1 => matches[0],
                0 => throw new InvalidOperationException($"No active PLM uses header ${headerPointer:X4}."),
                _ => throw new InvalidOperationException($"More than one active PLM uses header ${headerPointer:X4}."),
            };
        }

        /// <summary>Active station PLMs, highest native slot first.</summary>
        internal IReadOnlyList<StationPlmSnapshot> Stations
        {
            get
            {
                bool lockedOut = PrivateState.Field<bool>(plms, "_saveStationLockedOut");
                return PlmSlots(plms)
                    .Select((slot, index) => (slot, index))
                    .Where(entry => PrivateState.Property<bool>(entry.slot, "Active")
                        && PrivateState.Property<object?>(entry.slot, "Station") is not null)
                    .OrderByDescending(entry => entry.index)
                    .Select(entry =>
                    {
                        object station = PrivateState.Property<object>(entry.slot, "Station");
                        return new StationPlmSnapshot(entry.index,
                            PrivateState.Property<int>(entry.slot, "BlockIndex"),
                            PrivateState.Property<ushort>(entry.slot, "RoomArgument"),
                            PrivateState.Property<StationKind>(station, "Kind"),
                            PrivateState.Property<bool>(station, "Triggered"),
                            PrivateState.Property<SaveStationPhase>(station, "SavePhase"),
                            lockedOut);
                    })
                    .ToArray();
            }
        }
    }

    /// <summary>Enumerates the underlying PLM slots in native slot order.</summary>
    private static IEnumerable<object> PlmSlots(RoomPlmSystem plms) =>
        PrivateState.Field<Array>(plms, "_slots").Cast<object>();

    /// <summary>Every room-FX record the compiled catalog defines, in pointer order.</summary>
    internal static IEnumerable<RoomFxRecordDefinition> AllRoomFxRecords()
    {
        for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
            if (PrivateState.InvokeStatic(typeof(RoomFxRecordDefinitions), "SelectRecord", (ushort)pointer) is RoomFxRecordDefinition record)
                yield return record;
    }
}

/// <summary>One active station PLM as a verification snapshot.</summary>
/// <param name="NativeSlotIndex">Native PLM slot containing this station.</param>
/// <param name="BlockIndex">Room block associated with the station.</param>
/// <param name="RoomArgument">Argument word supplied by the room's PLM record.</param>
/// <param name="Kind">Station behavior represented by the PLM.</param>
/// <param name="Triggered">Whether Samus has activated the station.</param>
/// <param name="SavePhase">Current save-station interaction phase.</param>
/// <param name="SaveStationLockedOut">Whether save activation is currently blocked.</param>
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
            $"{PrivateState.Field<byte>(self, "_musicReadPosition"):X}/{PrivateState.Field<byte>(self, "_musicWritePosition"):X} " +
            $"t={PrivateState.Field<ushort>(self, "_musicTimer"):X} d=" +
            string.Join(",", PrivateState.Field<Array>(self, "_musicDelays").Cast<object>()
                .Select(delay => PrivateState.Property<object>(delay, "Frames")).Select(frames => $"{frames:X}"));

        /// <summary>One entry of an SFX library's queue ($0643+).</summary>
        internal byte SoundQueueEntryForVerification(int queue, int index) =>
            PrivateState.Field<byte[,]>(self, "_soundQueues")[queue, index];

        /// <summary>Queue start/next indices and dispatcher state of one SFX library ($0643+/$0646+/$0649+).</summary>
        internal (byte Start, byte Next, byte State) SoundQueueForVerification(int queue) =>
            (PrivateState.Field<byte[]>(self, "_soundReadPositions")[queue], PrivateState.Field<byte[]>(self, "_soundWritePositions")[queue],
             PrivateState.Field<byte[]>(self, "_soundStates")[queue]);
    }
}
