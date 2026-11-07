using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Ceres door tiles, color rows, and Mode 7 transfer frames.</summary>
internal static class EnemyTileArtworkCeresDoorVerification
{
    /// <summary>Compares native VRAM and CGRAM with the installed door art.</summary>
    internal static void VerifyInstalledCeresDoorVisuals(ISnesAddressSpace bus, InstalledEnemyArtwork stock)
    {
        CeresDoorSourceArtwork door = stock.Areas.CeresDoor
            ?? throw new InvalidDataException("Installed Ceres-door visuals are missing.");
        byte[] nativeTiles = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CeresSourceDefinitions.Door.Characters, CeresDoorVisualFormat.CharacterBytes);
        CeresDoorVisuals visual = door.Visuals;
        AssertTrue(door.Characters.Span.SequenceEqual(nativeTiles),
            "installed Ceres-door PNG preserves the complete direct tile DMA");
        // $A6:F739 copies the characters to VRAM byte $E000, word $7000: OBJ character $100.
        EncounterArtworkUpload overlay = EncounterLookUploads.Uploads(ComposedEncounter.CeresDoor,
            EncounterLookUploads.IndexOf(ComposedEncounter.CeresDoor, CeresDoorLookDefinitions.OverlayLook.Name), new EnemyTileTransfers(stock)).Single();
        AssertEqual(SpriteArtworkPlacements.TableByte(VramDmaDestination.FromNative(0x7000)), overlay.FirstByte,
            "the door's characters show where the cartridge's DMA lands them among the OBJ characters");
        AssertTrue(overlay.Characters.Span.SequenceEqual(nativeTiles), "the door's overlay look shows all its characters");

        // The installed colours, which the import pre-colours the doors' and elevator room's shades from (#1512), are
        // the cartridge's words colour for colour.
        for (int color = 0; color < CeresDoorVisualDefinitions.SetupColorCount; color++)
        {
            AssertEqual(Native(bus, CeresSourceDefinitions.Door.NormalColors + color * sizeof(ushort)), visual.NormalColor(color),
                $"installed Ceres-door normal colour {color} matches the cartridge");
            AssertEqual(Native(bus, CeresSourceDefinitions.Door.EscapeColors + color * sizeof(ushort)), visual.EscapeColor(color),
                $"installed Ceres-door escape colour {color} matches the cartridge");
        }
        for (int row = 0; row < CeresDoorVisualDefinitions.AnimationStepCount; row++)
        {
            for (int color = 0; color < CeresDoorVisualDefinitions.AnimationColorCount; color++)
                AssertEqual(Native(bus, CeresSourceDefinitions.Door.AnimationColors + row * CeresSourceDefinitions.Door.AnimationRowBytes + color * sizeof(ushort)),
                    visual.AnimationColor(row, color), $"installed Ceres-door animated row {row} colour {color} matches the cartridge");
        }
        // The platform's strips are pictures of the room's floor (#1467); the import pictures each from the native transfer.
        CeresFloorAnimationFrame[] strips = [CeresFloorAnimationFrame.ElevatorLight, CeresFloorAnimationFrame.ElevatorDark];
        const int stripCells = 4, stripFirstCell = 12 * 128 + 14;
        for (int frame = 0; frame < strips.Length; frame++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                EnemyRomTablePointers.Ceres.DoorTransferPointers + frame * sizeof(ushort));
            int record = 0xa60000 | pointer;
            AssertEqual((byte)0x80, CpuBusRegionRead.Read(bus, record),
                $"Ceres-door Mode-7 transfer {frame} uses the native low-byte DMA");
            int source = (int)new SnesAddress(CpuBusRegionRead.Read(bus, record + 3), LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, record + 1), CpuBusRegionRead.Read(bus, record + 2)));
            AssertEqual(frame == 0 ? CeresSourceDefinitionsDoorConstants.PlatformFrames :
                    CeresSourceDefinitionsDoorConstants.PlatformFrames + CeresSourceDefinitionsDoorConstants.PlatformFrameBytes,
                source, $"Ceres-door Mode-7 transfer {frame} source");
            AssertEqual(stripCells,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 4),
                $"Ceres-door Mode-7 transfer {frame} length");
            AssertEqual(stripFirstCell,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), record + 6),
                $"Ceres-door Mode-7 transfer {frame} destination");
            AssertEqual((byte)0, CpuBusRegionRead.Read(bus, record + 8),
                $"Ceres-door Mode-7 transfer {frame} increment mode");
            AssertEqual((byte)0, CpuBusRegionRead.Read(bus, record + 9),
                $"Ceres-door Mode-7 transfer {frame} terminator");
            byte[] nativeFrame = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), source, stripCells);
            IReadOnlyDictionary<int, int> pictured = CeresFloorDrawings.Cells(strips[frame]);
            AssertTrue(nativeFrame.Select((tile, cell) => pictured[stripFirstCell + cell] == tile).All(same => same),
                $"the import pictures Ceres-door floor strip {frame} from the native transfer");
        }
    }

    /// <summary>The colour the cartridge's colour word at <paramref name="address"/> holds: five bits each of red, green and blue.</summary>
    private static RgbColor Native(ISnesAddressSpace bus, int address)
    {
        int word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
        return new RgbColor(word & 31, word >> 5 & 31, word >> 10 & 31);
    }
}
