using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Rooms.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Areas.Ceres.Artwork;

/// <summary>
/// Checks the Ceres doors' rotating overlay picture against the cartridge (#1641). Only the elevator shaft's pre-explosion
/// overlay door (enemy parameter 2, $A6:F610) draws the frame ($A6:F921), and its initialization copies the overlay's
/// characters over the sprite characters from word $7000 ($A6:F739) before the frame first draws; in Ceres Ridley's arena
/// those characters are Ridley's own, which the frame is never drawn over. The expected characters are the cartridge's
/// room load and copy alone, never the import's composition.
/// </summary>
internal static class CeresDoorOverlayArtworkVerification
{
    /// <summary>The elevator shaft's state before Ridley's escape ($8F:DF57), which places the overlay door.</summary>
    private const int ShaftDefaultState = 0x8FDF57;

    /// <summary>Ceres Ridley's arena's state before his escape ($8F:E0C7), which places a door that never draws the overlay.</summary>
    private const int ArenaDefaultState = 0x8FE0C7;

    /// <summary>The copy's <c>LDA #</c> of its byte count ($A6:F744).</summary>
    private const int CopySizeLoad = 0xA6F744;

    /// <summary>The copy's <c>LDA #</c> of its source's top two bytes, stored one byte into the source ($A6:F74A).</summary>
    private const int CopySourceBankLoad = 0xA6F74A;

    /// <summary>The copy's <c>LDA #</c> of its source's bottom two bytes ($A6:F750).</summary>
    private const int CopySourceLoad = 0xA6F750;

    /// <summary>The copy's <c>LDA #</c> of its destination word ($A6:F756).</summary>
    private const int CopyDestinationLoad = 0xA6F756;

    /// <summary>The sprite characters per row of the sprites' character table, which a larger part's rows step by.</summary>
    private const int CharactersPerRow = 16;

    /// <summary>Pixels along a character's side.</summary>
    private const int CharacterPixels = 8;

    /// <summary>The words one four-colour-plane character spans.</summary>
    private const int CharacterWords = 16;

    /// <summary>
    /// In the elevator shaft's graphics set, which draws the frame, and in Ceres Ridley's arena's, the Ceres doors' sheet
    /// pictures the rotating overlay once, named for no look, over the characters the cartridge's load and the overlay door's
    /// copy leave: every character it reads is the door's own tiles or the overlay's, never Ridley's or another room's load.
    /// </summary>
    internal static void VerifyRotatingOverlayShowsTheOverlaysCharacters()
    {
        IImportCartridgeSource cartridge = CeresRidleyArenaBlocksVerification.Cartridge();
        int size = ImmediateWord(cartridge, CopySizeLoad);
        int source = (ImmediateWord(cartridge, CopySourceBankLoad) >> 8) << 16 | ImmediateWord(cartridge, CopySourceLoad);
        int destination = ImmediateWord(cartridge, CopyDestinationLoad);
        byte[] overlay = RomDataReader.ReadFixedBank(cartridge, source, size);
        string frame = EnemySheetDefinitions.FrameName(EnemyFrameId.CeresDoorRotatingOverlay);
        (int Header, RoomStateSettings State)[] states =
            [(ShaftDefaultState, CeresRoomStateDefinitions.CeresElevDefault), (ArenaDefaultState, CeresRoomStateDefinitions.CeresRidleyDefault)];
        foreach ((int header, RoomStateSettings state) in states)
        {
            CeresRidleyArenaBlocksVerification.LoadedSpriteCharacters shown = CeresRidleyArenaBlocksVerification.LoadedSpriteCharacters.Of(cartridge, header);
            // The overlay door's initialization copies the overlay's characters before the frame first draws.
            shown.Copy(destination, overlay);
            EnemySheetSource door = EnemySheetComposer.Sources(EnemySheetFixtures.Inputs, RoomEnemyGraphicsSetLists.Of(state)).Enemies
                .Single(sheet => sheet.Enemy == EnemyDefinitionId.CeresDoor);
            SheetFrameSource[] pictures = [.. door.Sources.Frames.Where(picture =>
                picture.Name == frame || picture.Name.StartsWith(frame + "|", StringComparison.Ordinal))];
            AssertEqual(1, pictures.Length, $"{state.Name}: the Ceres doors' sheet pictures the rotating overlay once, in the one look it is drawn in");
            AssertEqual(frame, pictures[0].Name, $"{state.Name}: the rotating overlay's picture is named for no look, as no look the doors show changes it");
            AssertTrue(!door.LookChanges.ContainsKey(frame), $"{state.Name}: no look change names the rotating overlay");
            ReadOnlySpan<byte> composed = pictures[0].Characters.Characters;
            foreach (SpriteFramePart part in pictures[0].Frame.Parts)
            {
                for (int row = 0; row < part.Size.Y / CharacterPixels; row++)
                {
                    for (int column = 0; column < part.Size.X / CharacterPixels; column++)
                    {
                        int character = part.Character + row * CharactersPerRow + column;
                        int word = CeresRidleyArenaBlocksVerification.SpriteCharactersFirstWord + character * CharacterWords;
                        AssertTrue(shown.Holds(word),
                            $"{state.Name}: the rotating overlay reads character {character:X3}, which neither the room's load nor the overlay's copy fills");
                        int first = character * CharacterWords * sizeof(ushort);
                        AssertTrue(composed.Slice(first, CharacterWords * sizeof(ushort)).SequenceEqual(shown.Bytes.Slice(first, CharacterWords * sizeof(ushort))),
                            $"{state.Name}: the rotating overlay's character {character:X3} is not the one the cartridge's load and the overlay's copy leave");
                    }
                }
            }
        }
        Console.WriteLine("  Ceres door overlay: the rotating overlay shows the door's and the overlay's own characters, in the shaft and in Ridley's arena.");
    }

    /// <summary>The word operand of the <c>LDA #</c> at <paramref name="instruction"/>.</summary>
    private static int ImmediateWord(IImportCartridgeSource cartridge, int instruction)
    {
        AssertEqual(CeresRidleyArenaBlocksVerification.LoadImmediate, cartridge.ReadCartridgeByte(instruction),
            $"the overlay door's copy loads a value at ${instruction:X6}");
        return RomDataReader.ReadWordFixedBank(cartridge, instruction + 1);
    }

    /// <summary>Runs the Ceres door overlay check.</summary>
    [VerificationCommand("--ceres-door-overlay-artwork")]
    internal static int CeresDoorOverlayArtworkCommand()
    {
        Suite(nameof(VerifyRotatingOverlayShowsTheOverlaysCharacters), VerifyRotatingOverlayShowsTheOverlaysCharacters);
        return 0;
    }
}
