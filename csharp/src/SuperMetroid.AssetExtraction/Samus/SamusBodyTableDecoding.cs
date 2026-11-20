using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The one place Samus's bank-$92 body tables are read (#2556): the definition-group starts, each pose's first frame
/// selector, the four-byte frame selectors, the pose-half spritemap bases and the spritemap pointer table, with the
/// placement tables. They are decoded once into what each pose's picture frames show: the top and bottom half pictures,
/// where the frame places the body, and the compositions its halves show, each half picture and composition named by
/// the first pose frame that shows it; and into the compositions the suit explosion and the diving splash and bubbles
/// show, named by their sheet frames. Record numbers, selector indexes and table entries stay inside this decoding.
/// </summary>
[EncodingOwner("Samus's bank-$92 body definition, frame selector and spritemap tables")]
internal static class SamusBodyTableDecoding
{
    /// <summary>Number of records in the cartridge's pose tables.</summary>
    internal const int PoseCount = SamusPoseNumbering.RecordCount;
    /// <summary>Number of four-byte frame selectors, stored one after another.</summary>
    private const int FrameCount = 1143;
    /// <summary>Number of upper-body definition groups.</summary>
    private const int TopSetCount = 13;
    /// <summary>Number of lower-body definition groups.</summary>
    private const int BottomSetCount = 11;
    /// <summary>Number of body definition records, top and bottom groups together.</summary>
    private const int RecordCount = 435;
    /// <summary>Number of entries in the pose-half spritemap table.</summary>
    private const int EntryCount = 2096;
    /// <summary>The largest number of parts one composition may hold.</summary>
    private const int MaxParts = 128;
    /// <summary>The lower group index with which a frame selector names no lower half.</summary>
    private const int NoBottomSet = byte.MaxValue;
    /// <summary>The spritemap table entry of the right-facing explosion's first frame; its later frames follow it.</summary>
    private const int RightExplosionFirstEntry = 2076;
    /// <summary>The spritemap table entry of the left-facing explosion's first frame; its later frames follow it.</summary>
    private const int LeftExplosionFirstEntry = 2085;
    /// <summary>The spritemap table entry of the diving splash's first frame; its later frames follow it.</summary>
    private const int DivingSplashFirstEntry = 399;
    /// <summary>The spritemap table entry of the bubbles' first frame; its later frames follow it.</summary>
    private const int BubblesFirstEntry = 390;

    /// <summary>
    /// Decodes the body tables <paramref name="bus"/> holds into what each frame <paramref name="shown"/> pictures shows:
    /// each pose's picture frames whose half pictures, placement and compositions the tables hold, by the frame's place
    /// in the game's layout of the pose's animation, and the compositions the explosion and atmosphere sheets show.
    /// </summary>
    /// <param name="bus">The cartridge.</param>
    /// <param name="shown">The frames her body can show in each pose and the picture each draws.</param>
    /// <param name="pictures">Receives the characters each half picture transfers.</param>
    /// <exception cref="InvalidDataException">A table, definition or composition the frames select is invalid.</exception>
    internal static SamusBodyDecoded Decode(ISnesAddressSpace bus, SamusShownFrames shown,
        out IReadOnlyDictionary<SamusBodyPicture, SamusBodyTileTransfer> pictures)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(shown);
        IImportCartridgeSource cartridge = bus;
        int[] topStarts = [.. ReadPointers(cartridge, SamusSourceDefinitions.TopDefinitionListPointers.Value, TopSetCount).Select(BodyRecord)];
        int[] bottomStarts = [.. ReadPointers(cartridge, SamusSourceDefinitions.BottomDefinitionListPointers.Value, BottomSetCount).Select(BodyRecord)];
        int firstRecord = topStarts.Concat(bottomStarts).Min();
        ushort[] posePointers = ReadPointers(cartridge, SamusSourceDefinitions.AnimationDefinitionListPointers.Value, PoseCount);
        int[] artOrigins = [.. Enumerable.Range(0, PoseCount).Select(pose => (int)unchecked((sbyte)bus.ReadCartridgeByte(
            SamusSourceDefinitions.PoseDefinitions.Value + pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4)))];
        ushort[] topBases = ReadPointers(cartridge, SamusSourceDefinitions.SpritemapTopBases.Value, PoseCount);
        ushort[] bottomBases = ReadPointers(cartridge, SamusSourceDefinitions.SpritemapBottomBases.Value, PoseCount);
        ushort[] spritemapPointers = ReadPointers(cartridge, SamusSourceDefinitions.SpritemapPointers.Value, EntryCount);
        Dictionary<SamusPoseId, SamusBodyPosePlacement> placements = SamusBodyPlacementDecoding.Decode(artOrigins,
            ReadBytes(bus, SamusSourceDefinitions.LandingVerticalOffsets.Value, SamusBodyPlacementSourceDefinitions.LandingByteCount, signed: false),
            ReadBytes(bus, SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value, SamusBodyPlacementSourceDefinitions.PostureByteCount, signed: true),
            ReadBytes(bus, SamusSourceDefinitions.DrainedVerticalOffsets.Value, SamusBodyPlacementSourceDefinitions.DrainedByteCount, signed: true));

        var pictureNames = new Dictionary<int, SamusBodyPicture>();
        var transfers = new Dictionary<SamusBodyPicture, SamusBodyTileTransfer>();
        var compositionNames = new Dictionary<ushort, SamusSpritemapId>();
        var compositions = new Dictionary<SamusSpritemapId, SpriteVisualPiece[]>();
        SamusBodyPicture PictureOf(int record, string user)
        {
            if (!pictureNames.TryGetValue(record, out SamusBodyPicture picture))
            {
                picture = new SamusBodyPicture(user);
                pictureNames.Add(record, picture);
                transfers.Add(picture, ReadDefinition(bus, record));
            }
            return picture;
        }
        SamusSpritemapId? CompositionOf(ushort pointer, string user)
        {
            if (pointer == 0)
                return null;
            if (!compositionNames.TryGetValue(pointer, out SamusSpritemapId composition))
            {
                composition = new SamusSpritemapId(user);
                compositionNames.Add(pointer, composition);
                compositions.Add(composition, ReadSpritemap(bus, pointer));
            }
            return composition;
        }

        var frames = new Dictionary<SamusPoseId, IReadOnlyDictionary<int, SamusBodyDecodedFrame>>();
        foreach (SamusPoseId pose in SamusPoseIdKeys.ListedOrder.Where(SamusPoseNumbers.RecordedPoses.Contains))
        {
            int number = SamusPoseNumbers.Number(pose);
            int firstFrame = BodyFrame(posePointers[number]);
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
            var decoded = new Dictionary<int, SamusBodyDecodedFrame>();
            string poseKey = SamusPoseIdKeys.Installed.Of(pose);
            IEnumerable<SamusAnimationFrame> pictured = shown.PictureFrames.ContainsKey(pose) ? shown.Pictures(pose) : [];
            foreach (SamusAnimationFrame picture in pictured.Distinct().Order(animation.FrameOrder))
            {
                int frame = SamusAnimationLayout.Offset(animation, picture);
                if (frame >= Math.Min(SamusBodySheetDefinitions.AnimationFrames, FrameCount - firstFrame) ||
                    !placements[pose].TryFrameRise(frame, out int rise))
                    continue;
                int selector = SamusSourceDefinitions.BodyFrames.Value + (firstFrame + frame) * 4;
                int topSet = bus.ReadCartridgeByte(selector), topPosition = bus.ReadCartridgeByte(selector + 1);
                int bottomSet = bus.ReadCartridgeByte(selector + 2), bottomPosition = bus.ReadCartridgeByte(selector + 3);
                bool namesBottom = bottomSet != NoBottomSet;
                bool showsBottom = SamusBodyCompositions.ShowsBottomHalf(pose, frame);
                if (!TryRecord(topStarts, firstRecord, topSet, topPosition, out int topRecord) ||
                    (namesBottom && !TryRecord(bottomStarts, firstRecord, bottomSet, bottomPosition, out _)) ||
                    !Covers(topBases[number], frame) || (showsBottom && !Covers(bottomBases[number], frame)))
                    continue;
                string user = $"{poseKey} {animation.NameOf(picture)}";
                SamusBodyPicture top = PictureOf(topRecord, $"top {user}");
                SamusBodyPicture? bottom = namesBottom ? PictureOf(bottomStarts[bottomSet] + bottomPosition, $"bottom {user}") : null;
                decoded.Add(frame, new SamusBodyDecodedFrame(new SamusBodyFramePictures(top, bottom), rise,
                    CompositionOf(spritemapPointers[topBases[number] + frame], $"top {user}"),
                    showsBottom ? CompositionOf(spritemapPointers[bottomBases[number] + frame], $"bottom {user}") : null));
            }
            frames.Add(pose, decoded);
        }

        var explosion = new Dictionary<string, SamusSpritemapId?>(StringComparer.Ordinal);
        foreach (HorizontalDirection facing in SamusDeathSheetComposer.Facings)
        {
            int first = facing == HorizontalDirection.Left ? LeftExplosionFirstEntry : RightExplosionFirstEntry;
            for (int frame = 0; frame < SamusDeathSheetDefinitions.Explosion.Count; frame++)
            {
                string name = SamusDeathSheetDefinitions.ExplosionFrame(facing, SamusDeathSheetDefinitions.Explosion[frame]);
                explosion.Add(name, CompositionOf(spritemapPointers[first + frame], name));
            }
        }
        var atmosphere = new Dictionary<string, SamusSpritemapId?>(StringComparer.Ordinal);
        void AddAtmosphere(IReadOnlyList<SamusAtmosphereFrame> effect, int first)
        {
            for (int frame = 0; frame < effect.Count; frame++)
            {
                string name = SamusAtmosphereSheetDefinitions.FrameName(effect[frame]);
                atmosphere.Add(name, CompositionOf(spritemapPointers[first + frame], name));
            }
        }
        AddAtmosphere(SamusAtmosphereSheetDefinitions.DivingSplash, DivingSplashFirstEntry);
        AddAtmosphere(SamusAtmosphereSheetDefinitions.Bubbles, BubblesFirstEntry);
        pictures = transfers;
        return new SamusBodyDecoded(
            SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose => placements[pose].ArtOrigin),
            frames, compositions, explosion, atmosphere);
    }

    /// <summary>Whether the spritemap table holds an entry for the frame <paramref name="frame"/> places after <paramref name="first"/>.</summary>
    private static bool Covers(int first, int frame) =>
        first < EntryCount && frame < Math.Min(SamusBodySheetDefinitions.AnimationFrames, EntryCount - first);

    /// <summary>
    /// The record a native set and position select: the selected group's first record plus the position. The native
    /// selector steps past the chosen group's first without a per-group bound, so several authored frames land in the
    /// next group; the physical record is resolved, not a per-group index.
    /// </summary>
    private static bool TryRecord(int[] starts, int firstRecord, int set, int position, out int record)
    {
        if ((uint)set >= (uint)starts.Length)
        {
            record = default;
            return false;
        }
        record = starts[set] + position;
        return record >= firstRecord && record < RecordCount;
    }

    /// <summary>Reads the seven-byte body definition <paramref name="record"/> and the characters its two transfers move.</summary>
    /// <exception cref="InvalidDataException">The definition's source or sizes are invalid.</exception>
    private static SamusBodyTileTransfer ReadDefinition(ISnesAddressSpace bus, int record)
    {
        IImportCartridgeSource cartridge = bus;
        int definitionAddress = CartridgeBankWindow.At(SamusSourceDefinitions.AnimationBank,
            SamusSourceDefinitions.FirstBodyDefinition + record * SamusSourceDefinitions.BodyDefinitionBytes);
        int sourceOffset = cartridge.ReadCartridgeWord(definitionAddress);
        int sourceBank = bus.ReadCartridgeByte(definitionAddress + 2);
        int firstSize = cartridge.ReadCartridgeWord(definitionAddress + 3);
        int secondSize = cartridge.ReadCartridgeWord(definitionAddress + 5);
        int byteCount = firstSize + secondSize;
        if (sourceOffset < SnesBusLayout.RomWindowStart || firstSize == 0 || byteCount > SamusBodyCharacterArtwork.BytesPerDefinitionSlot ||
            byteCount % 32 != 0 || sourceOffset + byteCount > CartridgeFormatDefinitions.BankByteCount)
            throw new InvalidDataException($"Invalid Samus body DMA at ${definitionAddress:X6}.");
        int sourceAddress = (int)SnesAddress.InBank(sourceBank, sourceOffset);
        byte[] planar = new byte[byteCount];
        for (int i = 0; i < byteCount; i++)
            planar[i] = bus.ReadCartridgeByte(sourceAddress + i);
        return new SamusBodyTileTransfer(firstSize, secondSize, planar);
    }

    /// <summary>Imports a bounded bank-$92 Samus map, decoding its five-byte records into stored sprite parts.</summary>
    private static SpriteVisualPiece[] ReadSpritemap(ISnesAddressSpace bus, int pointer)
    {
        IImportCartridgeSource cartridge = bus;
        int address = CartridgeBankWindow.At(SamusSourceDefinitions.AnimationBank, pointer);
        int count = cartridge.ReadCartridgeWord(address);
        if (pointer < SamusSourceDefinitions.FirstSpritemap || count > MaxParts || pointer + 2 + count * 5 > CartridgeFormatDefinitions.BankByteCount)
            throw new InvalidDataException($"Invalid Samus spritemap at $92:{pointer:X4}.");
        var parts = new SpriteVisualPiece[count];
        for (int i = 0; i < count; i++)
        {
            int part = address + 2 + i * 5;
            var record = new SpritemapPart(new SnesSpritemapXWord(cartridge.ReadCartridgeWord(part)),
                new SnesSpritemapYByte(bus.ReadCartridgeByte(part + 2)), cartridge.ReadCartridgeWord(part + 3));
            parts[i] = SpriteVisualPiece.FromSpritePart(record.ToPart(), SpriteArtwork.CharactersPerRow);
        }
        return parts;
    }

    /// <summary>Reads a contiguous little-endian pointer table from the cartridge.</summary>
    private static ushort[] ReadPointers(IImportCartridgeSource cartridge, int start, int count) =>
        [.. Enumerable.Range(0, count).Select(i => cartridge.ReadCartridgeWord(start + i * 2))];

    /// <summary>Reads <paramref name="count"/> placement bytes from <paramref name="start"/>, signed or not.</summary>
    private static int[] ReadBytes(ISnesAddressSpace bus, int start, int count, bool signed) =>
        [.. Enumerable.Range(0, count).Select(index => bus.ReadCartridgeByte(start + index))
            .Select(value => signed ? (int)unchecked((sbyte)value) : value)];

    /// <summary>The record of the body DMA definition at bank-$92 <paramref name="pointer"/>.</summary>
    private static int BodyRecord(ushort pointer) =>
        pointer >= SamusSourceDefinitions.FirstBodyDefinition && (pointer - SamusSourceDefinitions.FirstBodyDefinition) % SamusSourceDefinitions.BodyDefinitionBytes == 0
            ? (pointer - SamusSourceDefinitions.FirstBodyDefinition) / SamusSourceDefinitions.BodyDefinitionBytes
            : throw new InvalidDataException($"Samus body definition group ${pointer:X4} is not a record.");

    /// <summary>The frame selector at bank-$92 <paramref name="pointer"/>.</summary>
    private static int BodyFrame(ushort pointer) =>
        pointer >= SamusSourceDefinitions.FirstBodyFrame && (pointer - SamusSourceDefinitions.FirstBodyFrame) % 4 == 0 &&
        (pointer - SamusSourceDefinitions.FirstBodyFrame) / 4 < FrameCount
            ? (pointer - SamusSourceDefinitions.FirstBodyFrame) / 4
            : throw new InvalidDataException($"Samus pose frame list ${pointer:X4} is not a frame selector.");
}
