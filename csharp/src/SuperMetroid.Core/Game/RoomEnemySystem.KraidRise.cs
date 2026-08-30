namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's pre-combat room lock and rise dispatcher at `$A7:C865-$C9ED`. This is timing
/// code, not a cinematic approximation: all counters, half-pixel movement, X shake, and
/// transition pointers retain their retail values.
/// </summary>
public sealed partial class RoomEnemySystem
{
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
                state.TopTilemapUploadCount++;
                return;

            case KraidAiFunction.RaiseLoadBottomTilemap:
                RestrictSamusToKraidFirstScreen(samus);
                body.VariableA = (ushort)KraidAiFunction.RaiseRocksEvery16Frames;
                body.VariableF = 120;
                EarthquakeTimer = 496;
                state.MusicRequest = 5;
                state.BottomTilemapUploadCount++;
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
                    state.RiseRockSpawnRequestCount++;
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
                    state.RiseRockSpawnRequestCount++;
                }
                return;

            case KraidAiFunction.RaiseBody:
                RestrictSamusToKraidFirstScreen(samus);
                if ((EarthquakeTimer & 5) == 0)
                    state.RiseRockSpawnRequestCount++;
                body.XPosition = unchecked((ushort)(body.XPosition +
                    ((body.YPosition & 2) == 0 ? -1 : 1)));
                AddSignedKraidVerticalDisplacement(body, -0x8000);
                if (unchecked((short)(body.YPosition - 457)) < 0)
                {
                    body.XPosition = 176;
                    body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
                    state.ThinkingTimer = ReadKraidThinkingTimer();
                    _slots[2].YPosition = unchecked((ushort)(body.YPosition - 20));
                    _slots[3].YPosition = unchecked((ushort)(body.YPosition + 46));
                    _slots[4].YPosition = unchecked((ushort)(body.YPosition + 112));
                    _slots[1].CurrentInstruction = 0x89f3;
                    _slots[1].InstructionTimer = 1;
                }
                return;

            case KraidAiFunction.MainloopThinking:
                // The next slice translates mouth/body shot handling and the first-phase
                // thinker. Stop explicitly at the proven combat handoff instead of running
                // a host-authored idle state that would conceal missing attacks.
                throw new NotSupportedException(
                    "Kraid first-phase combat thinker $A7:ADE9 is not translated yet.");

            default:
                throw new NotSupportedException(
                    $"Kraid body function $A7:{body.VariableA:X4} is not translated.");
        }
    }

    private static void RestrictSamusToKraidFirstScreen(SamusState? samus)
    {
        if (samus is not null && unchecked((short)(samus.XPosition - 256)) >= 0)
            samus.XPosition = 256;
    }

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

    private static void AddSignedKraidVerticalDisplacement(
        RoomEnemySlot body,
        int displacement)
    {
        uint position = ((uint)body.YPosition << 16) | body.YSubposition;
        position = unchecked(position + (uint)displacement);
        body.YPosition = unchecked((ushort)(position >> 16));
        body.YSubposition = unchecked((ushort)position);
    }

    private ushort ReadKraidThinkingTimer()
    {
        ushort random = _readRandomNumber?.Invoke() ?? 0;
        int value = random & 7;
        if (value == 0)
            value = 2;
        return unchecked((ushort)(value << 6));
    }
}
