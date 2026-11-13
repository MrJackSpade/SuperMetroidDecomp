using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Areas.Ceres.CeresSteamCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireTongueCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedFrameArtworkCommonEmptyVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedFrameArtworkSporeSpawnSelectorsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Enemies.MaridiaSnails.MaridiaLargeSnailCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Core.World;

using SuperMetroid.AssetExtraction.Bosses.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
/// <summary>Checks installed extended enemy OAM compositions and guarded visual reads against native data.</summary>
internal static class EnemyExtendedFrameArtworkVerification
{
    /// <summary>
    /// Collects the Draygon presentation selectors that do not name an installed OAM frame and
    /// checks each such native frame holds a BG2 command component.
    /// </summary>
    private static HashSet<ushort> VerifyNativeDraygonBg2FrameSelectors(
        ISnesAddressSpace rom, HashSet<ushort> draygonOamPointers)
    {
        var draygonBg2Pointers = new HashSet<ushort>();
        // The Draygon oracle matches each typed frame to the operand of its native list frame.
        for (int index = 0; index < DraygonFrameToolingDefinitions.FrameCount; index++)
        {
            ushort pointer = DraygonFrameToolingDefinitions.Spritemap(new DraygonFrame(index));
            if (draygonOamPointers.Contains(pointer))
                continue;
            draygonBg2Pointers.Add(pointer);
            int count = rom.ReadCartridgeByte(0xa50000 | pointer);
            AssertTrue(Enumerable.Range(0, count).Any(component =>
            {
                ushort record = unchecked((ushort)(pointer + 2 + component * 8 + 4));
                ushort sprite = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa50000 | record);
                return CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa50000 | sprite)
                    == 0xfffe;
            }), $"unextracted Draygon frame $A5:{pointer:X4} is a BG2 command");
        }
        return draygonBg2Pointers;
    }

    /// <summary>Compares installed Pirate and boss extended frames to native OAM.</summary>
    internal static void VerifyInstalledEnemyExtendedFrames(
        CartridgeImportAddressSpace rom,
        ImportEnemyArtwork stock)
    {
        AssertTrue(stock.Enemies.ExtendedFrames is not null,
            "installed enemy catalog contains extended visual compositions");
        var guard = NewExtendedVisualReadGuard(rom, out Action<byte, ushort> guardBlockFrame);
        foreach (EnemyFrameName frame in EnemyExtendedFrameDefinitions.Frames)
        {
            guardBlockFrame(frame.Bank, frame.Pointer);
            AssertTrue((stock.Enemies.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGet(frame.Bank, frame.Pointer, out _),
                $"installed extended frame {frame.Name} exists");
        }
        ushort emptyPointer = EnemyB2ExtendedSpritemaps.EmptyExtendedSpritemap;
        guardBlockFrame(EnemyExtendedFrameDefinitions.Bank, emptyPointer);
        AssertTrue((stock.Enemies.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGet(EnemyExtendedFrameDefinitions.Bank,
                emptyPointer, out ReadOnlyMemory<EnemyExtendedDrawComponent> empty) &&
                   empty.IsEmpty,
            "walking Pirate initial empty frame is a compiled draw identity");
        OamBuffer nativeEmpty = DrawReferenceExtendedFrame(rom, EnemyExtendedFrameDefinitions.Bank, emptyPointer,
            new(0x0040, 0x0080));
        OamBuffer installedEmpty = DrawExtended(stock, emptyPointer,
            new(0x0040, 0x0080));
        AssertTrue(nativeEmpty.ImageEquals(installedEmpty) &&
                   nativeEmpty.NextByteOffset == installedEmpty.NextByteOffset,
            "walking Pirate common empty frame draws without ROM reads");
        Suite(nameof(VerifySharedEmptyExtendedFrames), () => VerifySharedEmptyExtendedFrames(rom, stock));
        Suite(nameof(VerifyInstalledCeresSteamInstructionFrames), () => VerifyInstalledCeresSteamInstructionFrames(stock));
        Suite(nameof(VerifyInstalledOumVisualSelectors), () => VerifyInstalledOumVisualSelectors(stock.Enemies));
        Suite(nameof(VerifyInstalledCrocomireTongueVisualSelectors), () => VerifyInstalledCrocomireTongueVisualSelectors(stock));
        AssertEqual(EnemyExtendedFrameDefinitions.ExpectedFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Length,
            "walking/wall Pirate distinct extended-frame count");
        AssertEqual(EnemyExtendedFrameDefinitions.WallFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("wall_pirate_", StringComparison.Ordinal)),
            "all wall-Pirate visual frame identities are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.NinjaFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("ninja_pirate_", StringComparison.Ordinal)),
            "all ninja-Pirate visual frame identities are installed");
        var differing = new List<string>();
        foreach (EnemyFrameName frame in EnemyExtendedFrameDefinitions.Frames)
        {
            foreach ((ushort x, ushort y) in new VerifyInstalledEnemyExtendedFramesCase[]
                     {
                         new VerifyInstalledEnemyExtendedFramesCase(0x0040, 0x0080),
                         new VerifyInstalledEnemyExtendedFramesCase(0x0000, 0x0000),
                     })
            {
                RoomEnemySlot? drawn = null;
                int drawnRow = 0;
                int drawnBase = 0;
                OamBuffer installed = DrawExtendedForBank(stock, frame.Bank, frame.Pointer, new(x, y), (slot, system) =>
                {
                    drawn = slot;
                    drawnRow = slot.PaletteRowIn(system);
                    drawnBase = slot.NativeTileBaseIn(system);
                });
                OamBuffer native = DrawReferenceExtendedFrame(rom,
                    frame.Bank, frame.Pointer, new(x, y),
                    NativeSpriteGraphicsWords.PaletteBits(drawn is null ? throw new InvalidOperationException("Verification requires the drawn slot.") : drawnRow),
                    checked((ushort)drawnBase));
                // Mother Brain's initial body frame, drawn only while her body is hidden, reads only the head artwork her
                // room loads, so it shows nothing (#1513), though the cartridge writes its records.
                bool shownBlank = frame.Name == "mother_brain_body_oam_initial_dummy" && installed.NextByteOffset == 0;
                if (!shownBlank && !installed.StandsFor(native, apartFromPriority: true))
                    differing.Add($"{frame.Name} at {x:X4},{y:X4}: native {Records(native)}, installed {Records(installed)}");
            }
        }
        AssertTrue(differing.Count == 0,
            $"every installed extended frame matches native OAM; {differing.Count} differ (x, y, tile, attributes):\n{string.Join("\n", differing)}");
        // The used records of an OAM low table, for a failure message.
        static string Records(OamBuffer oam) => string.Join(" ",
            Enumerable.Range(0, oam.NextByteOffset / 4).Select(record =>
                $"({oam.LowTable[record * 4]:X2},{oam.LowTable[(record * 4) + 1]:X2},{oam.LowTable[(record * 4) + 2]:X2},{oam.LowTable[(record * 4) + 3]:X2})"));
        Console.WriteLine("  Extended enemy OAM: every installed composition matches native component offsets and packed sprite bytes apart from the priority tier (#1514) at both fixture origins; the sprite coordinate range's clipping at the wrap edge is not reproduced (#1520).");
        Suite(nameof(VerifyInstalledSporeSpawnSelectorPrograms), () => VerifyInstalledSporeSpawnSelectorPrograms(rom, stock));
        AssertEqual(BossSpriteFrameDefinitions.RidleyFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("ridley_body_", StringComparison.Ordinal)),
            "all Ceres/Lower Norfair Ridley body frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.CeresSteamFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("ceres_steam_", StringComparison.Ordinal)),
            "all selected Ceres steam visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.OumFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("oum_", StringComparison.Ordinal)),
            "all selected Oum visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.CrocomireSpriteFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("crocomire_tongue_", StringComparison.Ordinal)),
            "all selected Crocomire tongue visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.CrocomireBodyFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("crocomire_body_", StringComparison.Ordinal)),
            "all selected Crocomire fight-body OAM frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.KraidArmFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("kraid_arm_", StringComparison.Ordinal)),
            "all Kraid arm visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoAwakeningFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_awake_", StringComparison.Ordinal)),
            "all Golden Torizo awakening visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoWalkingFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_walk_left_", StringComparison.Ordinal)),
            "all Golden Torizo walking-left visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoRightwardFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_rightward_", StringComparison.Ordinal)),
            "all Golden Torizo turning/walking-right visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.TorizoJumpBackFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("torizo_jump_back_", StringComparison.Ordinal) &&
                    !frame.Name.StartsWith("torizo_jump_back_left_", StringComparison.Ordinal)),
            "all shared Torizo jump-back visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.TorizoJumpBackLeftNewFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("torizo_jump_back_left_", StringComparison.Ordinal)),
            "both new left-facing Torizo jump-back visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoRightOrbFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_right_orb_", StringComparison.Ordinal)),
            "all Golden Torizo right-orb visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoLeftOrbFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_left_orb_", StringComparison.Ordinal)),
            "all Golden Torizo left-orb visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoRightSonicFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_right_sonic_", StringComparison.Ordinal)),
            "all Golden Torizo right-sonic visual frames are installed");
        AssertEqual(EnemyExtendedFrameDefinitions.TorizoFallingLeftFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("torizo_falling_left", StringComparison.Ordinal)),
            "shared Torizo falling-left visual frame is installed");
        AssertEqual(EnemyExtendedFrameDefinitions.GoldenTorizoLeftFootOrbFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("golden_torizo_left_foot_orb_", StringComparison.Ordinal)),
            "all new Golden Torizo left-foot orb visual frames are installed");
        AssertEqual(BossSpriteFrameDefinitions.DraygonSpriteFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("draygon_", StringComparison.Ordinal) && !DraygonBodyLayerFrameDefinitions.Frames.Contains(frame.Frame)),
            "all selected ordinary-OAM Draygon extended frames are installed");
        AssertEqual(BossSpriteFrameDefinitions.SporeSpawnSpriteFrameCount,
            EnemyExtendedFrameDefinitions.Frames.Count(
                frame => frame.Name.StartsWith("spore_spawn_", StringComparison.Ordinal)),
            "all selected Spore Spawn extended frames are installed");
        var draygonOamPointers = EnemyExtendedFrameDefinitions.Frames
            .Where(frame => frame.Name.StartsWith("draygon_", StringComparison.Ordinal) && !DraygonBodyLayerFrameDefinitions.Frames.Contains(frame.Frame))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        HashSet<ushort> draygonBg2Pointers = VerifyNativeDraygonBg2FrameSelectors(rom, draygonOamPointers);
        AssertEqual(34, draygonBg2Pointers.Count,
            "Draygon presentation selectors partition into 48 OAM and 34 BG2 frames");

        Console.WriteLine(
            "Extended enemy art: 131 Pirate, 11 Ridley, 48 Draygon, 12 Spore Spawn, 28 Ceres steam, 30 Oum, 9 Crocomire, 22 Kraid arm, 6 Golden Torizo awakening, 10 walking-left, 11 rightward, 3 shared right jump-back, 2 new left jump-back, 6 right-orb, 12 left-orb, 21 right-sonic, one falling-left and five left-foot orb OAM frames match native OAM " +
            "at three origins with visual ROM reads forbidden.");
    }
    /// <summary>Walks native extended spritemap components for an independent OAM reference, including clipping and packed sprite parts.</summary>
    /// <remarks>BG2 command streams are checked by their dedicated fixture.</remarks>
    internal static OamBuffer DrawReferenceExtendedFrame(
        ISnesAddressSpace bus, byte bank, ushort pointer, Position16<ScreenPixels> origin, ushort paletteBits = 0, ushort baseTileIndex = 0)
    {
        byte ReadByte(int address) => bus.ReadCartridgeByte(((int)new SnesAddress(bank, checked((ushort)(SnesAddressMath.OffsetInBank(address))))));
        ushort ReadWord(int address) => LittleEndianBytes.Word(ReadByte(address), ReadByte(address + 1));
        var oam = new OamBuffer();
        int count = ReadByte(pointer); // The high header byte is draw metadata.
        for (int index = 0; index < count; index++)
        {
            int component = pointer + 2 + index * 8;
            ushort sprite = ReadWord(component + 4);
            if (ReadWord(sprite) == 0xfffe)
                continue;
            ushort componentX = unchecked((ushort)(origin.X + ReadWord(component)));
            ushort componentY = unchecked((ushort)(origin.Y + ReadWord(component + 2)));
            if (((componentX + 128) & 0xfe00) != 0 ||
                ((componentY + 128) & 0xfe00) != 0)
                continue;
            // The cartridge's extended clip parks wrapped parts at Y=$F0; #1308 drops that presentation quirk, so the
            // reference keeps the parts at their wrapped coordinates as the whole-frame draws do.
            DrawImportedEnemySpritemap(bus, oam, new NativeEnemySpritemapDraw { Bank = bank, Pointer = sprite, Origin = new(componentX, componentY), PaletteBits = paletteBits, BaseTileIndex = baseTileIndex });
        }
        return oam;
    }

    /// <summary>Draws a bank-$B2 installed frame through the production enemy-layer path for OAM comparison.</summary>
    private static OamBuffer DrawExtended(ImportEnemyArtwork art,
        ushort pointer, Position16<RoomPixels> position,
        Action<RoomEnemySlot, RoomEnemySystem>? inspect = null)
    {
        return DrawExtendedForBank(art, EnemyExtendedFrameDefinitions.Bank,
            pointer, position, inspect);
    }

    /// <summary>Installs a chosen extended frame on a fixture enemy and captures the production layer draw for its bank.</summary>
    internal static OamBuffer DrawExtendedForBank(ImportEnemyArtwork? art,
        byte bank, ushort pointer, Position16<RoomPixels> position,
        Action<RoomEnemySlot, RoomEnemySystem>? inspect = null)
    {
        var enemies = new RoomEnemySystem((art is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(art.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = art?.Bosses };
        // A boss frame names its body frame through the installed body sheets (#1466), as a room loading the boss binds them.
        enemies.BossArtworkBinding.BodySheets = BossBodySheetFixtures.Stock.Sheets;
        enemies.Drawing.Enqueue(enemies.SlotTable.Slots[0], reached: true);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        EnemyFrameId? frame = RoomEnemySlotFrameAccess.TryFrameAt(bank, pointer);
        slot.Bind(ExtendedFrameShower(frame), slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        if (art is not null)
        {
            // The enemy draws as a room drawing it loads it: at its tile base and in its palette there.
            enemies.DrawUnder(SpawnedObjectFixtures.SetDrawing(slot.DefinitionId));
            slot.Shading = PictureShading.Standard;
        }
        // Ridley's scene draws his tail and wings before his body while his movement animation runs, and his AI latches
        // whether his body draws in the hurt flash; this fixture's animation is off and its latch holds his own colours.
        if (slot.DefinitionId == EnemyDefinitionId.Ridley)
            enemies.RidleyEncounter.Ridley = new RidleyEnemyState();
        slot.CollidesByFrameHitboxes = true;
        slot.Frame = frame;
        if (frame is { } shown)
            SpawnedObjectFixtures.ShowDrawnLooks(enemies, SuperMetroid.AssetExtraction.Enemies.FrameDrawings.Of(shown));
        slot.FrameHold = WrappingCountdown.Start(7);
        slot.Position = slot.Position.WithWhole(position);
        var oam = new OamBuffer();
        enemies.Drawing.DrawAsOam(oam, new(0, 0));
        inspect?.Invoke(slot, enemies);
        return oam;
    }

    /// <summary>
    /// The enemy a room shows <paramref name="frame"/> through: one a room draws whose behaviour shows it, other than Ridley,
    /// whose scene adds his tail and wings to his body; a frame only he shows, or one no enemy shows, as a bank's shared empty
    /// frame, draws through his sheet, with his scene's tail and wings left off.
    /// </summary>
    private static EnemyDefinitionId ExtendedFrameShower(EnemyFrameId? frame) =>
        EnemyDefinitionFrameDefinitions.Frames
            .Where(entry => frame is { } shown && entry.Value.Contains(shown) && SpawnedObjectFixtures.Drawn(entry.Key) &&
                entry.Key is not (EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.RidleyExplosion))
            .Select(entry => entry.Key).Order()
            .Cast<EnemyDefinitionId?>().FirstOrDefault() ?? EnemyDefinitionId.Ridley;

    /// <summary>Creates a read guard whose returned callback adds native extended frames and their sprite parts to the forbidden set.</summary>
    internal static CartridgeReadGuard NewExtendedVisualReadGuard(
        ISnesAddressSpace source, out Action<byte, ushort> blockFrame)
    {
        var blocked = new HashSet<int>();
        blockFrame = (bank, pointer) => BlockExtendedVisualFrame(blocked, source, bank, pointer);
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed extended enemy draw read visual ROM byte ${address:X6}.");
    }

    /// <summary>Blocks a frame header, its OAM components, sprite records, and any bounded BG2 command stream from runtime ROM reads.</summary>
    private static void BlockExtendedVisualFrame(
        HashSet<int> blocked, ISnesAddressSpace source, byte bank, ushort pointer)
    {
        int root = ((int)new SnesAddress(bank, pointer));
        int count = source.ReadCartridgeByte(root);
        Block(bank, pointer, 2 + count * 8);
        for (int index = 0; index < count; index++)
        {
            ushort component = unchecked((ushort)(pointer + 2 + index * 8));
            ushort sprite = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(component + 4)))));
            int spriteAddress = ((int)new SnesAddress(bank, sprite));
            int parts = LittleEndianBytes.Word(source.ReadCartridgeByte(spriteAddress), source.ReadCartridgeByte((int)new SnesAddress(bank, unchecked((ushort)(sprite + 1)))));
            if (parts == EnemyBg2FrameLayoutConstants.StreamMarker)
            {
                Block(bank, sprite, 2);
                ushort cursor = unchecked((ushort)(sprite + 2));
                bool terminated = false;
                for (int command = 0;
                     command < EnemyBg2FrameLayout.MaximumCommandsPerStream;
                     command++)
                {
                    ushort destination = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, cursor)));
                    if (destination == 0xffff)
                    {
                        Block(bank, cursor, 2);
                        terminated = true;
                        break;
                    }
                    ushort wordCount = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(((int)new SnesAddress(bank, unchecked((ushort)(cursor + 2)))));
                    int length = 4 + wordCount * 2;
                    Block(bank, cursor, length);
                    cursor = unchecked((ushort)(cursor + length));
                }
                if (!terminated)
                    throw new InvalidDataException(
                        $"Extended BG2 stream ${bank:X2}:{sprite:X4} has no terminator.");
                continue;
            }
            Block(bank, sprite, 2 + parts * 5);
        }

        void Block(byte blockBank, ushort blockPointer, int length)
        {
            for (int index = 0; index < length; index++)
                blocked.Add(((int)new SnesAddress(blockBank, unchecked((ushort)(blockPointer + index)))));
        }
    }
}
