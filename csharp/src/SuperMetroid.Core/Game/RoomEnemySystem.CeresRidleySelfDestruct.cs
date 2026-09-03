using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    private const ushort CeresEscapeTimerTransferList = 0xc4cb;
    private const ushort CeresEscapeSecondTransferList = 0xc4fe;
    private const ushort CeresJapaneseText = 0xc450;
    private const ushort CeresJapaneseTextTransferList = 0xc3b8;
    private const ushort CeresTypewriterTileBase = 0x3582;
    private const int CeresEmergencyTextTilemapAddress = 0xa6c164;
    private const ushort CeresEmergencyTextTilemapBytes = 0x0012;
    private const ushort CeresEmergencyTextVramDestination = 0x50cb;

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
                state.CeresEscapeTransferListPointer = CeresEscapeTimerTransferList;
                state.FunctionTimer = 2;
                goto case 2;

            case 2:
                if (QueueNextCeresEscapeTransfer(state, vramWriteQueue))
                {
                    state.CeresEscapeTransferListPointer = CeresEscapeSecondTransferList;
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
                    state.MusicRequest = 7;
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
                    QueueCeresTransferList(CeresJapaneseTextTransferList, vramWriteQueue);
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
                CeresEscapeStartedThisFrame = true;
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres self-destruct phase ${state.FunctionTimer:X4} is not a native even dispatcher value.");
        }
    }

    /// <summary>Ports DrawEmergencyText at $A6:C136 exactly.</summary>
    private static void QueueCeresEmergencyText(VramWriteQueue? vramWriteQueue)
    {
        if (vramWriteQueue is null)
            throw new InvalidOperationException("Ceres emergency text requires a VRAM write queue.");
        vramWriteQueue.Enqueue(
            CeresEmergencyTextTilemapBytes,
            CeresEmergencyTextTilemapAddress,
            CeresEmergencyTextVramDestination);
    }

    /// <summary>Ports ProcessSpriteTilesTransfers at $A6:C26E one record per actor call.</summary>
    private bool QueueNextCeresEscapeTransfer(
        RidleyEnemyState state,
        VramWriteQueue? vramWriteQueue)
    {
        ushort pointer = state.CeresEscapeTransferListPointer;
        ushort byteCount = ReadWord(_bus!, 0xa60000 | pointer);
        if (byteCount == 0)
            return true;
        if (vramWriteQueue is null)
        {
            throw new InvalidOperationException(
                "Ceres escape graphics require the runtime's native VRAM write queue.");
        }

        ushort sourceOffset = ReadWord(_bus!, 0xa60000 | unchecked((ushort)(pointer + 2)));
        byte sourceBank = _bus!.ReadByte(0xa60000 | unchecked((ushort)(pointer + 4)));
        ushort destination = ReadWord(_bus!, 0xa60000 | unchecked((ushort)(pointer + 5)));
        vramWriteQueue.Enqueue(byteCount, (sourceBank << 16) | sourceOffset, destination);

        state.CeresEscapeTransferListPointer = unchecked((ushort)(pointer + 7));
        return ReadWord(_bus!, 0xa60000 | state.CeresEscapeTransferListPointer) == 0;
    }

    /// <summary>Queues every record in the one-shot Japanese overlay list at $A6:C3B8.</summary>
    private void QueueCeresTransferList(ushort pointer, VramWriteQueue? vramWriteQueue)
    {
        if (vramWriteQueue is null)
            throw new InvalidOperationException("Ceres Japanese text requires a VRAM write queue.");

        for (int record = 0; record < 32; record++, pointer = unchecked((ushort)(pointer + 7)))
        {
            ushort byteCount = ReadWord(_bus!, 0xa60000 | pointer);
            if (byteCount == 0)
                return;
            ushort sourceOffset = ReadWord(_bus!, 0xa60000 | unchecked((ushort)(pointer + 2)));
            byte sourceBank = _bus!.ReadByte(0xa60000 | unchecked((ushort)(pointer + 4)));
            ushort destination = ReadWord(_bus, 0xa60000 | unchecked((ushort)(pointer + 5)));
            vramWriteQueue.Enqueue(byteCount, (sourceBank << 16) | sourceOffset, destination);
        }

        throw new InvalidDataException("Ceres Japanese transfer list exceeded 32 records.");
    }

    /// <summary>Ports the byte-oriented command stream consumed by $A6:C2A7.</summary>
    private bool StepCeresEscapeTypewriter(RidleyEnemyState state)
    {
        if (state.CeresEscapeTextDelayTimer != 0)
        {
            state.CeresEscapeTextDelayTimer--;
            return false;
        }
        state.CeresEscapeTextDelayTimer = state.CeresEscapeTextDelay;

        ushort pointer = state.CeresEscapeTextPointer;
        while (true)
        {
            ushort command = ReadWord(_bus!, 0xa60000 | pointer);
            if (command == 0)
                return true;
            if (command == 1)
            {
                state.CeresEscapeTextDelay = ReadWord(
                    _bus!, 0xa60000 | unchecked((ushort)(pointer + 2)));
                pointer = unchecked((ushort)(pointer + 4));
                continue;
            }
            if (command == 13)
            {
                state.CeresEscapeTextDestination = ReadWord(
                    _bus!, 0xa60000 | unchecked((ushort)(pointer + 2)));
                pointer = unchecked((ushort)(pointer + 4));
                continue;
            }

            byte character = unchecked((byte)command);
            state.CeresEscapeTextPointer = unchecked((ushort)(pointer + 1));
            if (character == 32)
            {
                state.CeresEscapeTextDestination++;
                return false;
            }
            if (character == 33)
                character = 91;

            ushort tile = unchecked((ushort)(CeresTypewriterTileBase + character - 65));
            _vram!.ExecuteWordTransfer([tile], state.CeresEscapeTextDestination, 1);
            state.CeresEscapeTextDestination++;
            state.CeresEscapeTextSoundCounter = unchecked((ushort)(
                (state.CeresEscapeTextSoundCounter + 1) % 2));
            // Ridley_Func_61 emits one key click for every second visible character.
            // Ceres is area six, selecting QueueSfx2_Max3($45); the alternate library-three
            // call belongs to the non-Ceres reuse of this shared native routine.
            if (state.CeresEscapeTextSoundCounter == 0)
                QueueEnemySound(library: SoundEffectLibrary.Library2, soundId: 0x0045, maximumQueued: 3);
            return false;
        }
    }

    /// <summary>Ports the three-color alarm palette cycle at $A6:C19C.</summary>
    private void UpdateCeresSelfDestructPalette(RidleyEnemyState state, ushort frameCounter)
    {
        if ((frameCounter & 3) != 0)
            return;

        state.CeresEscapePaletteFrame = unchecked((ushort)(
            (state.CeresEscapePaletteFrame + 1) & 0x000f));
        int source = 0xa6c1df + state.CeresEscapePaletteFrame * 6;
        _cgram!.LoadFromBus(_bus!, source, colorCount: 3, destinationIndex: 97);
    }
}
