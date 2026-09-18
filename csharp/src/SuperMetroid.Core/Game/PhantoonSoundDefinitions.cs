namespace SuperMetroid.Core.Game;

/// <summary>Compiled sound selections owned by Phantoon's bank-$A7 instruction callbacks.</summary>
internal static class PhantoonSoundDefinitions
{
    /// <summary>
    /// <c>Phantoon_MaterializationSFX</c> at <c>$A7:CDED-$A7:CDF2</c>, queued from
    /// sound library two in a repeating three-callback cycle.
    /// </summary>
    private static readonly ushort[] Materialization = [0x0079, 0x007a, 0x007b];

    /// <summary>Returns the materialization sound for a native zero-through-two index.</summary>
    internal static ushort MaterializationSound(ushort index)
    {
        if (index >= Materialization.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index), index,
                "Phantoon materialization sound index must be zero through two.");
        }

        return Materialization[index];
    }
}
