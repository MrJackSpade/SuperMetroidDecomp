using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Compares the compiled spike-wall pieces' motion limits for the wall's eight pieces with bank-$86 retail data.</summary>
internal static class CrocomireSpikeMotionDefinitionsVerification
{
    /// <summary>
    /// The native table entry of the top piece: the cartridge indexes its motion tables by projectile slot, and the
    /// pieces take the pool's last eight slots, the top piece the first of them.
    /// </summary>
    private const int TopPieceTableEntry = 10;

    /// <summary>The pieces the wall breaks into.</summary>
    private const int WallPieces = 8;

    /// <summary>The spacing, in pixels, between one piece's place below the wall's top and the next piece's.</summary>
    private const int PieceSpacing = 16;

    /// <summary>
    /// Checks each piece's acceleration growth, its depth below the wall's top and its landing sound. The depth and the
    /// sound replace the spawn's spacing of each place by sixteen pixels and the crumble taken from every other place from
    /// the top (#2539), which nothing in the cartridge's tables holds.
    /// </summary>
    internal static void VerifyCrocomireSpikeAccelerationDelta(CartridgeImportAddressSpace rom)
    {
        AssertEqual(WallPieces, CrocomireSpikeMotionDefinitions.Pieces.Count, "Crocomire's spike wall breaks into eight pieces");
        for (int piece = 0; piece < WallPieces; piece++)
        {
            int address = 0x8691c3 + 2 * (TopPieceTableEntry + piece);
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            CrocomireSpikeWallPiece definition = CrocomireSpikeMotionDefinitions.Pieces[piece];
            AssertEqual(SignedSixteenSixteen.FromParts(0, expected), definition.AccelerationGrowth,
                $"Crocomire spike acceleration increment, piece {piece}");
            AssertEqual(piece * PieceSpacing, definition.Depth, $"Crocomire spike depth below the wall's top, piece {piece}");
            SoundEffectId? crumble = piece % 2 == 0 ? CrocomireSoundDefinitions.Crumble : null;
            AssertEqual(crumble, definition.CrumbleSound, $"Crocomire spike crumble on landing, piece {piece}");
        }
    }

    /// <summary>Checks each piece's acceleration cap.</summary>
    internal static void VerifyCrocomireSpikeMaximumAcceleration(CartridgeImportAddressSpace rom)
    {
        for (int piece = 0; piece < WallPieces; piece++)
        {
            int address = 0x8691e7 + 2 * (TopPieceTableEntry + piece);
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(SignedSixteenSixteen.FromParts(0, expected), CrocomireSpikeMotionDefinitions.Pieces[piece].MaximumAcceleration,
                $"Crocomire spike acceleration limit, piece {piece}");
        }
    }

    /// <summary>Checks the low byte of each piece's velocity cap.</summary>
    internal static void VerifyCrocomireSpikeMaximumVelocity(CartridgeImportAddressSpace rom)
    {
        for (int piece = 0; piece < WallPieces; piece++)
            AssertEqual(rom.ReadCartridgeByte(0x86920b + 2 * (TopPieceTableEntry + piece)), CrocomireSpikeMotionDefinitions.Pieces[piece].FastestSpeed,
                $"Crocomire spike velocity limit, piece {piece}");
    }
}
