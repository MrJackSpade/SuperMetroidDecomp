using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    private const ushort CeresJapaneseText = 0xc450;
    private const ushort CeresTypewriterTileBase = 0x3582;

    /// <summary>
    /// Ports the shared Ridley function at $A6:C04E for the Ceres branch. The phase word
    /// is the actor's native variable F: even values select the exact C062/C08E/C09F/C0BB/
    /// C0F5/C104/C117 entries. English skips only the language-specific phase eight and
    /// enters phase ten to type the cartridge's three-line English warning. Japanese enters
    /// phase eight first, installs its subtitle tilemap, and then uses the same typewriter.
    /// </summary>
    private void TickCeresRidleySelfDestruct(
        RidleyEnemyState state,
        VramWriteQueue? vramWriteQueue)
    {
        switch (state.FunctionTimer)
        {
            case 0:
                // $A6:C062 copies four already-live colors before the warning graphics
                // replace their tiles. These are palette indices, not byte offsets.
                _cgram!.SetColor(97, _cgram.Colors[1]);
                _cgram.SetColor(99, _cgram.Colors[3]);
                _cgram.SetColor(81, _cgram.Colors[17]);
                _cgram.SetColor(83, _cgram.Colors[19]);
                state.CeresEscapeTransferListPointer = CeresEscapeVramTransferDefinitions.TimerSprites;
                state.FunctionTimer = 2;
                goto case 2;

            case 2:
                if (QueueNextCeresEscapeTransfer(state, vramWriteQueue))
                {
                    state.CeresEscapeTransferListPointer = CeresEscapeVramTransferDefinitions.TimerBackgrounds;
                    state.FunctionTimer = 4;
                    goto case 4;
                }
                return;

            case 4:
                if (QueueNextCeresEscapeTransfer(state, vramWriteQueue))
                {
                    // `$A6:C0AA` calls DrawEmergencyText immediately after the final
                    // BG1/BG2 graphics record. The word was missing from the earlier
                    // translation, so the newly uploaded letter tiles existed but no
                    // tilemap entries ever referenced them on screen.
                    QueueCeresEmergencyText(vramWriteQueue);
                    // $A6:C09F also queues music track seven after installing the final
                    // static warning tilemap page. The frontend consumes this typed request
                    // through the same bank-$80 queue used by every other music producer.
                    state.MusicRequest = MusicCommand.SelectTrack(7);
                    state.FunctionTimer = 6;
                    state.CeresEscapeTextDelayTimer = 128;
                }
                return;

            case 6:
                UpdateCeresSelfDestructPalette(state, _slots[0].FrameCounter);
                ushort oldDelay = state.CeresEscapeTextDelayTimer;
                state.CeresEscapeTextDelayTimer = unchecked((ushort)(oldDelay - 1));
                if (oldDelay != 1)
                    return;

                state.CeresEscapeTextPointer = CeresJapaneseText;
                state.CeresEscapeTextDestination = 0;
                state.CeresEscapeTextDelayTimer = 0;
                state.CeresEscapeTextDelay = 0;
                state.CeresEscapeTextSoundCounter = 0;
                state.CeresEscapeTypewriter = EscapeTypewriterPresentation is { } presentation
                    ? new(presentation.Get(EscapeTypewriterProgramId.Ceres), CeresTypewriterTileBase)
                    : null;

                // `$A6:C0E3-$A6:C0F1` advances the even-byte dispatch index twice for
                // English before executing the common final increment: 6 -> 8 -> 10.
                // Japanese skips the first increment and reaches phase 8. The previous
                // translation incorrectly counted the two INC instructions as four phase
                // entries and jumped English directly to phase 12, suppressing every
                // character below "EMERGENCY".
                state.FunctionTimer = JapaneseText ? (ushort)8 : (ushort)10;
                return;

            case 8:
                UpdateCeresSelfDestructPalette(state, _slots[0].FrameCounter);
                ushort oldJapaneseDelay = state.CeresEscapeTextDelayTimer;
                state.CeresEscapeTextDelayTimer = unchecked((ushort)(oldJapaneseDelay - 1));
                if (oldJapaneseDelay == 1)
                {
                    state.FunctionTimer = 10;
                    QueueCeresTransferList(CeresEscapeVramTransferDefinitions.JapaneseOverlay,
                        vramWriteQueue);
                }
                if (StepCeresEscapeTypewriter(state))
                    state.FunctionTimer = unchecked((ushort)(state.FunctionTimer + 2));
                return;

            case 10:
                UpdateCeresSelfDestructPalette(state, _slots[0].FrameCounter);
                if (StepCeresEscapeTypewriter(state))
                    state.FunctionTimer = 12;
                return;

            case 12:
                UpdateCeresSelfDestructPalette(state, _slots[0].FrameCounter);
                state.HorizontalVelocity = 0;
                state.VerticalVelocity = 0;
                state.FunctionTimer = 0;
                state.Function = RidleyAiFunction.CeresSelfDestructPaletteOnly;
                CeresStatus = 2;
                // $A6:C131 sets the area-boss bit inside Ridley's AI, so later slots (the
                // room's exit door at $A6:F562) observe it in this same EnemyMain pass.
                RequireSetAreaBossDefeated();
                CeresEscapeStartedThisFrame = true;
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres self-destruct phase ${state.FunctionTimer:X4} is not a native even dispatcher value.");
        }
    }

    /// <summary>Ports DrawEmergencyText at $A6:C136 exactly.</summary>
    private void QueueCeresEmergencyText(VramWriteQueue? vramWriteQueue)
    {
        if (vramWriteQueue is null)
            throw new InvalidOperationException("Ceres emergency text requires a VRAM write queue.");
        CeresEscapeOverlayTilemapDefinition page =
            CeresEscapeOverlayTilemapDefinitions.Emergency;
        ushort byteCount = checked((ushort)(page.WordCount * sizeof(ushort)));
        if (TileArtwork is { } installed &&
            installed.CeresEscapeOverlayTilemaps?.TryResolve(
                page.SourceAddress, byteCount, out _) != true)
            throw new InvalidDataException(
                "Installed Ceres escape has no editable EMERGENCY tilemap.");
        vramWriteQueue.Enqueue(
            byteCount,
            page.SourceAddress,
            CeresEscapeOverlayTilemapDefinitions.EmergencyDestination);
    }

    /// <summary>Ports ProcessSpriteTilesTransfers at $A6:C26E one record per actor call.</summary>
    private bool QueueNextCeresEscapeTransfer(
        RidleyEnemyState state,
        VramWriteQueue? vramWriteQueue)
    {
        ushort pointer = state.CeresEscapeTransferListPointer;
        ushort byteCount;
        int sourceAddress;
        ushort destination;
        if (CeresEscapeVramTransferDefinitions.IsTerminator(pointer))
            return true;
        if (!CeresEscapeVramTransferDefinitions.TryGet(pointer, out var transfer))
            throw new InvalidDataException(
                $"Ceres escape transfer ${pointer:X4} has no compiled record.");
        byteCount = transfer.ByteCount;
        sourceAddress = transfer.SourceAddress;
        destination = transfer.DestinationWord;
        if (vramWriteQueue is null)
        {
            throw new InvalidOperationException(
                "Ceres escape graphics require the runtime's native VRAM write queue.");
        }
        if (TileArtwork is not null && EscapeTimerArtwork is null &&
            sourceAddress is EscapeTimerTileRomData.FirstSourceAddress or
                EscapeTimerTileRomData.SecondSourceAddress)
            throw new InvalidDataException(
                "Installed Ceres escape timer has no editable sprite tile artwork.");
        if (TileArtwork is { } installed &&
            CeresEscapeTileArtworkDefinitions.Contains(sourceAddress, byteCount) &&
            installed.CeresEscapeTiles?.TryResolve(sourceAddress, byteCount, out _) != true)
            throw new InvalidDataException(
                $"Installed Ceres escape has no editable character page for ${sourceAddress:X6}.");

        if (EscapeTimerArtwork?.TryQueueNativeTransfer(
            vramWriteQueue, sourceAddress, byteCount, destination) != true)
            vramWriteQueue.Enqueue(byteCount, sourceAddress, destination);

        state.CeresEscapeTransferListPointer = unchecked((ushort)(pointer + 7));
        return CeresEscapeVramTransferDefinitions.IsTerminator(
            state.CeresEscapeTransferListPointer);
    }

    /// <summary>Queues every record in the one-shot Japanese overlay list at $A6:C3B8.</summary>
    private void QueueCeresTransferList(ushort pointer, VramWriteQueue? vramWriteQueue)
    {
        if (vramWriteQueue is null)
            throw new InvalidOperationException("Ceres Japanese text requires a VRAM write queue.");

        for (int record = 0; record < 32; record++, pointer = unchecked((ushort)(pointer + 7)))
        {
            if (CeresEscapeVramTransferDefinitions.IsTerminator(pointer))
                return;
            if (!CeresEscapeVramTransferDefinitions.TryGet(pointer, out var transfer))
                throw new InvalidDataException(
                    $"Ceres Japanese transfer ${pointer:X4} has no compiled record.");
            if (TileArtwork?.CeresEscapeOverlayTilemaps?.TryResolve(
                    transfer.SourceAddress, transfer.ByteCount, out _) != true)
                throw new InvalidDataException(
                    $"Installed Ceres Japanese overlay ${pointer:X4} has no editable tilemap.");
            vramWriteQueue.Enqueue(transfer.ByteCount, transfer.SourceAddress,
                transfer.DestinationWord);
        }

        throw new InvalidDataException("Ceres Japanese transfer list exceeded 32 records.");
    }

    /// <summary>Ports the byte-oriented command stream consumed by $A6:C2A7.</summary>
    private bool StepCeresEscapeTypewriter(RidleyEnemyState state)
    {
        var installed = state.CeresEscapeTypewriter ?? throw new InvalidDataException(
            "Ceres warning text requires installed typewriter artwork.");
        bool completed = installed.Step(_vram!);
        state.CeresEscapeTextPointer = unchecked((ushort)installed.Pointer);
        state.CeresEscapeTextDestination = installed.Destination;
        state.CeresEscapeTextDelayTimer = installed.DelayTimer;
        state.CeresEscapeTextDelay = installed.Delay;
        state.CeresEscapeTextSoundCounter = unchecked((ushort)(installed.GlyphsWritten & 1));
        if (installed.ClickRequested)
            QueueEnemySound(EscapeTypewriterRomData.CeresClick, maximumQueued: 3);
        return completed;
    }

    /// <summary>Ports the three-color alarm palette cycle at $A6:C19C.</summary>
    private void UpdateCeresSelfDestructPalette(RidleyEnemyState state, ushort frameCounter)
    {
        if ((frameCounter & 3) != 0)
            return;

        state.CeresEscapePaletteFrame = unchecked((ushort)(
            (state.CeresEscapePaletteFrame + 1) & 0x000f));
        if (CeresRidleyColors is { } colors)
            colors.ApplyAlarm(_cgram!, state.CeresEscapePaletteFrame);
        else throw new InvalidOperationException(
            "Ceres alarm requires installed Ridley color rows.");
    }
}
