namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's pre-combat room lock and rise dispatcher at `$A7:C865-$C9ED`. This is timing
/// code, not a cinematic approximation: all counters, half-pixel movement, X shake, and
/// transition pointers retain their retail values.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Advances Kraid's pre-combat rise state, including screen restriction, tilemap transfers, rock cadence, and first-phase handoff.</summary>
    /// <param name="body">Kraid body slot carrying the active rise function and its timer.</param>
    /// <param name="state">Shared Kraid state containing body parts and instruction continuations.</param>
    /// <param name="samus">Samus to constrain to the first screen while the rise sequence is active, when present.</param>
    private void RunKraidRiseFunction(
        RoomEnemySlot body,
        KraidEnemyState state,
        SamusState? samus)
    {
        KraidAiFunction function = (KraidAiFunction)body.VariableA;
        switch (function)
        {
            case KraidAiFunction.RestrictSamusToFirstScreen:
                RestrictSamusToKraidFirstScreen(samus);
                TickKraidFunctionTimer(body, state.Parts[0]);
                return;

            case KraidAiFunction.RaiseKraidThroughFloor:
                RestrictSamusToKraidFirstScreen(samus);
                body.VariableA = (ushort)KraidAiFunction.RaiseLoadBottomTilemap;
                TransferKraidTopTilemap(state);
                return;

            case KraidAiFunction.RaiseLoadBottomTilemap:
                RestrictSamusToKraidFirstScreen(samus);
                body.VariableA = (ushort)KraidAiFunction.RaiseRocksEvery16Frames;
                body.VariableF = 120;
                EarthquakeTimer = 496;
                state.MusicRequest = MusicCommand.SelectTrack(5);
                TransferKraidBottomTilemap(state);
                return;

            case KraidAiFunction.RaiseRocksEvery16Frames:
                RestrictSamusToKraidFirstScreen(samus);
                body.VariableF = unchecked((ushort)(body.VariableF - 1));
                if (body.VariableF == 0)
                {
                    body.VariableA = (ushort)KraidAiFunction.RaiseRocksEvery8Frames;
                    body.VariableF = 96;
                }
                else if ((body.VariableF & 0x000f) == 0)
                {
                    RequestKraidRisingRock(body, state);
                }
                return;

            case KraidAiFunction.RaiseRocksEvery8Frames:
                RestrictSamusToKraidFirstScreen(samus);
                body.VariableF = unchecked((ushort)(body.VariableF - 1));
                if (body.VariableF == 0)
                {
                    body.VariableA = (ushort)KraidAiFunction.RaiseBody;
                    body.VariableF = 288;
                }
                else if ((body.VariableF & 7) == 0)
                {
                    RequestKraidRisingRock(body, state);
                }
                return;

            case KraidAiFunction.RaiseBody:
                RestrictSamusToKraidFirstScreen(samus);
                if ((EarthquakeTimer & 5) == 0)
                    RequestKraidRisingRock(body, state);
                body.XPosition = unchecked((ushort)(body.XPosition +
                    ((body.YPosition & 2) == 0 ? -1 : 1)));
                AddSignedKraidVerticalDisplacement(body, -0x8000);
                if (unchecked((short)(body.YPosition - 457)) < 0)
                {
                    body.XPosition = 176;
                    // `$C97B` does not leave execution at setup routine `$ADE9`.
                    // That JSR immediately installs `$AEA4`, seeds the random thinking
                    // timer and selects roar list entry `$96DA` for the first attack.
                    body.VariableB = KraidHeadInstructionDefinitions.RoarContinuation;
                    SetupKraidFirstPhaseThinking(body, state);

                    // The foot owns first-phase lunges independently of the body thinker.
                    // Its 300-frame delay starts on the exact rise-completion frame.
                    RoomEnemySlot foot = _slots[5];
                    foot.VariableA = (ushort)KraidAiFunction.FootFirstPhaseThinking;
                    foot.VariableF = 300;
                    state.Parts[5].NextFunction = KraidAiFunction.FootPrepareFirstPhaseLunge;
                    _slots[1].CurrentInstruction =
                        KraidArmInstructionProgramDefinitions.Normal;
                    _slots[1].InstructionTimer = 1;
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Kraid body function $A7:{body.VariableA:X4} is not translated.");
        }
    }

    /// <summary>
    /// Ports <c>RestrictSamusXPositionToFirstScreen</c> ($A7:C9EE). It writes the integer X
    /// and previous X words only, so the camera's $90:95A0 compare sees Samus's subsequent
    /// movement away from $100 as a scroll.
    /// </summary>
    private static void RestrictSamusToKraidFirstScreen(SamusState? samus)
    {
        if (samus is not null && unchecked((short)(samus.XPosition - 256)) >= 0)
        {
            samus.XPosition = 256;
            samus.WritePreviousXPosition(256);
        }
    }

    /// <summary>Counts down a Kraid function timer and selects the part's next function when the timer expires.</summary>
    /// <param name="slot">Enemy slot whose function timer and function selector are updated.</param>
    /// <param name="part">Part state supplying the function to install at timer expiration.</param>
    private static void TickKraidFunctionTimer(
        RoomEnemySlot slot,
        KraidPartState part)
    {
        if (slot.VariableF == 0)
            return;
        slot.VariableF--;
        if (slot.VariableF == 0)
            slot.VariableA = (ushort)part.NextFunction;
    }

    /// <summary>Adds a signed 16.16 displacement to Kraid's combined integer and fractional vertical position.</summary>
    /// <param name="body">Kraid body slot whose vertical position is updated.</param>
    /// <param name="displacement">Signed fixed-point delta, with the integer portion in the upper 16 bits.</param>
    private static void AddSignedKraidVerticalDisplacement(
        RoomEnemySlot body,
        int displacement)
    {
        uint position = ((uint)body.YPosition << 16) | body.YSubposition;
        position = unchecked(position + (uint)displacement);
        body.YPosition = unchecked((ushort)(position >> 16));
        body.YSubposition = unchecked((ushort)position);
    }

    /// <summary>Consumes one random value and derives Kraid's first-phase thinking delay from its low three bits.</summary>
    /// <returns>A timer from 128 through 448 in 64-frame increments; a zero random selector maps to 128.</returns>
    private ushort ReadKraidThinkingTimer()
    {
        ushort random = RequireRandomNumber();
        int value = random & 7;
        if (value == 0)
            value = 2;
        return unchecked((ushort)(value << 6));
    }
}
