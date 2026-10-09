using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies the installed Ridley wing and tail tables and compares their production OAM with cartridge-rendered reference sprites.</summary>
    /// <param name="rom">Cartridge address space supplying the pinned ROM tables and reference sprite data.</param>
    /// <param name="stock">Artwork catalog installed into the production enemy system.</param>
    private static void VerifyInstalledRidleySupplementalArtwork(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        AssertEqual(20, RidleySupplementalVisualDefinitions.WingPointerCount,
            "Ridley wing table retains twenty animation selections");
        AssertEqual(16, RidleySupplementalVisualDefinitions.TailTipPointerCount,
            "Ridley tail tip retains sixteen direction selections");
        for (int index = 0; index < RidleySupplementalVisualDefinitions.WingPointerCount;
             index++)
        {
            ushort native = ReadRidleySupplementWord(rom,
                EnemyRomTablePointers.Ceres.WingSpritemapPointers + index * 2);
            AssertEqual(native, RidleySupplementalVisualDefinitions.WingFrameAt(index),
                $"Ridley wing animation table entry {index} matches the pinned ROM");

            ushort facing = index < 10 ? (ushort)0 : (ushort)2;
            ushort wingFrame = (ushort)(index % 10);
            OamBuffer actual = DrawInstalledRidleySupplement(
                stock, rom, facing, wingFrame, tailDirection: null);
            var expected = new OamBuffer();
            DrawImportedEnemySpritemap(rom, expected, RidleySupplementalVisualDefinitions.Bank,
                native, 200, 100, 0x0200, 0,
                clipVerticalWrap: true, originYIsOnScreen: true);
            AssertRidleySupplementOam(expected, actual,
                $"Ridley wing selection {index} uses installed OAM without table or frame reads");
        }

        for (int direction = 0;
             direction < RidleySupplementalVisualDefinitions.TailTipPointerCount;
             direction++)
        {
            ushort nativeTip = ReadRidleySupplementWord(rom,
                EnemyRomTablePointers.Ceres.TailTipSpritemapPointers + direction * 2);
            AssertEqual(nativeTip, RidleySupplementalVisualDefinitions.TailTipFrameAt(direction),
                $"Ridley tail-tip direction {direction} matches the pinned ROM");
            OamBuffer actual = DrawInstalledRidleySupplement(
                stock, rom, facing: 1, wingFrame: 0, tailDirection: direction);
            var expected = new OamBuffer();
            DrawImportedEnemySpritemap(rom, expected, RidleySupplementalVisualDefinitions.Bank,
                nativeTip, 140, 138, 0x0200, 0,
                clipVerticalWrap: true, originYIsOnScreen: true);
            for (int segment = 5; segment >= 0; segment--)
                DrawImportedEnemySpritemap(rom, expected, RidleySupplementalVisualDefinitions.Bank,
                    RidleySupplementalVisualDefinitions.SegmentFrameAt(segment),
                    unchecked((ushort)(80 + segment * 10)),
                    unchecked((ushort)(120 + segment * 3)),
                    0x0200, 0, clipVerticalWrap: true, originYIsOnScreen: true);
            AssertRidleySupplementOam(expected, actual,
                $"Ridley tail direction {direction} retains tip-first reverse-segment OAM");
        }

        AssertThrows<InvalidDataException>(
            () => RidleySupplementalVisualDefinitions.WingFrameAt(20),
            "Ridley rejects adjacent wing-table code as a pose");
        AssertThrows<InvalidDataException>(
            () => RidleySupplementalVisualDefinitions.TailTipFrameAt(16),
            "Ridley rejects adjacent tail-tip OAM as a pointer-table entry");
    }

    /// <summary>Runs the production Ridley supplemental-sprite renderer for a selected wing pose or tail direction.</summary>
    /// <param name="artwork">Artwork catalog supplied to the enemy system.</param>
    /// <param name="rom">Address space wrapped to detect reads from installed supplemental artwork ranges.</param>
    /// <param name="facing">Ridley facing direction used to select the wing pose.</param>
    /// <param name="wingFrame">Animation frame selected from the compiled wing table.</param>
    /// <param name="tailDirection">Optional tail-tip direction; when present, the renderer also emits the tail segments.</param>
    /// <returns>The OAM entries emitted by the production renderer.</returns>
    private static OamBuffer DrawInstalledRidleySupplement(
        EnemyTileArtworkCatalog artwork, ISnesAddressSpace rom,
        ushort facing, ushort wingFrame, int? tailDirection)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        typeof(RoomEnemySystem).GetField("_bus", flags)!
            .SetValue(enemies, new RidleySupplementReadGuard(rom));
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = 0xe13f;
        slot.XPosition = 200;
        slot.YPosition = 100;
        var state = new RidleyEnemyState
        {
            MovementAnimationEnabled = 1,
            FacingDirection = facing,
            WingFrame = wingFrame,
            SpritemapPaletteIndex = 0x0200,
            TailSegments = tailDirection is null ? [] :
                [.. Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
                {
                    XPosition = unchecked((ushort)(80 + index * 10)),
                    YPosition = unchecked((ushort)(120 + index * 3)),
                    Angle = index == 6 ? unchecked((ushort)(tailDirection.Value << 4)) : (ushort)0,
                })],
        };
        typeof(RoomEnemySystem).GetField("_ridleyState", flags)!.SetValue(enemies, state);
        var oam = new OamBuffer();
        typeof(RoomEnemySystem).GetMethod("DrawRidleySupplementalSprites", flags)!
            .CreateDelegate<Action<OamBuffer, RoomEnemySlot, ushort, ushort>>(enemies)(
                oam, slot, 0, 0);
        return oam;
    }

    /// <summary>Checks that two OAM buffers contain identical entries and end at the same byte offset.</summary>
    /// <param name="expected">Reference OAM rendered from cartridge sprite data.</param>
    /// <param name="actual">OAM produced by the installed-artwork path.</param>
    /// <param name="description">Context included with an assertion failure.</param>
    private static void AssertRidleySupplementOam(
        OamBuffer expected, OamBuffer actual, string description) =>
        AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) &&
                   expected.HighTable.SequenceEqual(actual.HighTable) &&
                   expected.NextByteOffset == actual.NextByteOffset, description);

    /// <summary>Reads a little-endian pointer from the Ridley supplemental-artwork tables.</summary>
    /// <param name="rom">Cartridge address space that contains the table.</param>
    /// <param name="address">Address of the pointer's low byte.</param>
    /// <returns>The 16-bit pointer value.</returns>
    private static ushort ReadRidleySupplementWord(ISnesAddressSpace rom, int address) =>
        unchecked((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));

    /// <summary>
    /// Wraps cartridge reads and rejects access to ROM ranges replaced by the installed Ridley supplemental artwork.
    /// </summary>
    /// <param name="source">Underlying address space used for allowed reads and forwarded writes.</param>
    private sealed class RidleySupplementReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer byte requests through the guarded read implementation.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The requested byte if its address is outside the guarded ranges.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from installed Ridley wing and tail artwork ranges and forwards other reads.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The byte returned by the underlying address space for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address is in a ROM range supplied by the installed artwork catalog.</exception>
        public byte ReadByte(int address) =>
            address is >= 0xa6db02 and < 0xa6db2a or
                >= 0xa6dc90 and < 0xa6de7a
                ? throw new InvalidOperationException(
                    $"Installed Ridley supplement reread ROM byte ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a cartridge memory write to the wrapped address space.</summary>
        /// <param name="address">Address to update.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
