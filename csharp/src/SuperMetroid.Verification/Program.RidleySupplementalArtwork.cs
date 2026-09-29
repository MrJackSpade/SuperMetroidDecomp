using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
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
            expected.AddEnemySpritemap(rom, RidleySupplementalVisualDefinitions.Bank,
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
            expected.AddEnemySpritemap(rom, RidleySupplementalVisualDefinitions.Bank,
                nativeTip, 140, 138, 0x0200, 0,
                clipVerticalWrap: true, originYIsOnScreen: true);
            for (int segment = 5; segment >= 0; segment--)
                expected.AddEnemySpritemap(rom, RidleySupplementalVisualDefinitions.Bank,
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

    private static void AssertRidleySupplementOam(
        OamBuffer expected, OamBuffer actual, string description) =>
        AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) &&
                   expected.HighTable.SequenceEqual(actual.HighTable) &&
                   expected.NextByteOffset == actual.NextByteOffset, description);

    private static ushort ReadRidleySupplementWord(ISnesAddressSpace rom, int address) =>
        unchecked((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));

    private sealed class RidleySupplementReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address is >= 0xa6db02 and < 0xa6db2a or
                >= 0xa6dc90 and < 0xa6de7a
                ? throw new InvalidOperationException(
                    $"Installed Ridley supplement reread ROM byte ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
