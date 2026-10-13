using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain encounter state shared across Tourian: the bank-$A9 state extension, the tube
/// HDMA deletion the elevator observes, and the sprite tile transfers that both Mother Brain and
/// the cutscene Baby request.
/// </summary>
internal sealed class MotherBrainEncounter
{
    /// <summary>VRAM and sprite presentation owner receiving encounter tile transfers.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Room services supplying installed artwork and mutable memory for native transfers.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Binds the encounter's sprite transfers to presentation and room asset owners.</summary>
    internal MotherBrainEncounter(EnemyPresentation presentation, EnemyRoomBinding room)
    {
        _presentation = presentation;
        _room = room;
    }

    /// <summary>The typed multipart encounter while Mother Brain's retail population is loaded.</summary>
    public MotherBrainEnemyState? MotherBrain { get; internal set; }

    /// <summary>
    /// Set on the frame the main tube lands (`$A9:8C0C`), which deletes HDMA objects 0-3.
    /// The runtime applies it to the HDMA owners; cleared at the start of each enemy frame.
    /// </summary>
    public bool MotherBrainDeletedHdmaObjects { get; internal set; }

    /// <summary>Loads known cartridge artwork from installed assets and preserves mutable WRAM DMA for the corpse buffer.</summary>
    [PpuLayoutBoundary("codec: Mother Brain sprite tile transfer requests' native VRAM destinations")]
    internal void ApplyMotherBrainRainbowTileTransfer(
        MotherBrainSpriteTileTransferRequest transfer)
    {
        if (_room.Artwork is not null &&
            MotherBrainSpecialSpriteArtworkDefinitions.TryForSource(
                transfer.SourceAddress, out MotherBrainSpecialSpriteSheetDefinition sheet))
        {
            int page = sheet.ResolvePage(
                transfer.SourceAddress, transfer.Size, transfer.VramDestination);
            MotherBrainSpecialSpriteArtworkCatalog artwork = _room.Artwork.MotherBrainSpecialSprites ??
                throw new InvalidDataException(
                    $"Installed enemy artwork is missing {sheet.FileName}.");
            _presentation.Vram.LoadBytes(transfer.VramDestination * 2,
                artwork.Get(sheet.SourceAddress).Transfer.Span.Slice(
                    page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                    transfer.Size));
            return;
        }
        if (MotherBrainEscapeTextArtworkDefinitions.ContainsSource(transfer.SourceAddress) &&
            _room.Artwork is not null)
        {
            int page = -1;
            for (int index = 0; index < MotherBrainEscapeTextArtworkDefinitions.PageCount;
                 index++)
            {
                if (transfer.SourceAddress ==
                    MotherBrainEscapeTextArtworkDefinitions.PageSource(index))
                {
                    page = index;
                    break;
                }
            }
            if (page < 0 ||
                transfer.Size != MotherBrainEscapeTextArtworkDefinitions.PageByteCount(page) ||
                transfer.VramDestination !=
                    MotherBrainEscapeTextArtworkDefinitions.PageDestination(page))
                throw new InvalidDataException(
                    $"Invalid Mother Brain escape-text sprite transfer {transfer}.");

            RoomCharacterAtlas artwork = _room.Artwork.MotherBrainEscapeText ??
                throw new InvalidDataException(
                    "Installed enemy artwork is missing mother-brain-escape-text-tiles.png.");
            int offset = checked((int)transfer.SourceAddress -
                MotherBrainEscapeTextArtworkDefinitions.SourceAddress);
            _presentation.Vram.LoadBytes(transfer.VramDestination * 2,
                artwork.Transfer.Span.Slice(offset, transfer.Size));
            return;
        }
        if (MotherBrainCorpseArtworkDefinitions.ContainsSource(transfer.SourceAddress) &&
            _room.Artwork is not null)
        {
            // The native list supplies both the source page and its OBJ destination.
            // Keep that scheduling intact, but source the transferred pixels from the
            // installed sheet instead of making a visual read from cartridge ROM.
            int page = -1;
            for (int index = 0; index < MotherBrainCorpseArtworkDefinitions.RowCount;
                 index++)
            {
                if (transfer.SourceAddress ==
                    MotherBrainCorpseArtworkDefinitions.VramPageSource(index))
                {
                    page = index;
                    break;
                }
            }
            if (page < 0 ||
                transfer.Size != MotherBrainCorpseArtworkDefinitions.VramPageByteCount ||
                transfer.VramDestination !=
                    MotherBrainCorpseArtworkDefinitions.VramPageDestination(page))
                throw new InvalidDataException(
                    $"Invalid Mother Brain corpse sprite transfer {transfer}.");

            RoomCharacterAtlas artwork = _room.MotherBrainCorpseArtwork;
            int offset = checked((int)transfer.SourceAddress -
                MotherBrainCorpseArtworkDefinitions.SourceAddress);
            _presentation.Vram.LoadBytes(transfer.VramDestination * 2,
                artwork.Transfer.Span.Slice(offset, transfer.Size));
            return;
        }
        if (_presentation.EscapeTimerArtwork?.TryLoadNativeTransfer(
            _presentation.Vram, unchecked((int)transfer.SourceAddress), transfer.Size, transfer.VramDestination) == true)
            return;
        if (SnesAddressMath.BankNumber(unchecked((int)transfer.SourceAddress)) is not (126 or 127)) // Banks $7E/$7F: WRAM.
        {
            // Every cartridge-backed source used by this encounter has an installed
            // art owner above. The remaining live transfer is the mutable $7E:9000
            // corpse buffer; an unknown ROM source must never silently reintroduce
            // a cartridge dependency in an installed game.
            throw new InvalidDataException(
                $"Mother Brain sprite transfer {transfer} has no installed artwork binding.");
        }
        // The live $7E:9000 corpse buffer reaches VRAM through the ordinary mode-1 DMA.
        _presentation.Vram.ExecuteQueuedMemoryWrite(
            SnesMemoryBusRequirement.Require(_room.LoadedBus, "Mother Brain's mutable sprite transfer requires WRAM."),
            unchecked((int)transfer.SourceAddress), transfer.Size, transfer.VramDestination);
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (MotherBrain?.LastSoundEffect is ushort motherBrainLibrary2)
            audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, motherBrainLibrary2), maximumQueued: 6);
        if (MotherBrain?.LastSoundEffectLibrary1 is ushort motherBrainLibrary1)
            audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, motherBrainLibrary1), maximumQueued: 6);
        if (MotherBrain?.LastSoundEffectLibrary3 is ushort motherBrainLibrary3)
        {
            // Explosion instruction $13 is QueueSfx3_Max3; the other translated library-
            // three Mother Brain instructions use Max6.
            byte maximumQueued = motherBrainLibrary3 == MotherBrainSoundDefinitions.Explosion.Value ? (byte)3 : (byte)6;
            audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, motherBrainLibrary3), maximumQueued);
        }
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        if (MotherBrain is not null)
        {
            foreach (MotherBrainMusicRequest request in MotherBrain.MusicRequests)
            {
                audio.QueueMusic(new EnemyMusicRequest(request.Command, request.Delay));
            }
        }
    }
}
