using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using Audio = SuperMetroid.Core.Audio;

/// <summary>
/// Cartridge-backed regression for the shared Sidehopper/Dessgeega state machine. Blue
/// Hopper is deliberately used because its retail population contains only two Tourian
/// hoppers; unrelated untranslated actors therefore cannot make this family appear healthy.
/// </summary>
internal static class HopperAudit
{
    private static class RoomDefinitions
    {
        /// <summary>Room <c>$01/$02</c> header at <c>$8F:9B9D</c>.</summary>
        public const ushort CrateriaCeilingSidehopper = 0x9b9d;

        /// <summary>Ceiling Sidehopper's first landed spritemap at <c>$A3:AF34</c>.</summary>
        public const ushort CeilingSidehopperLandedSpritemap = 0xaf34;

        /// <summary>Floor Sidehopper frame mirrored by <c>$A3:AF34</c>.</summary>
        public const ushort FloorSidehopperLandedSpritemap = 0xaee3;

        /// <summary>Upper Norfair room <c>$02/$04</c> header at <c>$8F:A815</c>.</summary>
        public const ushort UpperNorfairDessgeegaRoom = 0xa815;

        /// <summary>Small Dessgeega enemy definition at <c>$A0:D97F</c>.</summary>
        public const ushort SmallDessgeegaDefinition = 0xd97f;
    }

    private const ushort BlueHopperRoomHeader = 0xdc19;
    private const ushort BlueHopperDefaultState = 0xdc2b;
    private const ushort TourianSidehopperDefinition = 0xd9ff;

    public static int Run(string romPath, string? outputDirectory = null)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, BlueHopperRoomHeader);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        if (room.State.Pointer != BlueHopperDefaultState || enemies.EnemyCount != 2 ||
            enemies.Slots.Take(2).Any(
                slot => slot.EnemyDefinitionPointer != TourianSidehopperDefinition) ||
            enemies.HopperStates.Take(2).Any(state => state is null))
        {
            throw new InvalidDataException(
                $"Blue Hopper selected state ${room.State.Pointer:X4} with " +
                $"{enemies.EnemyCount} non-uniform or uninitialized actors.");
        }

        // Blue Hopper contains one parameter-$8000 ceiling actor followed by one floor
        // actor. Exercise the latter here so the long-standing upward-hop assertions retain
        // their original meaning; room $01/$02 below covers the reported ceiling variant.
        RoomEnemySlot auditedSlot = enemies.Slots[1];
        HopperEnemyState auditedState = enemies.HopperStates[1]
            ?? throw new InvalidDataException("Blue Hopper slot one has no typed hopper state.");
        RoomEnemySlot ceilingSlot = enemies.Slots[0];
        HopperEnemyState ceilingState = enemies.HopperStates[0]
            ?? throw new InvalidDataException("Blue Hopper slot zero has no typed hopper state.");
        if (auditedSlot.XPosition != 0x0086 || auditedSlot.YPosition != 0x00a9 ||
            auditedSlot.Health != 1500 || auditedSlot.Definition.Damage != 120 ||
            auditedState.UpsideDown || auditedState.HopTableIndex != 2 ||
            auditedState.VariantTableOffset != 2 ||
            auditedState.Function != HopperEnemyFunction.ChooseHopSize ||
            auditedState.InstalledInstructionList != 0xb0d1)
        {
            throw new InvalidDataException(
                "Tourian hopper disagrees with its retail population/header/list data: " +
                $"position=(${auditedSlot.XPosition:X4},${auditedSlot.YPosition:X4}), " +
                $"health/damage={auditedSlot.Health}/{auditedSlot.Definition.Damage}, " +
                $"upsideDown={auditedState.UpsideDown}, physics={auditedState.HopTableIndex}, " +
                $"variant={auditedState.VariantTableOffset}, " +
                $"function=$A3:{(ushort)auditedState.Function:X4}, " +
                $"list=$A3:{auditedState.InstalledInstructionList:X4}.");
        }
        if (!ceilingState.UpsideDown)
            throw new InvalidDataException("Blue Hopper slot zero did not select ceiling behavior.");

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0080,
            YPosition = 0x0080,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        VerifyBlueHopperFloorOrientation(
            bus,
            enemies,
            assets,
            vram,
            cgram,
            auditedSlot,
            auditedState,
            outputDirectory);

        ushort startX = auditedSlot.XPosition;
        ushort startY = auditedSlot.YPosition;
        ushort minimumY = startY;
        var functions = new HashSet<HopperEnemyFunction>();
        var maps = new HashSet<ushort>();
        var sounds = new HashSet<ushort>();
        bool sawFalling = false;
        bool sawReturnToLandedWait = false;
        for (int frame = 0; frame < 600; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            VerifyLiveMountingFlags(bus, enemies, ceilingSlot, auditedSlot, frame);
            functions.Add(auditedState.Function);
            maps.Add(auditedSlot.SpritemapPointer);
            minimumY = Math.Min(minimumY, auditedSlot.YPosition);
            sawFalling |= auditedState.Falling;
            sawReturnToLandedWait |= frame > 20 &&
                auditedState.Function == HopperEnemyFunction.WaitToHop;
            if (enemies.LastHopperSoundEffect is ushort sound)
                sounds.Add(sound);
        }

        if (!functions.Contains(HopperEnemyFunction.PrepareSmallHop) ||
            !functions.Contains(HopperEnemyFunction.PrepareBigHop) ||
            !functions.Contains(HopperEnemyFunction.JumpingUpsideUpBackward) ||
            !sawFalling || !sawReturnToLandedWait || minimumY >= startY ||
            auditedSlot.XPosition == startX || maps.Count < 3 ||
            !sounds.SetEquals([0x005d, 0x005e]))
        {
            throw new InvalidDataException(
                $"Tourian hopper cycle failed: functions={string.Join(',', functions.Select(x => $"${(ushort)x:X4}"))}, " +
                $"falling={sawFalling}, landed={sawReturnToLandedWait}, " +
                $"position=({startX},{startY})->({auditedSlot.XPosition},{auditedSlot.YPosition}), " +
                $"minY={minimumY}, maps={maps.Count}, sounds={string.Join(',', sounds.Select(x => $"${x:X2}"))}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Blue Hopper's live ROM spritemap emitted no enemy OBJ.");

        // The Tourian definition has its own restrictive projectile vulnerability table,
        // but touch remains the common $A0:8023 path and must apply the header's 120 damage.
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        samus.XPosition = auditedSlot.XPosition;
        samus.YPosition = auditedSlot.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 879 ||
            !samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Tourian hopper common contact failed: health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}.");
        }

        VerifyRoom0102CeilingOrientation(bus, outputDirectory);
        VerifyUpperNorfairSmallDessgeegaCycle(bus, outputDirectory);

        Console.WriteLine(
            "Blue Hopper audit passed: two retail Tourian Sidehoppers loaded, both random " +
            $"hop sizes traversed the ROM quadratic arc and landing loop, {maps.Count} maps " +
            $"animated, 120 contact damage resolved, and {oam.LastFinalizedSpriteCount} OBJ pieces rendered.");
        return 0;
    }

    /// <summary>
    /// Captures every distinct live Small Dessgeega spritemap from the exact Upper Norfair
    /// retail room reported in issue #248. This diagnostic deliberately observes both
    /// population mountings through the real instruction interpreter and enemy OAM path.
    /// </summary>
    private static void VerifyUpperNorfairSmallDessgeegaCycle(
        SuperMetroidAddressSpace bus,
        string? outputDirectory)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            RoomDefinitions.UpperNorfairDessgeegaRoom);
        if (room.Identity != new RoomIdentity(AreaId.Norfair, 0x04))
            throw new InvalidDataException($"Dessgeega audit selected {room.Identity}, expected $02/$04.");

        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        RoomEnemySlot[] dessgeegas = enemies.Slots
            .Where(slot => slot.EnemyDefinitionPointer == RoomDefinitions.SmallDessgeegaDefinition)
            .ToArray();
        if (!dessgeegas.Any(slot => slot.Parameter1 == 0) ||
            !dessgeegas.Any(slot => slot.Parameter1 != 0))
        {
            throw new InvalidDataException(
                $"Room $02/$04 exposes {dessgeegas.Length} Small Dessgeegas without both mountings.");
        }

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0180,
            YPosition = 0x0380,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var seenMaps = new HashSet<(bool Ceiling, ushort Spritemap)>();
        var floorFrames = new List<Rgba32[]>();
        var ceilingFrames = new List<Rgba32[]>();
        var floorFunctions = new HashSet<HopperEnemyFunction>();
        var ceilingFunctions = new HashSet<HopperEnemyFunction>();

        for (int frame = 0; frame < 600; frame++)
        {
            enemies.StepFrame(0x0100, 0x0300, false, samus, level: assets.LevelData);
            foreach (RoomEnemySlot slot in dessgeegas)
            {
                HopperEnemyState state = enemies.HopperStates[slot.SlotIndex]
                    ?? throw new InvalidDataException(
                        $"Small Dessgeega slot {slot.SlotIndex} has no typed hopper state.");
                (state.UpsideDown ? ceilingFunctions : floorFunctions).Add(state.Function);
                if (slot.SpritemapPointer == 0x804d)
                    continue;
                if (!seenMaps.Add((state.UpsideDown, slot.SpritemapPointer)))
                    continue;

                var oam = new OamBuffer();
                oam.BeginFrame();
                oam.AddEnemySpritemap(
                    bus,
                    slot.Definition.Bank,
                    slot.SpritemapPointer,
                    originX: 128,
                    originY: 112,
                    slot.PaletteIndex,
                    slot.VramTilesIndex);
                oam.FinalizeFrame();
                OamEntry[] pieces = Enumerable.Range(0, oam.LastFinalizedSpriteCount)
                    .Select(oam.GetEntry)
                    .ToArray();
                if (pieces.Length != 5 ||
                    pieces.Any(piece => piece.FlipY != state.UpsideDown))
                {
                    throw new InvalidDataException(
                        $"Small Dessgeega {(state.UpsideDown ? "ceiling" : "floor")} " +
                        $"map $A3:{slot.SpritemapPointer:X4} emitted {pieces.Length} pieces " +
                        $"with flipY=[{string.Join(',', pieces.Select(piece => piece.FlipY))}].");
                }
                Rgba32[] pixels = SnesObjRenderer.Render(oam, vram, cgram, obsel: 0x03);
                (state.UpsideDown ? ceilingFrames : floorFrames).Add(pixels);
                Console.WriteLine(
                    $"Small Dessgeega {(state.UpsideDown ? "ceiling" : "floor")} frame {frame}: " +
                    $"map $A3:{slot.SpritemapPointer:X4}, " +
                    $"flipY=[{string.Join(',', pieces.Select(piece => piece.FlipY))}].");

                if (outputDirectory is not null)
                {
                    Directory.CreateDirectory(outputDirectory);
                    PngWriter.WriteRgba(
                        Path.Combine(
                            outputDirectory,
                            $"small-dessgeega-{(state.UpsideDown ? "ceiling" : "floor")}-{slot.SpritemapPointer:X4}.png"),
                        256,
                        224,
                        pixels);
                }
            }
        }

        HopperEnemyFunction[] requiredFloorFunctions =
        [
            HopperEnemyFunction.WaitToHop,
            HopperEnemyFunction.JumpingUpsideUpForward,
            HopperEnemyFunction.Landed,
        ];
        HopperEnemyFunction[] requiredCeilingFunctions =
        [
            HopperEnemyFunction.WaitToHop,
            HopperEnemyFunction.JumpingUpsideDownBackward,
            HopperEnemyFunction.Landed,
        ];
        if (floorFrames.Count != 3 || ceilingFrames.Count != 3 ||
            requiredFloorFunctions.Any(function => !floorFunctions.Contains(function)) ||
            requiredCeilingFunctions.Any(function => !ceilingFunctions.Contains(function)))
        {
            throw new InvalidDataException(
                $"Small Dessgeega full cycle mismatch: maps={floorFrames.Count}/{ceilingFrames.Count}, " +
                $"floor-functions=[{string.Join(',', floorFunctions)}], " +
                $"ceiling-functions=[{string.Join(',', ceilingFunctions)}].");
        }
        for (int animationFrame = 0; animationFrame < floorFrames.Count; animationFrame++)
        {
            VerifyExactVerticalMirror(
                floorFrames[animationFrame],
                ceilingFrames[animationFrame],
                originY: 112,
                $"Small Dessgeega animation frame {animationFrame}");
        }

        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            RoomDefinitions.UpperNorfairDessgeegaRoom,
            cameraX: 0x0100,
            cameraY: 0x0300);
        if (runtime.Samus is not null)
        {
            runtime.Samus.XPosition = 0x0180;
            runtime.Samus.YPosition = 0x0380;
        }
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        for (int frame = 0; frame < 40; frame++)
            runtime.StepFrame(controller1Input: 0);
        foreach (RoomEnemySlot slot in runtime.Enemies.Slots.Where(slot =>
                     slot.EnemyDefinitionPointer == RoomDefinitions.SmallDessgeegaDefinition &&
                     slot.SpritemapPointer != 0x804d))
        {
            Rgba32[] reference = RenderIsolatedHopper(
                bus, vram, cgram, slot, slot.SpritemapPointer, 128, 112);
            Rgba32[] integrated = RenderIsolatedHopper(
                bus, runtime.Vram, runtime.Cgram, slot, slot.SpritemapPointer, 128, 112);
            if (!reference.AsSpan().SequenceEqual(integrated))
            {
                throw new InvalidDataException(
                    $"Integrated room load changed Small Dessgeega map $A3:{slot.SpritemapPointer:X4} " +
                    "graphics or palette data.");
            }
        }
        if (outputDirectory is not null)
        {
            PngWriter.WriteRgba(
                Path.Combine(outputDirectory, "small-dessgeega-integrated-room-02-04.png"),
                256,
                224,
                SuperMetroidRuntimeFrameRenderer.Render(runtime));
        }
    }

    private static void VerifyExactVerticalMirror(
        ReadOnlySpan<Rgba32> floor,
        ReadOnlySpan<Rgba32> ceiling,
        int originY,
        string description)
    {
        for (int y = 0; y < FrontendFrame.Height; y++)
        {
            int mirroredY = originY * 2 - 1 - y;
            for (int x = 0; x < FrontendFrame.Width; x++)
            {
                Rgba32 expected = mirroredY is >= 0 and < FrontendFrame.Height
                    ? floor[mirroredY * FrontendFrame.Width + x]
                    : default;
                Rgba32 actual = ceiling[y * FrontendFrame.Width + x];
                if (actual != expected)
                {
                    throw new InvalidDataException(
                        $"{description} ceiling pixel ({x},{y}) is {actual}; " +
                        $"floor mirror ({x},{mirroredY}) is {expected}.");
                }
            }
        }
    }

    /// <summary>
    /// Verifies all cartridge animation frames, not only the initial landed pose. The two
    /// Blue Hopper actors are adjacent in native population/OAM order, allowing their
    /// exact piece ranges to be isolated without guessing from screen coordinates.
    /// </summary>
    private static void VerifyLiveMountingFlags(
        SuperMetroidAddressSpace bus,
        RoomEnemySystem enemies,
        RoomEnemySlot ceilingHopper,
        RoomEnemySlot floorHopper,
        int frame)
    {
        int ceilingPieceCount = ReadSpritemapPieceCount(bus, ceilingHopper);
        int floorPieceCount = ReadSpritemapPieceCount(bus, floorHopper);
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();

        OamEntry[] pieces = Enumerable.Range(0, oam.LastFinalizedSpriteCount)
            .Select(oam.GetEntry)
            .ToArray();
        if (pieces.Length != ceilingPieceCount + floorPieceCount)
        {
            throw new InvalidDataException(
                $"Blue Hopper frame {frame} emitted {pieces.Length} pieces; cartridge maps " +
                $"require {ceilingPieceCount}+{floorPieceCount}.");
        }

        if (pieces.Take(ceilingPieceCount).Any(piece => !piece.FlipY) ||
            pieces.TakeLast(floorPieceCount).Any(piece => piece.FlipY))
        {
            throw new InvalidDataException(
                $"Blue Hopper frame {frame} mixed mounting orientation: ceiling=" +
                $"[{string.Join(',', pieces.Take(ceilingPieceCount).Select(piece => piece.FlipY))}], " +
                $"floor=[{string.Join(',', pieces.TakeLast(floorPieceCount).Select(piece => piece.FlipY))}].");
        }
    }

    private static int ReadSpritemapPieceCount(
        SuperMetroidAddressSpace bus,
        RoomEnemySlot slot) =>
        bus.ReadByte((slot.Definition.Bank << 16) | slot.SpritemapPointer) |
        (bus.ReadByte(
            (slot.Definition.Bank << 16) |
            unchecked((ushort)(slot.SpritemapPointer + 1))) << 8);

    /// <summary>
    /// Replays a player's complete desktop recording and captures the first genuinely
    /// visible floor- and ceiling-mounted hopper through the integrated frontend renderer.
    /// This is intentionally separate from <see cref="Run"/>: the cartridge-room fixture
    /// isolates enemy OAM, while this path proves that room loading, camera subtraction,
    /// staged OAM, and final PPU composition do not invert the same actor afterward.
    /// </summary>
    public static int RunRecording(
        string recordingPath,
        string romPath,
        string outputDirectory)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        string fullRomPath = Path.GetFullPath(romPath);
        byte[] digest;
        using (FileStream rom = File.OpenRead(fullRomPath))
            digest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(digest, recording.RomSha256))
            throw new InvalidDataException("Hopper replay ROM SHA-256 does not match the recording.");

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions);
        var apuPortEchoes = new byte[4];
        bool capturedFloor = false;
        bool capturedCeiling = false;
        Directory.CreateDirectory(outputDirectory);

        for (int frameIndex = 0; frameIndex < recording.ControllerInputs.Length; frameIndex++)
        {
            FrontendFrame frame = game.Step(recording.ControllerInputs[frameIndex]);
            foreach (Audio.CartridgeAudioCommand command in frame.AudioCommands)
            {
                if (command.Kind == Audio.CartridgeAudioCommandKind.WritePort)
                    apuPortEchoes[command.Port] = command.Value;
            }
            game.SetAudioAcknowledgements(new Audio.CartridgeAudioAcknowledgements(
                apuPortEchoes[0],
                apuPortEchoes[1],
                apuPortEchoes[2],
                apuPortEchoes[3]));

            if (game.RuntimeForVerification is not { ActiveRoom: { } room, Camera: { } camera } runtime)
                continue;

            foreach (HopperEnemyState state in runtime.Enemies.HopperStates.OfType<HopperEnemyState>())
            {
                RoomEnemySlot slot = runtime.Enemies.Slots.Single(
                    candidate => ReferenceEquals(runtime.Enemies.HopperStates[candidate.SlotIndex], state));
                int screenX = unchecked((short)(slot.XPosition - camera.XPosition));
                int screenY = unchecked((short)(slot.YPosition - camera.YPosition));
                // $804D is the common empty enemy spritemap. Waiting for the actor origin
                // to enter the visible field also avoids recording a room-transition frame
                // whose off-screen object has not yet begun its cartridge animation.
                if (slot.SpritemapPointer is 0 or 0x804d ||
                    screenX is < 48 or > FrontendFrame.Width - 48 ||
                    screenY is < 48 or > FrontendFrame.Height - 48)
                {
                    continue;
                }

                bool alreadyCaptured = state.UpsideDown ? capturedCeiling : capturedFloor;
                if (alreadyCaptured)
                    continue;

                string mounting = state.UpsideDown ? "ceiling" : "floor";
                string capturePath = Path.Combine(
                    outputDirectory,
                    $"recording-{mounting}-room-{(byte)room.AreaIndex:X2}-{room.RoomIndex:X2}-frame-{frameIndex}.png");
                PngWriter.WriteRgba(capturePath, FrontendFrame.Width, FrontendFrame.Height, frame.Pixels);
                Console.WriteLine(
                    $"Captured {mounting} hopper at recording frame {frameIndex}, " +
                    $"room ${(byte)room.AreaIndex:X2}/${room.RoomIndex:X2}, map " +
                    $"$A3:{slot.SpritemapPointer:X4}, world (${slot.XPosition:X4},${slot.YPosition:X4}), " +
                    $"screen ({screenX},{screenY}).");
                if (state.UpsideDown)
                    capturedCeiling = true;
                else
                    capturedFloor = true;
            }

            if (capturedFloor && capturedCeiling)
                break;
        }

        if (!capturedFloor || !capturedCeiling)
        {
            throw new InvalidDataException(
                $"Recording exposed floor={capturedFloor}, ceiling={capturedCeiling}; " +
                "both live mounting variants are required for the integrated visual audit.");
        }

        Console.WriteLine("Integrated hopper recording audit captured both mounting variants.");
        return 0;
    }

    /// <summary>
    /// Reproduces issue #248 against the actual floor actor in Blue Hopper. The ceiling
    /// and floor actors share one draw queue, so use the floor actor's live cartridge
    /// spritemap count and native population order to isolate its trailing OAM records.
    /// This asserts the pixels' vertical-flip attributes rather than merely trusting the
    /// already-correct typed <see cref="HopperEnemyState.UpsideDown"/> selector.
    /// </summary>
    private static void VerifyBlueHopperFloorOrientation(
        SuperMetroidAddressSpace bus,
        RoomEnemySystem enemies,
        CartridgeRoomAssets assets,
        SnesVram vram,
        SnesCgram cgram,
        RoomEnemySlot floorHopper,
        HopperEnemyState floorState,
        string? outputDirectory)
    {
        enemies.StepFrame(0, 0, false, samus: null, level: assets.LevelData);
        int pieceCount = bus.ReadByte(0xa30000 | floorHopper.SpritemapPointer) |
            (bus.ReadByte(0xa30000 | unchecked((ushort)(floorHopper.SpritemapPointer + 1))) << 8);
        if (pieceCount <= 0)
        {
            throw new InvalidDataException(
                $"Blue Hopper floor map $A3:{floorHopper.SpritemapPointer:X4} has no OBJ pieces.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (outputDirectory is not null)
        {
            Directory.CreateDirectory(outputDirectory);
            PngWriter.WriteRgba(
                Path.Combine(outputDirectory, "blue-hopper-both-variants.png"),
                256,
                224,
                SnesObjRenderer.Render(oam, vram, cgram, obsel: 0x03));
        }
        OamEntry[] floorPieces = Enumerable.Range(0, oam.LastFinalizedSpriteCount)
            .Select(oam.GetEntry)
            .TakeLast(pieceCount)
            .ToArray();
        if (floorState.UpsideDown || floorPieces.Length != pieceCount ||
            floorPieces.Any(piece => piece.FlipY))
        {
            throw new InvalidDataException(
                $"Blue Hopper floor actor emitted {floorPieces.Length}/{pieceCount} OBJ pieces " +
                $"with vertical flips [{string.Join(',', floorPieces.Select(piece => piece.FlipY))}].");
        }
    }

    private static void VerifyRoom0102CeilingOrientation(
        SuperMetroidAddressSpace bus,
        string? outputDirectory)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            RoomDefinitions.CrateriaCeilingSidehopper);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        RoomEnemySlot ceilingHopper = enemies.Slots.Single(
            slot => slot.EnemyDefinitionPointer == RoomEnemySystem.SidehopperDefinition);
        HopperEnemyState state = enemies.HopperStates[ceilingHopper.SlotIndex]
            ?? throw new InvalidDataException("Room $01/$02 Sidehopper has no typed hopper state.");
        if (ceilingHopper.Parameter1 != 1 || !state.UpsideDown)
        {
            throw new InvalidDataException(
                $"Room $01/$02 ceiling selector mismatch: parameter1=${ceilingHopper.Parameter1:X4}, " +
                $"upsideDown={state.UpsideDown}.");
        }

        const ushort cameraX = 0x0180;
        enemies.StepFrame(cameraX, 0, false, samus: null, level: assets.LevelData);
        if (ceilingHopper.SpritemapPointer != RoomDefinitions.CeilingSidehopperLandedSpritemap)
        {
            throw new InvalidDataException(
                $"Room $01/$02 ceiling Sidehopper selected map $A3:{ceilingHopper.SpritemapPointer:X4}, " +
                $"not cartridge ceiling map $A3:{RoomDefinitions.CeilingSidehopperLandedSpritemap:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, cameraX, 0, 0, 7);
        oam.FinalizeFrame();
        if (outputDirectory is not null)
        {
            Directory.CreateDirectory(outputDirectory);
            PngWriter.WriteRgba(
                Path.Combine(outputDirectory, "room-01-02-ceiling-hopper.png"),
                256,
                224,
                SnesObjRenderer.Render(oam, vram, cgram, obsel: 0x03));
        }
        OamEntry[] pieces = Enumerable.Range(0, oam.LastFinalizedSpriteCount)
            .Select(oam.GetEntry)
            .ToArray();
        OamEntry[] hopperPieces = pieces.Take(5).ToArray();
        if (hopperPieces.Length != 5 || hopperPieces.Any(piece => !piece.FlipY))
        {
            throw new InvalidDataException(
                $"Room $01/$02 ceiling Sidehopper emitted {hopperPieces.Length} leading OBJ " +
                $"pieces with vertical flips [{string.Join(',', hopperPieces.Select(piece => piece.FlipY))}].");
        }

        VerifyRenderedMountingPair(bus, vram, cgram, ceilingHopper);
    }

    /// <summary>
    /// Renders the retail floor and ceiling landed maps at a shared origin and proves the
    /// final RGBA rasters are exact vertical mirrors around the native half-pixel axis.
    /// This catches tile-row ordering bugs in <see cref="SnesObjRenderer"/> that raw OAM
    /// flip-bit assertions alone cannot see.
    /// </summary>
    private static void VerifyRenderedMountingPair(
        SuperMetroidAddressSpace bus,
        SnesVram vram,
        SnesCgram cgram,
        RoomEnemySlot hopper)
    {
        const ushort originX = 128;
        const ushort originY = 112;
        Rgba32[] floor = RenderIsolatedHopper(
            bus,
            vram,
            cgram,
            hopper,
            RoomDefinitions.FloorSidehopperLandedSpritemap,
            originX,
            originY);
        Rgba32[] ceiling = RenderIsolatedHopper(
            bus,
            vram,
            cgram,
            hopper,
            RoomDefinitions.CeilingSidehopperLandedSpritemap,
            originX,
            originY);

        for (int y = 0; y < FrontendFrame.Height; y++)
        {
            int mirroredY = originY * 2 - 1 - y;
            for (int x = 0; x < FrontendFrame.Width; x++)
            {
                Rgba32 expected = mirroredY is >= 0 and < FrontendFrame.Height
                    ? floor[mirroredY * FrontendFrame.Width + x]
                    : default;
                Rgba32 actual = ceiling[y * FrontendFrame.Width + x];
                if (actual != expected)
                {
                    throw new InvalidDataException(
                        $"Rendered ceiling hopper pixel ({x},{y}) is {actual}; " +
                        $"floor mirror ({x},{mirroredY}) is {expected}.");
                }
            }
        }
    }

    private static Rgba32[] RenderIsolatedHopper(
        SuperMetroidAddressSpace bus,
        SnesVram vram,
        SnesCgram cgram,
        RoomEnemySlot hopper,
        ushort spritemap,
        ushort originX,
        ushort originY)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        oam.AddEnemySpritemap(
            bus,
            hopper.Definition.Bank,
            spritemap,
            originX,
            originY,
            hopper.PaletteIndex,
            hopper.VramTilesIndex);
        oam.FinalizeFrame();
        return SnesObjRenderer.Render(oam, vram, cgram, obsel: 0x03);
    }
}
