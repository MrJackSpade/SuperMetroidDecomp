using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Exports the native cannon-cover pose/OAM selectors and twelve character tiles: the installation holds each pose's typed
/// placement, which the game binds, and the extracted document and the cover page are import data (#1636, #1638).
/// </summary>
public static partial class SamusArmCannonArtworkFiles
{
    /// <summary>Exports each pose's selector by the pose's name, bounded bank-$90 drawing descriptors, ten direction styles/tile lists, and twelve cannon-cover characters.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for cover placement/OBJ attributes and bank-$9A character pixels.</param>
    /// <param name="importData">The import-data directory receiving the extracted document (#1638) and the 96-by-8 indexed cover page (#1636), which only the import reads; <see cref="Install"/> writes the layout, and no manifest is written (#1693).</param>
    /// <remarks>The sixteen-color PNG palette is diagnostic. Writes are separate and not rolled back if a later write fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A direction's cartridge tile list uploads a closed-cover tile or names a source that is no installed cover tile.</exception>
    /// <exception cref="IOException">Filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string importData)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        Directory.CreateDirectory(importData);
        // The pose table is read by the cartridge's pose numbers and installed by pose name (#1595).
        Dictionary<SamusPoseId, int> descriptors = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose,
            pose => RomDataReader.ReadWordFixedBank(bus,
                SamusSourceDefinitions.PoseDrawingDataPointers.Value + SamusPoseNumbers.Number(pose) * 2) - SamusSourceDefinitions.ArmCannonDrawingData);
        int[] drawingData = Enumerable.Range(0, SamusArmCannonArtworkFormat.DrawingDataByteCount)
            .Select(index => (int)bus.ReadCartridgeByte(SamusSourceDefinitions.MovementBank.Value |
                (SamusSourceDefinitions.ArmCannonDrawingData + index))).ToArray();
        Dictionary<SamusProjectileDirection, SpriteVisualPiece> appearances = SamusProjectileDirectionCodec.InAimOrder.ToDictionary(
            direction => direction,
            direction => SpriteVisualPiece.FromSpritePart(new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(
                bus, SamusSourceDefinitions.ArmCannonSpriteAttributes.Value + direction.ToAimIndex() * 2)).ToSinglePart(),
                SpriteArtwork.CharactersPerRow));
        Dictionary<SamusProjectileDirection, SamusArmCannonCoverOrientation> orientations = SamusProjectileDirectionCodec.InAimOrder
            .ToDictionary(direction => direction, direction => CoverOrientation(bus, direction));
        var document = new SamusArmCannonArtworkDocument
        {
            Version = SamusArmCannonArtworkFormat.Version,
            PoseDescriptors = descriptors,
            DrawingData = drawingData,
            CoverAppearances = appearances,
            CoverOrientations = orientations,
        };
        // The import's cover page holds the cover pictures in the order the cartridge keeps them.
        byte[] planar = [.. ArmCannonTileSources.InSourceOrder.SelectMany(tile => Enumerable.Range(0, SamusArmCannonArtworkFormat.CoverTileByteCount)
            .Select(offset => bus.ReadCartridgeByte(SamusSourceDefinitions.CharacterDataBank.Value | (ArmCannonTileSources.Source(tile) + offset))))];
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            SamusArmCannonArtworkFormat.CoverTileCount,
            out Extent16<ImagePixels> size);
        using var pngStream = new MemoryStream();
        IndexedPng.Write(pngStream, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] png = pngStream.ToArray();
        byte[] json = SamusArmCannonSourceArtwork.Write(document);
        File.WriteAllBytes(Path.Combine(importData, SamusArmCannonArtworkFormat.TileFileName), png);
        File.WriteAllBytes(Path.Combine(importData, SamusArmCannonArtworkFormat.ArtworkFileName), json);
    }

    /// <summary>
    /// The orientation whose opening <paramref name="direction"/>'s cartridge tile list uploads (#2556): its closed frame
    /// uploads nothing and each opening frame the orientation's picture for that frame.
    /// </summary>
    /// <exception cref="InvalidDataException">The list uploads a closed-cover picture, names a source that is no cover picture, or does not open one orientation's pictures in order.</exception>
    private static SamusArmCannonCoverOrientation CoverOrientation(ISnesAddressSpace bus, SamusProjectileDirection direction)
    {
        int list = RomDataReader.ReadWordFixedBank(bus,
            SamusSourceDefinitions.TileListPointers.Value + direction.ToAimIndex() * 2);
        int Source(int frame) => RomDataReader.ReadWordFixedBank(bus,
            SamusSourceDefinitions.MovementBank.Value | (list + frame * 2));
        if (Source(SamusArmCannonCoverFrameCodec.ClosedFrame) != 0)
            throw new InvalidDataException($"Arm-cannon direction {direction} uploads a closed-cover tile.");
        // The first opening frame names the orientation, and every later frame must open the same one.
        SamusArmCannonCoverOrientation[] opened = [.. SamusArmCannonCoverFrameCodec.InOpeningOrder.Select(frame =>
        {
            int source = Source(SamusArmCannonCoverFrameCodec.CartridgeFrame(frame));
            if (!ArmCannonTileSources.TryDecode(source, out SamusArmCannonCoverTile tile))
                throw new InvalidDataException($"Arm-cannon direction {direction} names cover source ${source:X4}, which is no installed tile.");
            return tile.Frame == frame ? tile.Orientation
                : throw new InvalidDataException($"Arm-cannon direction {direction} does not open one orientation's cover pictures in order.");
        })];
        if (opened.Length == 0)
            throw new InvalidDataException($"Arm-cannon direction {direction} opens no cover picture.");
        if (opened.Any(orientation => orientation != opened[0]))
            throw new InvalidDataException($"Arm-cannon direction {direction} does not open one orientation's cover pictures in order.");
        return opened[0];
    }

    /// <summary>
    /// Installs what the game binds of the arm cannon (#1638): each pose's typed placement, decoded by the import from the
    /// extracted descriptors for the animation frames the pose shows. The descriptors, appearances and tile selections stay
    /// with the import.
    /// </summary>
    /// <param name="directory">Artwork directory receiving <c>samus-arm-cannon.json</c>, which is overwritten.</param>
    /// <param name="layout">The placements to install.</param>
    /// <exception cref="IOException">Filesystem output fails.</exception>
    internal static void Install(string directory, SamusArmCannonLayout layout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Ensure.NotNull(layout);
        File.WriteAllBytes(Path.Combine(directory, SamusArmCannonArtworkFormat.JsonFileName), layout.Write());
    }

    /// <summary>
    /// Loads what the game binds of the installed arm cannon, as the player may have edited it: each pose's drawing order,
    /// directions and displacements (#1638). No descriptor, appearance, character page or manifest (#1693) is read.
    /// </summary>
    /// <param name="directory">Artwork directory containing the required layout JSON.</param>
    /// <exception cref="InvalidDataException">Strict JSON or the tables are invalid; admission errors identify the offending filename.</exception>
    /// <exception cref="IOException">A required installed file cannot be read.</exception>
    public static SamusArmCannonLayout Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        return SamusArtworkFile.Read(Path.Combine(directory, SamusArmCannonArtworkFormat.JsonFileName)).Compile(SamusArmCannonLayout.Read);
    }

    /// <summary>
    /// Loads the arm cannon as the import reads it (#1638): the extracted document, with its cover appearances and tile
    /// selections, from the import data <paramref name="importData"/>.
    /// </summary>
    /// <param name="importData">The component's import-data directory, holding the extracted document.</param>
    /// <exception cref="InvalidDataException">Strict JSON, selectors or appearances are invalid; admission errors identify the offending filename.</exception>
    /// <exception cref="IOException">A required file cannot be read.</exception>
    public static SamusArmCannonSourceArtwork LoadForImport(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        return SamusArmCannonSourceArtwork.FromPlacement(SamusArtworkFile.Read(Path.Combine(importData, SamusArmCannonArtworkFormat.ArtworkFileName))
            .Compile(SamusArmCannonSourceArtwork.LoadPlacement));
    }

    /// <summary>Loads the cover characters of <paramref name="cannon"/> from the import's page in <paramref name="importData"/> (#1636).</summary>
    /// <param name="cannon">The arm cannon as the import reads it, whose tile selections name the characters.</param>
    /// <param name="importData">The component's import-data directory, holding the 96-by-8 indexed cover page.</param>
    /// <exception cref="InvalidDataException">The page is invalid; the error names its file.</exception>
    public static SamusArmCannonCoverCharacters LoadCoverCharacters(SamusArmCannonSourceArtwork cannon, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        return new SamusArmCannonCoverCharacters(cannon, SamusArtworkFile.Read(Path.Combine(importData, SamusArmCannonArtworkFormat.TileFileName))
            .Compile(stream => RoomCharacterAtlasPng.Read(stream,
                SamusArmCannonArtworkFormat.CoverTileCount * SamusArmCannonArtworkFormat.CoverTileByteCount)));
    }
}
