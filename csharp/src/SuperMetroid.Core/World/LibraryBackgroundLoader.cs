using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms;
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
    public static LibraryBackgroundExecutionResult BindCharacters(LibraryBackgroundProgram program, CartridgeDoorHeader activeDoor,
        LibraryBackgroundCharacterArtwork artwork)
    {
        Ensure.NotNull(program);
        Ensure.NotNull(artwork);
        bool selectsKraidHudCharacters = false;
        foreach (LibraryBackgroundInstruction instruction in program.Instructions)
        {
            switch (instruction.Command)
            {
                case LibraryBackgroundCommand.TransferToVram:
                    _ = TryBindTransferredCharacters(instruction, artwork);
                    break;
                case LibraryBackgroundCommand.TransferToVramForKraid:
                    _ = TryBindTransferredCharacters(instruction, artwork);
                    selectsKraidHudCharacters = true;
                    break;
                case LibraryBackgroundCommand.TransferForDoor:
                    if (instruction.Door == activeDoor)
                        _ = TryBindTransferredCharacters(instruction, artwork);
                    break;
                case LibraryBackgroundCommand.DecompressToWorkRam:
                case LibraryBackgroundCommand.ClearFxTilemap:
                case LibraryBackgroundCommand.ClearBg2:
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    // Tilemap commands: the background's image pictures the pages they build.
                    break;
                case LibraryBackgroundCommand.End:
                default:
                    throw new InvalidDataException(
                        $"Unknown compiled library-background command {instruction.Command} in list {program.Name}.");
            }
        }

        return new LibraryBackgroundExecutionResult(selectsKraidHudCharacters);
    }

    /// <summary>
    /// Binds the characters one of the list's transfers brings, when it brings any: the Tourian statue ghost's, or the HUD's
    /// that Kraid's list moves under BG1. Every other transfer moves tilemap words, which the background's image pictures,
    /// and binds nothing. Extraction's replay of the list binds through this same decision.
    /// </summary>
    /// <returns>Whether the transfer brought characters, which are now bound.</returns>
    /// <exception cref="InvalidOperationException">The transfer brings characters whose installed artwork is not bound.</exception>
    /// <exception cref="InvalidDataException">The statue ghost's transfer does not bring all of its characters.</exception>
    [PpuLayoutBoundary("codec: library-background character transfers' native VRAM words")]
    internal static bool TryBindTransferredCharacters(LibraryBackgroundInstruction transfer, LibraryBackgroundCharacterArtwork artwork)
    {
        Ensure.NotNull(artwork);
        if (transfer.SourceAddress == RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress)
        {
            RoomCharacterAtlasCatalog characters = artwork.CharacterArt
                ?? throw new InvalidOperationException("The Tourian statue ghost's library characters have no installed character artwork.");
            ReadOnlyMemory<byte> ghost = characters.Get(RoomCharacterSet.TourianStatueGhost).Characters;
            if (transfer.ByteCount != ghost.Length)
                throw new InvalidDataException(
                    $"Tourian statue-ghost transfer requested {transfer.ByteCount} bytes, not its {ghost.Length}.");
            artwork.Backgrounds.Bind(RoomBgArtwork.Library(transfer.SourceAddress), transfer.FirstWord, ghost.Span);
            return true;
        }
        else if (transfer.SourceAddress == HudTileAtlasFormat.SourceAddress && transfer.ByteCount == HudTileAtlasFormat.CharacterByteCount)
        {
            HudTileAtlas hud = artwork.HudArt
                ?? throw new InvalidOperationException("Kraid's library background moves the HUD's characters, which have no installed artwork.");
            artwork.Backgrounds.Bind(RoomBgArtwork.Library(transfer.SourceAddress), transfer.FirstWord,
                hud.Bytes.Span[..HudTileAtlasFormat.CharacterByteCount]);
            return true;
        }
        return false;
    }
}
