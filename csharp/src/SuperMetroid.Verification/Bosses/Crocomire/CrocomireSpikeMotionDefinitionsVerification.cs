using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Compares compiled spike motion limits for the wall's eight pieces with bank-$86 retail data.</summary>
internal static class CrocomireSpikeMotionDefinitionsVerification
{
    /// <summary>
    /// The native table entry of the top piece: the cartridge indexes its motion tables by projectile slot, and the
    /// pieces take the pool's last eight slots, the top piece the first of them.
    /// </summary>
    private const int TopPieceTableEntry = 10;

    /// <summary>Checks each piece's acceleration increment and rejects places outside the wall.</summary>
    internal static void VerifyCrocomireSpikeAccelerationDelta(CartridgeImportAddressSpace rom)
    {
        for (int piece = 0; piece < CrocomireSpikeMotionDefinitions.PieceCount; piece++)
        {
            int address = 0x8691c3 + 2 * (TopPieceTableEntry + piece);
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, CrocomireSpikeMotion.AccelerationDelta(piece),
                $"Crocomire spike acceleration increment, piece {piece}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.AccelerationDelta(-1), "negative increment piece");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.AccelerationDelta(CrocomireSpikeMotionDefinitions.PieceCount), "increment piece past end");
    }

    /// <summary>Checks each piece's acceleration cap and rejects places outside the wall.</summary>
    internal static void VerifyCrocomireSpikeMaximumAcceleration(CartridgeImportAddressSpace rom)
    {
        for (int piece = 0; piece < CrocomireSpikeMotionDefinitions.PieceCount; piece++)
        {
            int address = 0x8691e7 + 2 * (TopPieceTableEntry + piece);
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, CrocomireSpikeMotion.MaximumAcceleration(piece),
                $"Crocomire spike acceleration limit, piece {piece}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.MaximumAcceleration(-1), "negative acceleration limit piece");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.MaximumAcceleration(CrocomireSpikeMotionDefinitions.PieceCount), "acceleration limit piece past end");
    }

    /// <summary>Checks the low byte of each piece's velocity cap and rejects places outside the wall.</summary>
    internal static void VerifyCrocomireSpikeMaximumVelocity(CartridgeImportAddressSpace rom)
    {
        for (int piece = 0; piece < CrocomireSpikeMotionDefinitions.PieceCount; piece++)
            AssertEqual(rom.ReadCartridgeByte(0x86920b + 2 * (TopPieceTableEntry + piece)), CrocomireSpikeMotion.MaximumVelocity(piece),
                $"Crocomire spike velocity limit, piece {piece}");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.MaximumVelocity(-1), "negative velocity limit piece");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireSpikeMotion.MaximumVelocity(CrocomireSpikeMotionDefinitions.PieceCount), "velocity limit piece past end");
    }
}
