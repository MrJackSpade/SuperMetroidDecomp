using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World;

/// <summary>
/// Applies a room state's library background as its room loads (#1406). BG2 shows the background's pre-assembled image,
/// which the room scope selects by the background's name and the door it was entered through; the tilemap pages the list
/// builds are pictured in that image, so the load runs none of its tilemap commands. What remains is the character artwork
/// the background brings, bound for the room's pictures to resolve against, and whether it moves the HUD's characters to
/// Kraid's base.
/// </summary>
public static class LibraryBackgroundLoader
{
    /// <summary>
    /// Binds the characters <paramref name="program"/> brings when its room is entered through <paramref name="activeDoor"/>,
    /// and reports whether it moves the HUD's characters to Kraid's base.
    /// </summary>
    /// <exception cref="InvalidDataException">The program holds a command the cartridge's interpreter does not accept.</exception>
    /// <exception cref="InvalidOperationException">The program brings characters whose installed artwork is not bound.</exception>
    public static LibraryBackgroundExecutionResult BindCharacters(LibraryBackgroundProgram program, DoorDefinition activeDoor,
        LibraryBackgroundCharacterArtwork artwork)
    {
        Ensure.NotNull(program);
        Ensure.NotNull(artwork);
        bool selectsKraidHudCharacters = false;
        foreach (LibraryBackgroundInstruction instruction in program.Instructions)
        {
            switch (instruction.Command)
            {
                case LibraryBackgroundCommand.Transfer:
                    _ = TryBindTransferredCharacters(instruction, artwork);
                    break;
                case LibraryBackgroundCommand.TransferForKraid:
                    _ = TryBindTransferredCharacters(instruction, artwork);
                    selectsKraidHudCharacters = true;
                    break;
                case LibraryBackgroundCommand.TransferForDoor:
                    if (instruction.Door == activeDoor)
                        _ = TryBindTransferredCharacters(instruction, artwork);
                    break;
                case LibraryBackgroundCommand.Decompress:
                case LibraryBackgroundCommand.ClearFxTilemap:
                case LibraryBackgroundCommand.ClearBg2:
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    // Tilemap commands: the background's image pictures the pages they build.
                    break;
                default:
                    throw new InvalidDataException(
                        $"Unknown compiled library-background command {instruction.Command} in list {program.Name}.");
            }
        }

        return new LibraryBackgroundExecutionResult(selectsKraidHudCharacters);
    }

    /// <summary>
    /// Binds the characters one of the list's transfers brings, when it brings any (#1406): the HUD's, which Kraid's list
    /// copies for layer 3 to read while his body covers the standard ones. The Tourian statue ghost's characters reach the
    /// sprite characters, which only the statue ghost would draw, and it draws nothing, so they bind nowhere. Every other
    /// transfer moves tilemap words, which the background's image pictures, and binds nothing. Extraction's replay of the
    /// list binds through this same decision.
    /// </summary>
    /// <returns>Whether the transfer brought characters.</returns>
    /// <exception cref="InvalidOperationException">The transfer brings characters whose installed artwork is not bound.</exception>
    /// <exception cref="InvalidDataException">
    /// The statue ghost's transfer does not bring all of its characters, or a list other than Kraid's moves the HUD's.
    /// </exception>
    internal static bool TryBindTransferredCharacters(LibraryBackgroundInstruction transfer, LibraryBackgroundCharacterArtwork artwork)
    {
        Ensure.NotNull(artwork);
        if (transfer.Source == LibraryBackgroundSource.TourianStatueGhostCharacters)
        {
            RoomCharacterAtlases characters = artwork.CharacterArt
                ?? throw new InvalidOperationException("The Tourian statue ghost's library characters have no installed character artwork.");
            ReadOnlyMemory<byte> ghost = characters.Get(RoomCharacterSet.TourianStatueGhost).Characters;
            if (transfer.ByteCount != ghost.Length)
                throw new InvalidDataException(
                    $"Tourian statue-ghost transfer requested {transfer.ByteCount} bytes, not its {ghost.Length}.");
            return true;
        }
        else if (transfer.Source == LibraryBackgroundSource.HudCharacters && transfer.ByteCount == HudTileAtlasFormat.CharacterByteCount)
        {
            if (transfer.Command != LibraryBackgroundCommand.TransferForKraid)
                throw new InvalidDataException($"A library background's {transfer.Command} moves the HUD's characters, which only Kraid's list copies.");
            if (artwork.HudArt is null)
                throw new InvalidOperationException("Kraid's library background moves the HUD's characters, which have no installed artwork.");
            artwork.Backgrounds.Bind(RoomBgArtwork.Library(transfer.Source), BgCharacterSet.KraidLayer3, 0,
                BgCharacterSet.KraidLayer3.CharactersIn(HudTileAtlasFormat.CharacterByteCount));
            return true;
        }
        return false;
    }
}
