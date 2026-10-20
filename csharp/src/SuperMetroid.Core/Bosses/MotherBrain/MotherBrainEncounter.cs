using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain encounter state shared across Tourian: the bank-$A9 state extension, the tube
/// HDMA deletion the elevator observes, and the sprite tile transfers that both Mother Brain and
/// the cutscene Baby request.
/// </summary>
internal sealed class MotherBrainEncounter
{
    /// <summary>Sprite presentation owner whose OBJ character table shows encounter tile transfers.</summary>
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
    /// Shows a sprite tile transfer in the OBJ character table: known cartridge artwork as its installed page, and the
    /// mutable WRAM corpse buffer as the bytes it holds.
    /// </summary>
    [PpuLayoutBoundary("codec: Mother Brain sprite tile transfer requests' native VRAM destinations")]
    internal void ApplyMotherBrainRainbowTileTransfer(
        MotherBrainSpriteTileTransferRequest transfer)
    {
        VramDmaDestination destination = VramDmaDestination.At(GameplayVramLayout.ObjectCharacters,
            transfer.VramDestination - GameplayVramLayout.ObjectCharacters.Offset);
        if (_room.Artwork is not null &&
            MotherBrainSpecialSpriteArtworkDefinitions.TryForSource(
                transfer.SourceAddress, out MotherBrainSpecialSpriteSheetDefinition sheet))
        {
            int page = sheet.ResolvePage(
                transfer.SourceAddress, transfer.Size, transfer.VramDestination);
            if (_room.Artwork.MotherBrainSpecialSprites is null)
                throw new InvalidDataException(
                    $"Installed enemy artwork is missing {sheet.FileName}.");
            _presentation.ObjCharacters.RecordUpload(destination, transfer.Size, new MotherBrainSpecialSpriteCharacters(
                Enumerable.Range(0, MotherBrainSpecialSpriteArtworkDefinitions.All.Count)
                    .Single(index => MotherBrainSpecialSpriteArtworkDefinitions.All[index] == sheet), page, transfer.Size));
            return;
        }
        if (EscapeTextArtworkDefinitions.ContainsSource(transfer.SourceAddress) &&
            _room.Artwork is not null)
        {
            int page = -1;
            for (int index = 0; index < EscapeTextArtworkDefinitions.PageCount;
                 index++)
            {
                if (transfer.SourceAddress ==
                    EscapeTextArtworkDefinitions.PageSource(index))
                {
                    page = index;
                    break;
                }
            }
            if (page < 0 ||
                transfer.Size != EscapeTextArtworkDefinitions.PageByteCount(page) ||
                transfer.VramDestination !=
                    EscapeTextArtworkDefinitions.PageDestination(page))
                throw new InvalidDataException(
                    $"Invalid Mother Brain escape-text sprite transfer {transfer}.");

            if (_room.Artwork.MotherBrainEscapeText is null)
                throw new InvalidDataException(
                    "Installed enemy artwork is missing mother-brain-escape-text-tiles.png.");
            _presentation.ObjCharacters.RecordUpload(destination, transfer.Size, new MotherBrainEscapeTextCharacters(page));
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

            _ = _room.MotherBrainCorpseArtwork;
            _presentation.ObjCharacters.RecordUpload(destination, transfer.Size, new MotherBrainCorpseCharacters(page));
            return;
        }
        if (_presentation.EscapeTimerArtwork?.TryRecordNativeTransfer(
            _presentation.ObjCharacters, unchecked((int)transfer.SourceAddress), checked((ushort)transfer.Size), checked((ushort)transfer.VramDestination)) == true)
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
        // The mutable $7E:9000 corpse buffer reaches the OBJ characters through the ordinary mode-1 DMA.
        _presentation.ObjCharacters.RecordUpload(destination, transfer.Size, new StagedObjCharacters(
            _room.Staging.ReadNativeTransfer(unchecked((int)transfer.SourceAddress), transfer.Size)));
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (MotherBrain?.LastSoundEffect is SoundEffectId motherBrainLibrary2)
            audio.QueueEnemySound(RequireLibrary(motherBrainLibrary2, SoundEffectLibrary.Library2), maximumQueued: 6);
        if (MotherBrain?.LastSoundEffectLibrary1 is SoundEffectId motherBrainLibrary1)
            audio.QueueEnemySound(RequireLibrary(motherBrainLibrary1, SoundEffectLibrary.Library1), maximumQueued: 6);
        if (MotherBrain?.LastSoundEffectLibrary3 is SoundEffectId motherBrainLibrary3)
        {
            // Explosion instruction $13 is QueueSfx3_Max3; the other translated library-
            // three Mother Brain instructions use Max6.
            byte maximumQueued = motherBrainLibrary3 == MotherBrainSoundDefinitions.Explosion ? (byte)3 : (byte)6;
            audio.QueueEnemySound(RequireLibrary(motherBrainLibrary3, SoundEffectLibrary.Library3), maximumQueued);
        }
    }

    /// <summary>
    /// Each published slot stands for one of the cartridge's per-library sound queues, so a sound
    /// recorded in a slot must belong to that slot's library.
    /// </summary>
    private static SoundEffectId RequireLibrary(SoundEffectId sound, SoundEffectLibrary library) =>
        sound.Library == library ? sound : throw new InvalidOperationException(
            $"Mother Brain published {sound} in its {library} sound slot.");

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        if (MotherBrain is not null)
        {
            foreach (DelayedMusicRequest request in MotherBrain.MusicRequests)
            {
                audio.QueueMusic(new DelayedMusicRequest(request.Command, request.Delay));
            }
        }
    }
}
