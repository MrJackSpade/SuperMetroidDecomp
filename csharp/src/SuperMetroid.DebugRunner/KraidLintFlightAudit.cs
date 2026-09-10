using SuperMetroid.Core.Game;

/// <summary>
/// Observes naturally spawned belly platforms during the existing retail Kraid encounter.
/// Bank A7:B89B subtracts 3.5 pixels each frame, disables contact below X=56, and
/// hides/reschedules below X=32. In particular, there is no stationary wall-lodging phase.
/// This observer does not change actor state or substitute a test movement implementation.
/// </summary>
internal sealed class KraidLintFlightAudit
{
    private readonly uint?[] _before = new uint?[3];
    private readonly int[] _flightFrames = new int[3];
    private readonly int[] _wallFrames = new int[3];
    private readonly int[] _resets = new int[3];

    public void BeforeFrame(RoomEnemySystem enemies)
    {
        for (int index = 0; index < 3; index++)
        {
            RoomEnemySlot lint = enemies.Slots[index + 2];
            _before[index] = lint.VariableA == (ushort)KraidAiFunction.LintFire
                ? ((uint)lint.XPosition << 16) | lint.XSubposition
                : null;
        }
    }

    public void AfterFrame(RoomEnemySystem enemies)
    {
        for (int index = 0; index < 3; index++)
        {
            if (_before[index] is not uint previous)
                continue;
            RoomEnemySlot lint = enemies.Slots[index + 2];
            uint actual = ((uint)lint.XPosition << 16) | lint.XSubposition;
            uint expected = unchecked(previous - 0x00038000u);
            if (actual != expected)
                throw new InvalidDataException(
                    $"Kraid lint {index}: expected uninterrupted flight {expected:X8}, got {actual:X8}.");
            _flightFrames[index]++;

            bool pastContactBoundary = unchecked((short)(lint.XPosition - 56)) < 0;
            bool hidden = unchecked((short)(lint.XPosition - 32)) < 0;
            if (lint.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) != pastContactBoundary ||
                lint.Properties.HasAny(EnemyProperties.Invisible) != hidden)
                throw new InvalidDataException($"Kraid lint {index}: wrong wall visibility/contact at X={lint.XPosition}.");
            if (pastContactBoundary)
                _wallFrames[index]++;
            if (hidden)
            {
                if (lint.VariableA != (ushort)KraidAiFunction.AlignPartToKraid ||
                    lint.VariableF != 300 || lint.VariableB != 0 ||
                    enemies.Kraid!.Parts[index + 2].NextFunction != KraidAiFunction.LintProduce)
                    throw new InvalidDataException($"Kraid lint {index}: incorrect wall reset/delay.");
                _resets[index]++;
            }
            else if (lint.VariableA != (ushort)KraidAiFunction.LintFire)
                throw new InvalidDataException($"Kraid lint {index}: stopped before disappearing into wall.");
        }
    }

    public void VerifyCoverage()
    {
        for (int index = 0; index < 3; index++)
            if (_flightFrames[index] == 0 || _wallFrames[index] == 0 || _resets[index] == 0)
                throw new InvalidDataException($"Kraid lint {index}: encounter did not cover full flight and wall reset.");
        Console.WriteLine($"Kraid lint wall audit: flight=[{string.Join(',', _flightFrames)}], " +
            $"wall=[{string.Join(',', _wallFrames)}], resets=[{string.Join(',', _resets)}].");
    }
}
