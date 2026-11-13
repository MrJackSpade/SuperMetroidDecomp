using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomBlockShapeAssertions;
namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks Mother Brain escape-gate PLM program and draw fields against native data.</summary>
internal static class LookupEscapeGateVerification
{
    /// <summary>Checks escape-gate control opcodes and operands over the complete program domain.</summary>
    internal static void VerifyEscapeGateProgramControls(CartridgeImportAddressSpace rom) => VerifyEscapeGateProgramField(rom, false);
    /// <summary>Checks escape-gate draw operands over the complete program domain.</summary>
    internal static void VerifyEscapeGateProgramDraws(CartridgeImportAddressSpace rom) => VerifyEscapeGateProgramField(rom, true);

    /// <summary>Compares control or draw bytes and overlapping words with bank-$84 instructions.</summary>
    private static void VerifyEscapeGateProgramField(CartridgeImportAddressSpace rom, bool draw)
    {
        ushort[] controls = [0xbb34,0xbb38,0xbb3a,0xbb3e,0xbb42,0xbb44,0xbb48,0xbb4c,0xbb50];
        ushort[] draws = [0xbb36,0xbb3c,0xbb40,0xbb46,0xbb4a,0xbb4e];
        ushort[] fields = draw ? draws : controls;
        bool OwnsByte(int address) => fields.Contains((ushort)(address & ~1));
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool byteOwned = raw is >= 0xbb34 and <= 0xbb51;
            bool wordOwned = raw is >= 0xbb34 and < 0xbb51;
            AssertEqual(byteOwned, MotherBrainEscapeGateObjectProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Escape gate complete byte domain");
            AssertEqual(wordOwned, MotherBrainEscapeGateObjectProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "Escape gate complete overlapping word domain");
            if (!byteOwned) AssertEqual((byte)0, b, "Escape gate missing byte output");
            else if (OwnsByte(raw))
            {
                byte expected = EscapeGateProgramFieldRomByte(rom, 0x840000 | raw);
                AssertEqual(expected, b, "Escape gate original field byte");
            }
            if (!wordOwned) AssertEqual((ushort)0, w, "Escape gate missing word output");
            else
            {
                int mask = (OwnsByte(raw) ? 0xff : 0) | (OwnsByte(raw + 1) ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterObjectWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "Escape gate original field in overlapping word");
            }
        }
    }

    /// <summary>Reads a native escape-gate program byte for independent field comparison.</summary>
    private static byte EscapeGateProgramFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>The three gate frames' draw-list addresses and published names.</summary>
    private static readonly LookupEscapeGateStockFrame[] Frames = [new(0x9473, "open"), new(0x947f, "half-closed"), new(0x948b, "closed")];

    /// <summary>
    /// Checks which draws are the gate's, each frame's one vertical run against the cartridge, and that each frame's shape
    /// covers exactly the four cells its draw list writes.
    /// </summary>
    internal static void VerifyEscapeGateDrawGeometry(CartridgeImportAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool owned = Frames.Any(frame => frame.Pointer == raw);
            AssertEqual(owned, MotherBrainEscapeGateDraws.TryDescribe(NativeObjectDraws.At<MotherBrainEscapeGateDraw>(raw), out RoomBlockShape? shape), "Escape gate descriptor domain");
            if (!owned) AssertTrue(shape is null, "Escape gate missing descriptor");
        }
        AssertLayoutsAsNative(rom, RoomObjectEscapeGateVisualFiles.Family, Frames, collision: false);
        foreach (LookupEscapeGateStockFrame frame in Frames)
        {
            AssertEqual((ushort)0x8004, ReadSamusEaterObjectWord(rom, 0x840000 | frame.Pointer), "Escape gate native vertical geometry");
            AssertEqual((ushort)0, ReadSamusEaterObjectWord(rom, 0x840000 | (frame.Pointer + 10)), "Escape gate native final offset");
            if (!MotherBrainEscapeGateDraws.TryDescribe(NativeObjectDraws.At<MotherBrainEscapeGateDraw>(frame.Pointer), out RoomBlockShape? shape))
                throw new InvalidOperationException($"Escape gate frame {frame.Id} has no shape.");
            RoomBlockShape described = shape;
            AssertShapeAsNative(rom, described, frame.Pointer, frame.Id);
            foreach (Offset16<RoomBlocks> outside in new Offset16<RoomBlocks>[] { new(0, -1), new(0, 4) })
                AssertThrows<ArgumentOutOfRangeException>(() => described.CollisionAt(outside), "Escape gate shape covers only its column's four cells");
        }
    }

    /// <summary>Checks that every cell of every gate frame collides as the cartridge's does.</summary>
    internal static void VerifyEscapeGateDrawCollision(CartridgeImportAddressSpace rom) =>
        AssertLayoutsAsNative(rom, RoomObjectEscapeGateVisualFiles.Family, Frames, collision: true);
}
