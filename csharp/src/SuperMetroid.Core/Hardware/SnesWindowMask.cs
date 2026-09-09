namespace SuperMetroid.Core.Hardware;

/// <summary>Hardware window membership, before TM/TS and TMW/TSW layer admission.</summary>
public static class SnesWindowMask
{
    /// <summary>
    /// Evaluates one layer's selected windows at an eight-bit screen X. Endpoints
    /// are inclusive; left greater than right is empty, not a wrapping interval.
    /// A disabled window contributes nothing, even when its inversion bit is set.
    /// Color-window membership uses the same operation; color-math clipping policy
    /// and main/subscreen layer masking are separate consumers of this result.
    /// </summary>
    public static bool Contains(SnesWindowSelection selection, SnesWindowLogic logic,
        byte x, byte firstLeft, byte firstRight, byte secondLeft, byte secondRight)
    {
        const SnesWindowSelection known = SnesWindowSelection.InvertFirst | SnesWindowSelection.EnableFirst |
            SnesWindowSelection.InvertSecond | SnesWindowSelection.EnableSecond;
        if ((selection & ~known) != 0)
            throw new ArgumentOutOfRangeException(nameof(selection), "Expected one window-selection nibble.");
        if (logic is < SnesWindowLogic.Or or > SnesWindowLogic.Xnor)
            throw new ArgumentOutOfRangeException(nameof(logic));
        bool firstEnabled = (selection & SnesWindowSelection.EnableFirst) != 0;
        bool secondEnabled = (selection & SnesWindowSelection.EnableSecond) != 0;
        bool first = (x >= firstLeft && x <= firstRight) ^ ((selection & SnesWindowSelection.InvertFirst) != 0);
        bool second = (x >= secondLeft && x <= secondRight) ^ ((selection & SnesWindowSelection.InvertSecond) != 0);
        if (!firstEnabled) return secondEnabled && second;
        if (!secondEnabled) return first;
        return logic switch
        {
            SnesWindowLogic.Or => first || second,
            SnesWindowLogic.And => first && second,
            SnesWindowLogic.Xor => first != second,
            SnesWindowLogic.Xnor => first == second,
            _ => throw new ArgumentOutOfRangeException(nameof(logic)),
        };
    }
}
