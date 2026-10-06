using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Native $A6:C1DF..C23E EMERGENCY text colors, with the return half reflected from independently required forward colors.</summary>
internal sealed class CeresRidleyAlarmColorDefinitions
{
    // All27 forward words remain required. A reflected sample is stored only when
    // independently supplied content differs from its own supplied forward partner.
    private readonly Dictionary<int, ushort> inputs = new();

    internal CeresRidleyAlarmColorDefinitions(ushort[][] rows)
    {
        for (int row = 0; row < CeresRidleyPaletteRomData.AlarmRowCount; row++)
        for (int color = 0; color < CeresRidleyPaletteRomData.AlarmColorCount; color++)
        {
            int source = SourceRow(row);
            if (source == row || rows[row][color] != rows[source][color])
                inputs[row * CeresRidleyPaletteRomData.AlarmColorCount + color] = rows[row][color];
        }
    }

    /// <summary>$A6:C1DF..C23E: sixteen rows ascend through row8, then revisit rows7..1 in reverse order.</summary>
    internal static int SourceRow(int row)
    {
        if ((uint)row >= CeresRidleyPaletteRomData.AlarmRowCount)
            throw new ArgumentOutOfRangeException(nameof(row));
        return Math.Min(row, CeresRidleyPaletteRomData.AlarmRowCount - row);
    }

    internal ushort Resolve(int row, int color)
    {
        int source = SourceRow(row);
        if ((uint)color >= CeresRidleyPaletteRomData.AlarmColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        int key = row * CeresRidleyPaletteRomData.AlarmColorCount + color;
        return inputs.TryGetValue(key, out ushort value) ? value
            : inputs[source * CeresRidleyPaletteRomData.AlarmColorCount + color];
    }
}
