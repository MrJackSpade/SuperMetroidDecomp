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
    extension(SuperMetroidGame game)
    {
        internal SuperMetroidRuntime? RuntimeForVerification => PrivateState.Field<SuperMetroidRuntime?>(game, "runtime");
        internal int AttractDemoHoldFramesRemaining => PrivateState.Field<int>(game, "demoHoldFramesRemaining");
        internal bool GameplayMovementEnabled => game.RuntimeForVerification?.GroundedSamusMovementEnabled ?? false;
        internal byte GameplaySamusPose => game.RuntimeForVerification?.Samus?.Pose ?? 0;
        internal ushort GameplaySamusX => game.RuntimeForVerification?.Samus?.XPosition ?? 0;
        internal ushort GameplaySamusY => game.RuntimeForVerification?.Samus?.YPosition ?? 0;
    }

    extension(IntroCinematicState intro)
    {
        internal ushort ActiveFlashbackProjectileCount =>
            PrivateState.Field<SamusProjectileSystem>(intro, "flashbackProjectiles").ProjectileCounter;
        internal ushort FlashbackSamusX => PrivateState.Field<SamusState?>(intro, "flashbackSamus")?.XPosition ?? 0;
        internal ushort FlashbackSamusY => PrivateState.Field<SamusState?>(intro, "flashbackSamus")?.YPosition ?? 0;
        internal ushort MotherBrainHitCount =>
            PrivateState.Field<IntroMotherBrainSpriteState?>(intro, "flashbackMotherBrain")?.HitCount ?? 0;
    }

    extension(EndingCreditsState ending)
    {
        internal byte Brightness => PrivateState.Field<byte>(ending, "brightness");
        internal ushort CinematicFrame => PrivateState.Field<ushort>(ending, "cinematicFrame");
        internal ushort CreditsVerticalScroll => PrivateState.Field<CreditsObjectState?>(ending, "credits")?.VerticalScroll ?? 0;
    }

    extension(PauseMenuState pause)
    {
        internal ushort LastIndicatorOriginX => PrivateState.Field<ushort>(pause, "lastIndicatorOriginX");
        internal ushort LastIndicatorOriginY => PrivateState.Field<ushort>(pause, "lastIndicatorOriginY");
        internal ushort MapHorizontalScroll => PrivateState.Field<ushort>(pause, "mapHorizontalScroll");
        internal ushort MapVerticalScroll => PrivateState.Field<ushort>(pause, "mapVerticalScroll");

        internal MapTileWord ReadDisplayedMapTile(int mapX, int mapY)
        {
            var vram = PrivateState.Field<SnesVram>(pause, "vram");
            int byteAddress = (PauseMenuLayout.Bg1TilemapWord + AreaMapLayout.GetTilemapWordIndex(mapX, mapY)) * 2;
            return new MapTileWord(unchecked((ushort)(vram.ReadByte(byteAddress) | (vram.ReadByte(byteAddress + 1) << 8))));
        }
    }

    extension(EndingShootingStars shootingStars)
    {
        internal ReadOnlySpan<EndingShootingStar> Stars => PrivateState.Field<EndingShootingStar[]>(shootingStars, "stars");
    }

    extension(ExtractedAudioAssetCatalog catalog)
    {
        internal int CanonicalSampleCount => PrivateState.Field<int>(catalog, "canonicalSampleCount");
        internal int SourceMappingCount => PrivateState.Field<IReadOnlyDictionary<int, ManagedPcmSampleBank>>(catalog, "sampleBanks")
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
        internal ReadOnlySpan<ushort> Initial => PrivateState.Field<ushort[]>(palettes, "initial");
    }

    extension(HudState hud)
    {
        internal ReadOnlySpan<ushort> Tiles => PrivateState.Field<ushort[]>(hud, "_tiles");
    }

    extension(MapTileWord tile)
    {
        internal SnesTileFlipFlags FlipFlags => (SnesTileFlipFlags)(tile.Raw & 0xc000);
    }

    extension(OamBuffer oam)
    {
        internal byte[] CreateUploadPayload()
        {
            var payload = new byte[OamBuffer.UploadByteCount];
            PrivateState.Field<byte[]>(oam, "_lowTable").CopyTo(payload, 0);
            PrivateState.Field<byte[]>(oam, "_highTable").CopyTo(payload, OamBuffer.LowTableByteCount);
            return payload;
        }
    }

    extension(RenderPublicationProfile profile)
    {
        internal void Reset()
        {
            PrivateState.Invoke(profile, "VerifyOwner");
            PrivateState.SetProperty(profile, "Ticks", 0L);
            PrivateState.SetProperty(profile, "AllocatedBytes", 0L);
            PrivateState.SetProperty(profile, "Publications", 0L);
        }
    }

    extension(RoomLevelData level)
    {
        internal ReadOnlyMemory<ushort> BackgroundEntries => PrivateState.Field<ushort[]>(level, "_backgroundEntries");
    }

    extension(RoomPaletteFxSystem fx)
    {
        internal int ActiveCount => PrivateState.Field<Array>(fx, "slots").Cast<object>()
            .Count(slot => PrivateState.Property<ushort>(slot, "Id") != 0);
    }

    extension(SuperMetroidAddressSpace bus)
    {
        internal Span<byte> WorkRam => PrivateState.Field<byte[]>(bus, "_workRam");
    }

    extension(VramWriteQueue queue)
    {
        internal IReadOnlyList<VramWriteEntry> Entries => PrivateState.Field<List<VramWriteEntry>>(queue, "_entries");
    }

    extension(SamusGrappleMovement)
    {
        /// <summary>Accepted grapple firing with its connection pose applied immediately, as ordinary firing does.</summary>
        internal static GrappleMovementResult ConnectAcceptedFiring(ISnesAddressSpace bus, SamusState samus,
            SamusGrappleState grapple, ushort previousXPosition, ushort previousYPosition,
            bool validateAnchorBlock = true, bool validateAnchorEnemy = false) =>
            (GrappleMovementResult)PrivateState.InvokeStatic(typeof(SamusGrappleMovement), "ConnectAcceptedFiringCore",
                bus, samus, grapple, previousXPosition, previousYPosition, validateAnchorBlock, validateAnchorEnemy, false)!;
    }

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

        internal IReadOnlyList<ChozoStatuePlmRequest> ChozoStatuePlmRequests =>
            PrivateState.Field<List<ChozoStatuePlmRequest>>(enemies, "_chozoStatuePlmRequests");
        internal IReadOnlyList<ChozoStatueState?> ChozoStatueStates =>
            PrivateState.Field<ChozoStatueState?[]>(enemies, "_chozoStatueStates");
        internal IReadOnlyList<NinjaSpacePirateEnemyState?> NinjaSpacePirateStates =>
            PrivateState.Field<NinjaSpacePirateEnemyState?[]>(enemies, "_ninjaSpacePirateStates");
    }

    extension(CeresElevatorArrivalState arrival)
    {
        internal ushort PadYPosition => PrivateState.Property<ushort>(PrivateState.Field<object>(arrival, "pad"), "YPosition");
        internal ushort PlatformYPosition => PrivateState.Property<ushort>(PrivateState.Field<object>(arrival, "platform"), "YPosition");
    }

    extension(SuperMetroidRuntime runtime)
    {
        internal ushort SamusLoadAppearanceFramesRemaining => PrivateState.Field<ushort>(runtime, "_samusLoadAppearanceFramesRemaining");
        internal bool SamusLoadAppearanceActive => runtime.SamusLoadAppearanceFramesRemaining != 0;
    }

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
internal readonly record struct StationPlmSnapshot(
    int NativeSlotIndex,
    int BlockIndex,
    ushort RoomArgument,
    StationKind Kind,
    bool Triggered,
    SaveStationPhase SavePhase,
    bool SaveStationLockedOut);
