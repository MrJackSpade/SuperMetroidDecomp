using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Verification's native view of the arm cannon: descriptors by their bank-$90 address and cover tiles by their
/// bank-$9A source, as the cartridge's tables store them.
/// </summary>
internal static class SamusArmCannonNative
{
    /// <summary>Native views of the arm cannon's import document (#2604), which the import composes the cannon from.</summary>
    extension(SamusArmCannonArtworkDocument document)
    {
        /// <summary>The bank-$90 address of <paramref name="pose"/>'s drawing descriptor, as the document places it.</summary>
        internal ushort PoseDrawingData(SamusPoseId pose) =>
            checked((ushort)(SamusSourceDefinitions.ArmCannonDrawingData + document.PoseDescriptors[pose]));

        /// <summary>The document's drawing byte at bank-$90 <paramref name="address"/>.</summary>
        /// <exception cref="InvalidDataException">The address lies outside the extracted drawing window.</exception>
        internal byte ReadDrawingByteAt(int address)
        {
            int offset = address - SamusSourceDefinitions.ArmCannonDrawingData;
            return (uint)offset < (uint)document.DrawingData.Length
                ? checked((byte)document.DrawingData[offset])
                : throw new InvalidDataException($"Arm-cannon drawing byte {offset} is not extracted.");
        }
    }

    /// <summary>Native views of the installed arm cannon.</summary>
    extension(SamusArmCannonSourceArtwork cannon)
    {
        /// <summary>
        /// The bank-$9A source of the cover picture cartridge aim row <paramref name="direction"/> shows at the cartridge's
        /// cover frame <paramref name="frame"/>, or zero for the closed cover.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The row names no aim or the frame no cover frame.</exception>
        internal ushort TileSource(int direction, int frame)
        {
            SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection aim = SamusProjectileDirectionCodec.FromAimIndex(direction);
            if (frame == SamusArmCannonCoverFrameCodec.ClosedFrame)
                return 0;
            if (frame < 1 || frame > SamusArmCannonCoverFrameCodec.InOpeningOrder.Count)
                throw new ArgumentOutOfRangeException(nameof(frame), frame, "The cover list holds four frames.");
            return ArmCannonTileSources.Source(cannon.CoverTile(aim, SamusArmCannonCoverFrameCodec.InOpeningOrder[frame - 1]));
        }
    }

    /// <summary>Native views of the arm cannon's installed tile files.</summary>
    extension(SamusArmCannonArtworkFormat)
    {
        /// <summary>The bank-$9A source of each cover picture, in the order the cartridge keeps them.</summary>
        internal static IReadOnlyList<ushort> TileSourcePointers => ArmCannonTileSources.InSourceOrder.Select(ArmCannonTileSources.Source).ToList();
    }
}
