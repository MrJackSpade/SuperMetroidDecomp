using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Checks Ceres Ridley's arena's block pictures against the cartridge (#1648). Both states of the arena ($8F:E0B5) run the
/// setup $8F:C97B, which points BG1 and BG2 at the characters from word $6000, the sprites' characters, before the room's
/// first frame; the tileset's own characters, its Mode 7 floor at word 0, are never under the arena's blocks. The room's
/// load ($82:DFD1) leaves the Ceres door's tiles at word $6D00, which hold every character the arena's blocks read
/// ($D4-$FF), and nothing the arena runs copies over them while it shows. The expected pictures are built from the
/// cartridge's own tables and that load alone, never from the import's composition.
/// </summary>
internal static class CeresRidleyArenaBlocksVerification
{
    /// <summary>The arena's state before Ridley's escape ($8F:E0C7).</summary>
    private const int ArenaDefaultState = 0x8FE0C7;

    /// <summary>The arena's state once Ridley has escaped ($8F:E0E1).</summary>
    private const int ArenaEscapedState = 0x8FE0E1;

    /// <summary>The byte of a state header holding its tileset.</summary>
    private const int StateTilesetOffset = 3;

    /// <summary>The bytes of a state header from its start to its enemy set's pointer.</summary>
    private const int StateEnemySetOffset = 0x0A;

    /// <summary>The bytes of a state header from its start to its setup routine's pointer.</summary>
    private const int StateSetupOffset = 0x18;

    /// <summary>The setup the arena's states run: it stores BG1's and BG2's character bases ($8F:C97B).</summary>
    private const int ArenaSetup = 0xC97B;

    /// <summary>The bank of the room setup routines and the state headers that point at them.</summary>
    private const int RoomBank = 0x8F;

    /// <summary>The arena's setup routine ($8F:C97B), which begins <c>PHP</c>, <c>SEP #$20</c>: its accumulator is one byte.</summary>
    private const int ArenaSetupRoutine = RoomBank << 16 | ArenaSetup;

    /// <summary>Where in the arena's setup its <c>LDA #</c> of the two character bases lies ($8F:C97E), after those two instructions.</summary>
    private const int ArenaSetupBaseLoad = ArenaSetupRoutine + 3;

    /// <summary>The 65816's <c>PHP</c> opcode.</summary>
    private const byte PushStatus = 0x08;

    /// <summary>The 65816's <c>SEP #</c> opcode.</summary>
    private const byte SetStatusBits = 0xE2;

    /// <summary>The status bit <c>SEP #</c> sets to make the accumulator one byte.</summary>
    private const byte ByteAccumulator = 0x20;

    /// <summary>The direct-page byte the setup stores the two bases into, which the vertical blank copies to BG12NBA.</summary>
    private const int CharacterBasesDirectPage = 0x5D;

    /// <summary>The 65816's <c>LDA #</c> opcode.</summary>
    internal const byte LoadImmediate = 0xA9;

    /// <summary>The 65816's <c>STA dp</c> opcode.</summary>
    private const byte StoreDirectPage = 0x85;

    /// <summary>The words one character base step spans.</summary>
    private const int CharacterBaseStepWords = 0x1000;

    /// <summary>The bank of the enemy sets.</summary>
    private const int EnemySetBank = 0xB4;

    /// <summary>The bank of the enemy headers.</summary>
    private const int EnemyHeaderBank = 0xA0;

    /// <summary>The bytes of an enemy header from its start to its tiles' long pointer ($82:DFE6).</summary>
    private const int HeaderTilesOffset = 0x36;

    /// <summary>The word that ends an enemy set.</summary>
    private const int EnemySetEnd = 0xFFFF;

    /// <summary>The bit of an enemy's tile size that places its tiles at the block its set entry names ($82:DFF8).</summary>
    private const int FixedBlockFlag = 0x8000;

    /// <summary>Where the door transition's load stacks the first enemy's tiles that take no fixed block ($82:DFD1).</summary>
    private const int StackedTilesFirstWord = 0x7000;

    /// <summary>The first word of the sprites' characters.</summary>
    internal const int SpriteCharactersFirstWord = 0x6000;

    /// <summary>The words of the sprites' characters, through the end of the video memory.</summary>
    private const int SpriteCharacterWords = 0x2000;

    /// <summary>The words one four-colour-plane character spans.</summary>
    private const int CharacterWords = 16;

    /// <summary>Tiles a block's picture paints: top left, top right, bottom left, bottom right.</summary>
    private const int BlockTiles = 4;

    /// <summary>Pixels along a tile's side.</summary>
    private const int TilePixels = 8;

    /// <summary>The colours of one background palette row.</summary>
    private const int RowColours = 16;

    /// <summary>
    /// Every block the arena's level places, in either layer, shows in its installed picture over the sprites' artwork
    /// exactly its tiles, from the cartridge's tile table, drawn over the characters the cartridge's room load leaves from
    /// word $6000: each pixel the palette index its tile paints, in the part its tile's priority puts it in. No block depends
    /// on any appearance input there: nothing the arena copies while it shows lands on the characters its blocks read.
    /// </summary>
    internal static void VerifyArenaBlocksShowLoadedSpriteCharacters()
    {
        IImportCartridgeSource cartridge = Cartridge();
        AssertEqual(SpriteCharactersFirstWord, ArenaCharacterBase(cartridge),
            "the arena's setup points BG1 and BG2 at the sprites' characters");
        TilesetDefinition tileset = RoomTilesets.Get(RoomGraphicsSetCodec.Decode(cartridge.ReadCartridgeByte(ArenaDefaultState + StateTilesetOffset)));
        RoomBlockSheet sheet = InstalledFixtures.RoomAssets.BlockSheets.Sheets[tileset];
        byte[] table = RomDataReader.Decompress(cartridge, RoomTilesetSources.Read(cartridge).Blocks[tileset.Blocks]);
        LoadedSpriteCharacters loaded = LoadedSpriteCharacters.Of(cartridge, ArenaDefaultState);
        int[] placed = PlacedBlocks(cartridge, ArenaDefaultState);
        AssertTrue(placed.Length > 0, "the arena's level places blocks");
        foreach (int block in placed)
        {
            string where = $"{RoomBlockSheet.NameOf(tileset)} block {block:X3}";
            AssertTrue(sheet.Inputs.Shows(block), $"{where}: the installed sheet shows the block the arena's level places");
            ImmutableArray<AppearanceInput> inputs = sheet.Inputs.Of(block, RoomBlockArtwork.Sprites);
            AssertTrue(inputs.IsEmpty, $"{where} depends on {string.Join(", ", inputs.Select(input => input.Token))}, though nothing the arena copies lands on its characters");
            LayeredPicture picture = sheet.BlockPicture(block, RoomBlockArtwork.Sprites, inputs, []);
            for (int tile = 0; tile < BlockTiles; tile++)
            {
                int entry = (block * BlockTiles + tile) * sizeof(ushort);
                var word = new SnesBgTilemapWord((ushort)(table[entry] | table[entry + 1] << 8));
                int character = SpriteCharactersFirstWord + word.CharacterIndex * CharacterWords;
                AssertTrue(character + CharacterWords <= SpriteCharactersFirstWord + SpriteCharacterWords && loaded.Holds(character),
                    $"{where}: tile {tile} reads character {word.CharacterIndex:X3}, which the arena's load fills among the sprites' characters");
                for (int y = 0; y < TilePixels; y++)
                {
                    for (int x = 0; x < TilePixels; x++)
                    {
                        int colour = loaded.ColourAt(character, word.FlipHorizontally ? TilePixels - 1 - x : x, word.FlipVertically ? TilePixels - 1 - y : y);
                        int expected = colour == 0 ? 0 : word.PaletteIndex * RowColours + colour;
                        var pixel = new Position16<ImagePixels>(tile % 2 * TilePixels + x, tile / 2 * TilePixels + y);
                        if (!PaintsInPart(picture, pixel, expected, word.HasPriority))
                            AssertTrue(false, $"{where}: tile {tile} (${word.Raw:X4}) pixel ({x}, {y}) is not palette index {expected} {(word.HasPriority ? "in front" : "behind")}");
                    }
                }
            }
        }
        Console.WriteLine($"  Ceres Ridley's arena: {placed.Length} placed blocks show the sprites' characters its load leaves.");
    }

    /// <summary>
    /// Neither of the arena's tilesets pictures a block over its own artwork: every state that loads them runs the setup that
    /// points the blocks at the sprites' characters before the room's first frame, so no picture over the tileset's characters
    /// is ever shown, and none is installed or recorded.
    /// </summary>
    internal static void VerifyArenaPicturesNoBlockOverItsTileset()
    {
        IImportCartridgeSource cartridge = Cartridge();
        AssertEqual(SpriteCharactersFirstWord, ArenaCharacterBase(cartridge),
            "the arena's setup points BG1 and BG2 at the sprites' characters");
        foreach (int state in (int[])[ArenaDefaultState, ArenaEscapedState])
        {
            AssertEqual(ArenaSetup, RomDataReader.ReadWordFixedBank(cartridge, state + StateSetupOffset),
                $"arena state ${state:X6} runs the setup that points its blocks at the sprites' characters");
            TilesetDefinition tileset = RoomTilesets.Get(RoomGraphicsSetCodec.Decode(cartridge.ReadCartridgeByte(state + StateTilesetOffset)));
            RoomBlockSheet sheet = InstalledFixtures.RoomAssets.BlockSheets.Sheets[tileset];
            string name = RoomBlockSheet.NameOf(tileset);
            AssertTrue(!sheet.Inputs.ShowsOver(RoomBlockArtwork.Tileset), $"{name} records its blocks over the tileset's artwork, which its rooms never show");
            AssertTrue(sheet.Inputs.ShowsOver(RoomBlockArtwork.Sprites), $"{name} records its blocks over the sprites' artwork, which its rooms show");
            for (int block = 0; block < sheet.Inputs.Count; block++)
            {
                if (!sheet.Inputs.Shows(block))
                    continue;
                string over = RoomBlockPictureNames.Of(block, RoomBlockArtwork.Tileset, [], []);
                AssertTrue(!PictureParts.Holds(sheet.Sheet, over), $"{name} holds picture {over} of block {block:X3} over the tileset's characters, which no room shows");
            }
        }
        Console.WriteLine("  Ceres Ridley's arena: neither tileset pictures a block over its own characters.");
    }

    /// <summary>The retail cartridge.</summary>
    internal static IImportCartridgeSource Cartridge() =>
        CartridgeImportSource.Require(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    /// <summary>
    /// The word BG1's and BG2's characters begin at in the arena, decoded from the setup's own instructions: it loads the two
    /// bases as one byte, BG1's in its low four bits and BG2's in its high four, and stores them where the vertical blank
    /// copies them from.
    /// </summary>
    private static int ArenaCharacterBase(IImportCartridgeSource cartridge)
    {
        AssertEqual(PushStatus, cartridge.ReadCartridgeByte(ArenaSetupRoutine), "the arena's setup first saves the processor status");
        AssertEqual(SetStatusBits, cartridge.ReadCartridgeByte(ArenaSetupRoutine + 1), "the arena's setup then sets status bits");
        AssertEqual(ByteAccumulator, cartridge.ReadCartridgeByte(ArenaSetupRoutine + 2), "the arena's setup makes its accumulator one byte");
        AssertEqual(LoadImmediate, cartridge.ReadCartridgeByte(ArenaSetupBaseLoad), "the arena's setup loads its character bases as a value");
        AssertEqual(StoreDirectPage, cartridge.ReadCartridgeByte(ArenaSetupBaseLoad + 2), "the arena's setup stores its character bases in the direct page");
        AssertEqual(CharacterBasesDirectPage, cartridge.ReadCartridgeByte(ArenaSetupBaseLoad + 3), "the arena's setup stores the bases the vertical blank copies");
        int bases = cartridge.ReadCartridgeByte(ArenaSetupBaseLoad + 1);
        AssertEqual(bases & 0x0F, bases >> 4, "the arena's setup gives BG1 and BG2 one character base");
        return (bases & 0x0F) * CharacterBaseStepWords;
    }

    /// <summary>The distinct blocks the level of the state at <paramref name="state"/> places in either layer, ascending.</summary>
    private static int[] PlacedBlocks(IImportCartridgeSource cartridge, int state)
    {
        byte[] level = RomDataReader.Decompress(cartridge, RomDataReader.ReadLongFixedBank(cartridge, state));
        int foregroundBytes = level[0] | level[1] << 8;
        int foreground = sizeof(ushort);
        int background = foreground + foregroundBytes + foregroundBytes / 2;
        var blocks = new SortedSet<int>();
        for (int word = foreground; word < foreground + foregroundBytes; word += sizeof(ushort))
            blocks.Add((level[word] | level[word + 1] << 8) & 0x3FF);
        for (int word = background; word + 1 < level.Length; word += sizeof(ushort))
            blocks.Add((level[word] | level[word + 1] << 8) & 0x3FF);
        return [.. blocks];
    }

    /// <summary>
    /// Whether <paramref name="picture"/> paints, at <paramref name="pixel"/>, what a tile in front when
    /// <paramref name="inFront"/> and behind otherwise paints there as palette index <paramref name="expected"/> (#1634): the
    /// part of the tile's depth shows the index's colour and the other part nothing; where the tile paints nothing, neither
    /// part does.
    /// </summary>
    private static bool PaintsInPart(LayeredPicture picture, Position16<ImagePixels> pixel, int expected, bool inFront)
    {
        SpriteSheetFrame? shown = inFront ? picture.InFront : picture.Behind;
        SpriteSheetFrame? other = inFront ? picture.Behind : picture.InFront;
        if (expected == 0)
            return PaintsNothing(picture.Behind, pixel) && PaintsNothing(picture.InFront, pixel);
        if (shown is null)
            return false;
        int at = At(shown, pixel);
        return shown.Sheet.Pixels[at].A != 0 && InstalledPictureColoursConstants.KeyComposedAs(shown.Sheet, expected) is { } key
            && shown.Sheet.KeyAt(at) == key && PaintsNothing(other, pixel);
    }

    /// <summary>Whether <paramref name="part"/> shows nothing at <paramref name="pixel"/>: it is not installed, or its pixel there is clear.</summary>
    private static bool PaintsNothing(SpriteSheetFrame? part, Position16<ImagePixels> pixel) =>
        part is null || part.Sheet.Pixels[At(part, pixel)].A == 0;

    /// <summary>The index in its sheet's pixels of <paramref name="pixel"/> of <paramref name="part"/>.</summary>
    private static int At(SpriteSheetFrame part, Position16<ImagePixels> pixel) =>
        (part.Corner.Y + pixel.Y) * part.Sheet.Size.X + part.Corner.X + pixel.X;

    /// <summary>Runs the Ceres Ridley arena block checks.</summary>
    [VerificationCommand("--ceres-ridley-arena-blocks")]
    internal static int CeresRidleyArenaBlocksCommand()
    {
        Suite(nameof(VerifyArenaBlocksShowLoadedSpriteCharacters), VerifyArenaBlocksShowLoadedSpriteCharacters);
        Suite(nameof(VerifyArenaPicturesNoBlockOverItsTileset), VerifyArenaPicturesNoBlockOverItsTileset);
        return 0;
    }

    /// <summary>
    /// The sprites' characters, words $6000-$7FFF, as a door transition's load into a room state leaves the parts its enemy
    /// set fills ($82:DFD1): each member in set order, its tiles stacked from word $7000, or, for a member whose tile size
    /// carries the fixed-block flag, at word $6000 plus the block its set entry's top four bits name, $100 words each. Only
    /// the characters the load fills are known; every other word holds what earlier rooms left.
    /// </summary>
    internal sealed class LoadedSpriteCharacters
    {
        /// <summary>The bytes of words $6000-$7FFF.</summary>
        private readonly byte[] bytes = new byte[SpriteCharacterWords * sizeof(ushort)];

        /// <summary>Whether the load fills each word.</summary>
        private readonly bool[] filled = new bool[SpriteCharacterWords];

        /// <summary>The bytes of words $6000-$7FFF, through the load's characters.</summary>
        internal ReadOnlySpan<byte> Bytes => bytes;

        /// <summary>The characters the load of the state whose header lies at <paramref name="state"/> fills.</summary>
        /// <exception cref="InvalidDataException">A member's tiles would leave the sprites' characters.</exception>
        internal static LoadedSpriteCharacters Of(IImportCartridgeSource cartridge, int state)
        {
            var loaded = new LoadedSpriteCharacters();
            int set = EnemySetBank << 16 | RomDataReader.ReadWordFixedBank(cartridge, state + StateEnemySetOffset);
            int stacked = StackedTilesFirstWord;
            for (int entry = set; ; entry += 2 * sizeof(ushort))
            {
                int header = RomDataReader.ReadWordFixedBank(cartridge, entry);
                if (header == EnemySetEnd)
                    return loaded;
                int placement = RomDataReader.ReadWordFixedBank(cartridge, entry + sizeof(ushort));
                int size = RomDataReader.ReadWordFixedBank(cartridge, EnemyHeaderBank << 16 | header);
                int tiles = RomDataReader.ReadLongFixedBank(cartridge, (EnemyHeaderBank << 16 | header) + HeaderTilesOffset);
                int destination;
                if ((size & FixedBlockFlag) == 0)
                {
                    destination = stacked;
                    stacked += size / sizeof(ushort);
                }
                else
                {
                    size &= ~FixedBlockFlag;
                    destination = SpriteCharactersFirstWord | (placement & 0xF000) >> 4;
                }
                loaded.Copy(destination, RomDataReader.ReadFixedBank(cartridge, tiles, size));
            }
        }

        /// <summary>Copies <paramref name="source"/> over the words from <paramref name="destination"/>, as the load's transfer does.</summary>
        /// <exception cref="InvalidDataException">The copy would leave the sprites' characters.</exception>
        internal void Copy(int destination, ReadOnlySpan<byte> source)
        {
            int first = destination - SpriteCharactersFirstWord;
            if (first < 0 || first + source.Length / sizeof(ushort) > SpriteCharacterWords)
                throw new InvalidDataException($"A copy of {source.Length} bytes to word ${destination:X4} leaves the sprites' characters.");
            source.CopyTo(bytes.AsSpan(first * sizeof(ushort)));
            filled.AsSpan(first, source.Length / sizeof(ushort)).Fill(true);
        }

        /// <summary>Whether the load fills the whole character that begins at word <paramref name="character"/>.</summary>
        internal bool Holds(int character) =>
            !filled.AsSpan(character - SpriteCharactersFirstWord, CharacterWords).Contains(false);

        /// <summary>The colour, within its palette row, of pixel (<paramref name="x"/>, <paramref name="y"/>) of the character at word <paramref name="character"/>.</summary>
        internal int ColourAt(int character, int x, int y)
        {
            int row = (character - SpriteCharactersFirstWord) * sizeof(ushort) + y * sizeof(ushort);
            int bit = 7 - x;
            return (bytes[row] >> bit & 1) | (bytes[row + 1] >> bit & 1) << 1
                | (bytes[row + 16] >> bit & 1) << 2 | (bytes[row + 17] >> bit & 1) << 3;
        }
    }
}
