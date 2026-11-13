using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>
/// Reproduces #1642: the walking Wrecked Ship Chozo statue's installed pictures, compared pixel for pixel with the pictures
/// the cartridge composes of them in the Bowling Alley once Phantoon is dead, the one room state that shows the statue
/// walking (its walk, <c>InstList_Chozo_WreckedShip_Activated_2</c> at $AA:E4C1, shows spritemaps 4 to 11, and only the
/// Wrecked Ship statue's activation reaches it, which waits on the area's boss being dead, $AA:E7AE).
/// <para>
/// The cartridge's picture is composed here from the cartridge alone, never from the import: the statue's spritemaps, the
/// tiles its enemy header names, uploaded where that room's graphics set (Chozo first, $B4:8C01) puts its first record,
/// and the colours its initialization loads for the Wrecked Ship statue. Its initialization clears the statue's palette
/// bits ($AA:E74C), so each part shows the row its own spritemap entry names: the statue's body row one and its back leg
/// row two, both of which it fills with the Wrecked Ship statue's colours ($AA:E75E to $AA:E771). The statue is drawn in
/// those colours for its whole life, so its installed pictures, the ones a reviewer sees and an editor changes, show them
/// as stored, before any shade.
/// </para>
/// </summary>
internal static class ChozoStatueWalkingPicturesVerification
{
    /// <summary>The Chozo statue's enemy header.</summary>
    private const int StatueHeader = 0xA0F0FF;

    /// <summary>Where the header holds the byte count of the statue's tiles, which the set's load reads ($A0:8DBC).</summary>
    private const int HeaderTileByteCount = 0x00;

    /// <summary>Where the header holds the long address of the statue's tiles, which the set's load reads ($A0:8E0D, $A0:8E12).</summary>
    private const int HeaderTileAddress = 0x36;

    /// <summary>The byte-count bits the set's load keeps of the header's word; its top bit asks for a fixed staging block ($A0:8E1F).</summary>
    private const int TileByteCountBits = 0x7FFF;

    /// <summary>
    /// The sprite character the first record of a graphics set uploads its tiles to: the set's load stages them $800 bytes
    /// into the enemy tile buffer ($A0:8D69, $A0:8E3E), which the room load transfers to the sprite characters from $C0 on
    /// ($A0:8CE4). The first record also draws at character index zero ($A0:8DA5, $A0:8E5F), so a spritemap's tile number
    /// names the character itself.
    /// </summary>
    private const int FirstRecordCharacter = 0x100;

    /// <summary>The Wrecked Ship statue's colours its initialization loads into sprite row one ($AA:E768).</summary>
    private const int WreckedShipRowOneColours = 0xAAE31D;

    /// <summary>The Wrecked Ship statue's colours its initialization loads into sprite row two ($AA:E761).</summary>
    private const int WreckedShipRowTwoColours = 0xAAE33D;

    /// <summary>The bytes of one spritemap entry: a placement word, a vertical offset byte and an attribute word.</summary>
    private const int EntryByteCount = 5;

    /// <summary>The bytes of one sprite character: eight rows of four bit planes.</summary>
    private const int CharacterByteCount = 32;

    /// <summary>The colours of one sprite row.</summary>
    private const int RowColourCount = 16;

    /// <summary>The sprite rows.</summary>
    private const int RowCount = 8;

    /// <summary>The sprite row the statue's body shows, which its initialization fills.</summary>
    private const int BodyRow = 1;

    /// <summary>The sprite row the statue's back leg shows, which its initialization fills.</summary>
    private const int BackLegRow = 2;

    /// <summary>A spritemap entry's large-size bit, in its placement word.</summary>
    private const int LargeBit = 0x8000;

    /// <summary>A spritemap entry's nine-bit horizontal offset, in its placement word.</summary>
    private const int HorizontalOffsetBits = 0x1FF;

    /// <summary>The sign bit of a spritemap entry's nine-bit horizontal offset.</summary>
    private const int HorizontalSignBit = 0x100;

    /// <summary>A spritemap entry's tile number, in its attribute word.</summary>
    private const int TileBits = 0x1FF;

    /// <summary>The bit of a tile number that selects the second half of the sprite characters.</summary>
    private const int TileHalfBit = 0x100;

    /// <summary>Where a spritemap entry's palette row starts in its attribute word.</summary>
    private const int RowShift = 9;

    /// <summary>A spritemap entry's palette row, once shifted down.</summary>
    private const int RowBits = 7;

    /// <summary>A spritemap entry's horizontal mirror bit, in its attribute word.</summary>
    private const int MirrorXBit = 0x4000;

    /// <summary>A spritemap entry's vertical mirror bit, in its attribute word.</summary>
    private const int MirrorYBit = 0x8000;

    /// <summary>The side of a small part and of one character, in pixels.</summary>
    private const int SmallSide = 8;

    /// <summary>The side of a large part on the gameplay screen, in pixels.</summary>
    private const int LargeSide = 16;

    /// <summary>How many differing pixels a failure names for each frame.</summary>
    private const int NamedDifferences = 8;

    /// <summary>The walking statue's poses, each with the cartridge spritemap it shows, in the order its walk shows them.</summary>
    private static readonly WalkingPose[] WalkingPoses =
    [
        new(EnemyFrameId.ChozoStatueWreckedShipWalkingFirst, 0xAAEC49),
        new(EnemyFrameId.ChozoStatueWreckedShipWalkingSecond, 0xAAE943),
        new(EnemyFrameId.ChozoStatueWreckedShipWalkingThird, 0xAAE9AE),
        new(EnemyFrameId.ChozoStatueWreckedShipWalkingFourth, 0xAAEA1E),
        new(EnemyFrameId.ChozoStatueWreckedShipWalkingFifth, 0xAAEA8E),
        new(EnemyFrameId.ChozoStatueWreckedShipWalkingSixth, 0xAAEAFE),
        new(EnemyFrameId.ChozoStatueWreckedShipWalkingSeventh, 0xAAEB69),
        new(EnemyFrameId.ChozoStatueWreckedShipWalkingEighth, 0xAAEBD9),
    ];

    /// <summary>
    /// Compares each walking pose's installed picture in the Bowling Alley once Phantoon is dead with the cartridge's
    /// composition of it, at every pixel either shows, naming each pose that differs and its first differing pixels.
    /// </summary>
    internal static void VerifyChozoStatueWalkingPictures(IImportCartridgeSource cartridge)
    {
        Ensure.NotNull(cartridge);
        byte[] tiles = StatueTiles(cartridge);
        RgbColor[] colours = WreckedShipStatueColours(cartridge);
        SpriteSheet sheet = InstalledFixtures.EnemySheets.For(RoomEnemyGraphicsSetDefinitions.BowlingAlleyWhenAreaBossDead, EnemyDefinitionId.Chozo);
        var failures = new List<string>();
        foreach (WalkingPose pose in WalkingPoses)
        {
            Dictionary<SpritemapPoint, RgbColor> cartridgePicture = Composed(cartridge, pose.Spritemap, tiles, colours);
            AssertTrue(cartridgePicture.Count > 0, $"the cartridge's spritemap ${pose.Spritemap:X6} of {pose.Frame} shows a picture");
            string? failure = Difference(pose, cartridgePicture, sheet.Frame(EnemySheetDefinitions.FrameName(pose.Frame)));
            if (failure is not null)
                failures.Add(failure);
        }
        AssertTrue(failures.Count == 0,
            $"{failures.Count} of the walking Chozo statue's {WalkingPoses.Length} installed pictures differ from the cartridge's (x, y from the spritemap's origin):{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Chozo statue walking pictures: all {WalkingPoses.Length} installed walking poses match the cartridge's composition at every pixel.");
    }

    /// <summary>
    /// The pixels where the installed <paramref name="frame"/> and the cartridge's picture of <paramref name="pose"/> differ,
    /// as one failure line naming the pose and its first differing pixels in reading order, or null when none differs.
    /// </summary>
    private static string? Difference(WalkingPose pose, Dictionary<SpritemapPoint, RgbColor> cartridgePicture, SpriteSheetFrame frame)
    {
        SpriteImage image = frame.Sheet;
        ReadOnlySpan<RgbaPixel> pixels = image.Pixels;
        var installed = new Dictionary<SpritemapPoint, RgbColor>();
        for (int row = 0; row < frame.Size.Y; row++)
        {
            for (int column = 0; column < frame.Size.X; column++)
            {
                RgbaPixel pixel = pixels[(frame.Corner.Y + row) * image.Size.X + frame.Corner.X + column];
                if (pixel.A != 0)
                    installed.Add(new SpritemapPoint(column - frame.Origin.X, row - frame.Origin.Y), new RgbColor(pixel.R, pixel.G, pixel.B));
            }
        }
        SpritemapPoint[] differing = [.. cartridgePicture.Keys.Union(installed.Keys)
            .Where(point => !cartridgePicture.TryGetValue(point, out RgbColor shown) || !installed.TryGetValue(point, out RgbColor held) || shown != held)
            .OrderBy(point => point.Y).ThenBy(point => point.X)];
        if (differing.Length == 0)
            return null;
        IEnumerable<string> named = differing.Take(NamedDifferences).Select(point =>
            $"({point.X}, {point.Y}) cartridge {Spelled(cartridgePicture, point)}, installed {Spelled(installed, point)}");
        return $"  {pose.Frame} (spritemap ${pose.Spritemap:X6}): {differing.Length} pixels differ; first {string.Join("; ", named)}";
    }

    /// <summary>The colour <paramref name="picture"/> shows at <paramref name="point"/>, or that it shows none there.</summary>
    private static string Spelled(Dictionary<SpritemapPoint, RgbColor> picture, SpritemapPoint point) =>
        picture.TryGetValue(point, out RgbColor colour) ? $"#{colour.Red:x2}{colour.Green:x2}{colour.Blue:x2}" : "clear";

    /// <summary>
    /// The picture the cartridge composes of the spritemap at <paramref name="spritemap"/>: each visible pixel's colour by
    /// its place from the spritemap's origin. An earlier entry lies in front of a later one, as the enemy draw writes the
    /// entries to the sprite table in order and an earlier sprite shows over a later one.
    /// </summary>
    /// <exception cref="InvalidDataException">An entry shows a row the statue's initialization does not fill, or a character its upload does not hold.</exception>
    private static Dictionary<SpritemapPoint, RgbColor> Composed(IImportCartridgeSource cartridge, int spritemap, byte[] tiles, RgbColor[] colours)
    {
        var picture = new Dictionary<SpritemapPoint, RgbColor>();
        int count = cartridge.ReadCartridgeWordFixedBank(spritemap);
        for (int entry = 0; entry < count; entry++)
        {
            int at = SnesAddressMath.AddWithinBank(spritemap, sizeof(ushort) + entry * EntryByteCount);
            ushort placement = cartridge.ReadCartridgeWordFixedBank(at);
            int top = unchecked((sbyte)cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(at, sizeof(ushort))));
            ushort attributes = cartridge.ReadCartridgeWordFixedBank(SnesAddressMath.AddWithinBank(at, sizeof(ushort) + sizeof(byte)));
            int left = (placement & HorizontalOffsetBits) - ((placement & HorizontalSignBit) != 0 ? 2 * HorizontalSignBit : 0);
            int side = (placement & LargeBit) != 0 ? LargeSide : SmallSide;
            int tile = attributes & TileBits;
            int row = (attributes >> RowShift) & RowBits;
            if (row is not (BodyRow or BackLegRow))
                throw new InvalidDataException($"Spritemap ${spritemap:X6} entry {entry} shows sprite row {row}, which the statue's initialization does not fill.");
            bool mirrorX = (attributes & MirrorXBit) != 0;
            bool mirrorY = (attributes & MirrorYBit) != 0;
            for (int y = 0; y < side; y++)
            {
                for (int x = 0; x < side; x++)
                {
                    int partX = mirrorX ? side - 1 - x : x;
                    int partY = mirrorY ? side - 1 - y : y;
                    int index = ColourIndex(tiles, Character(tile, partX / SmallSide, partY / SmallSide), partX % SmallSide, partY % SmallSide,
                        $"spritemap ${spritemap:X6} entry {entry}");
                    if (index != 0)
                        picture.TryAdd(new SpritemapPoint(left + x, top + y), colours[row * RowColourCount + index]);
                }
            }
        }
        return picture;
    }

    /// <summary>
    /// The character a large part's character <paramref name="column"/> across and <paramref name="row"/> down shows: its
    /// tile number stepped within the sixteen-character rows of its half of the sprite characters, as the sprite hardware
    /// steps it.
    /// </summary>
    private static int Character(int tile, int column, int row) =>
        (tile & TileHalfBit) | (((((tile >> 4) & 0xF) + row) & 0xF) << 4) | (((tile & 0xF) + column) & 0xF);

    /// <summary>The colour index of the pixel <paramref name="x"/> across and <paramref name="y"/> down character <paramref name="character"/> shows.</summary>
    /// <exception cref="InvalidDataException">The statue's upload does not hold the character.</exception>
    private static int ColourIndex(byte[] tiles, int character, int x, int y, string shownBy)
    {
        int first = (character - FirstRecordCharacter) * CharacterByteCount;
        if (character < FirstRecordCharacter || first + CharacterByteCount > tiles.Length)
            throw new InvalidDataException($"The statue's {shownBy} shows sprite character ${character:X3}, which its tile upload does not hold.");
        int lowPlanes = first + y * 2;
        int highPlanes = first + CharacterByteCount / 2 + y * 2;
        int bit = SmallSide - 1 - x;
        return ((tiles[lowPlanes] >> bit) & 1) | (((tiles[lowPlanes + 1] >> bit) & 1) << 1)
            | (((tiles[highPlanes] >> bit) & 1) << 2) | (((tiles[highPlanes + 1] >> bit) & 1) << 3);
    }

    /// <summary>The statue's tiles, as its enemy header names them: their byte count and where they start.</summary>
    private static byte[] StatueTiles(IImportCartridgeSource cartridge)
    {
        int byteCount = cartridge.ReadCartridgeWordFixedBank(StatueHeader + HeaderTileByteCount) & TileByteCountBits;
        int address = cartridge.ReadCartridgeLongFixedBank(StatueHeader + HeaderTileAddress);
        var tiles = new byte[byteCount];
        for (int index = 0; index < byteCount; index++)
            tiles[index] = cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(address, index));
        return tiles;
    }

    /// <summary>
    /// The eight sprite rows' colours as far as the walking statue shows them: rows one and two hold the Wrecked Ship
    /// statue's colours its initialization loads; every other row stays black, and no walking pose shows one.
    /// </summary>
    private static RgbColor[] WreckedShipStatueColours(IImportCartridgeSource cartridge)
    {
        var colours = new RgbColor[RowColourCount * RowCount];
        for (int colour = 0; colour < RowColourCount; colour++)
        {
            colours[BodyRow * RowColourCount + colour] = RgbColor.FromWord(cartridge.ReadCartridgeWordFixedBank(SnesAddressMath.AddWithinBank(WreckedShipRowOneColours, colour * sizeof(ushort))));
            colours[BackLegRow * RowColourCount + colour] = RgbColor.FromWord(cartridge.ReadCartridgeWordFixedBank(SnesAddressMath.AddWithinBank(WreckedShipRowTwoColours, colour * sizeof(ushort))));
        }
        return colours;
    }

    /// <summary>Runs the walking Chozo statue's installed pictures against the cartridge's composition of them (#1642).</summary>
    [VerificationCommand("--chozo-statue-walking-pictures")]
    internal static int ChozoStatueWalkingPicturesCommand()
    {
        Suite(nameof(VerifyChozoStatueWalkingPictures), () => VerifyChozoStatueWalkingPictures(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>One walking pose and the cartridge spritemap it shows.</summary>
    /// <param name="Frame">The installed frame showing the pose.</param>
    /// <param name="Spritemap">The cartridge address of the spritemap the pose shows.</param>
    private readonly record struct WalkingPose(EnemyFrameId Frame, int Spritemap);

    /// <summary>A pixel's place from a spritemap's origin.</summary>
    /// <param name="X">Pixels right of the origin.</param>
    /// <param name="Y">Pixels below the origin.</param>
    private readonly record struct SpritemapPoint(int X, int Y);
}
