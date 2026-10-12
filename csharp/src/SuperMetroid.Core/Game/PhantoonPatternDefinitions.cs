namespace SuperMetroid.Core.Game;

/// <summary>Fixed Phantoon rain placements and shot-response markers.</summary>
public static class PhantoonPatternDefinitions
{
    /// <summary>$A7:CDAD, Phantoon_FlameRain_PositionTable: figure-eight cursor and world X/Y; each native record also has an unused zero word.</summary>
    /// <param name="pattern">Native random-bucket index, 0..7; patterns zero and four share the same body placement.</param>
    /// <returns>The body variable-A path cursor and whole-pixel room coordinates; does not draw RNG or move the boss.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pattern is outside 0..7.</exception>
    public static (ushort Cursor, ushort X, ushort Y) RainPlacement(int pattern) => pattern switch
    {
        0 or 4 => (1, 128, 96),
        1 => (71, 168, 64),
        2 => (136, 208, 96),
        3 => (201, 168, 128),
        5 => (334, 88, 64),
        6 => (399, 48, 96),
        7 => (465, 88, 128),
        _ => throw new ArgumentOutOfRangeException(nameof(pattern)),
    };

    /// <summary>
    /// $A7:CFC2 starts the eight-flame rain one column after the body. Wrapping nine
    /// twenty-pixel columns leaves the gap precisely at the body's rain-placement X.
    /// </summary>
    public static RainColumnSequence FirstRainColumns => default;

    /// <summary>Computed $A7:CFC2 first-flame column lookup, indexed by the eight rain patterns rather than by individual flames.</summary>
    public readonly struct RainColumnSequence : IReadOnlyList<byte>
    {
        /// <summary>Eight pattern-to-first-column entries; each pattern's caller subsequently emits eight flames across nine wrapping columns.</summary>
        public int Count => 8;
        /// <summary>Gets the first flame's zero-based column immediately after the selected body's column, wrapping from eight to zero.</summary>
        /// <param name="pattern">Native rain-pattern index, 0..7.</param>
        /// <returns>A column from 0..8; following eight consecutive columns leaves the body placement's column empty.</returns>
        /// <exception cref="IndexOutOfRangeException">The pattern is outside the eight-entry lookup.</exception>
        public byte this[int pattern]
        {
            get
            {
                if ((uint)pattern >= Count) throw new IndexOutOfRangeException();
                int origin = PhantoonFlameSpawnDefinitions.RainX(0);
                int spacing = PhantoonFlameSpawnDefinitions.RainX(1) - origin;
                int bodyColumn = (RainPlacement(pattern).X - origin) / spacing;
                return (byte)((bodyColumn + 1) % 9);
            }
        }
        /// <summary>Enumerates each pattern's first flame column in native pattern order, without advancing RNG or spawning flames.</summary>
        /// <returns>An enumerator over the eight computed column selectors.</returns>
        public IEnumerator<byte> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// $A7:CDA5, Phantoon_Unknown0FEAValues: the shot reaction at $A7:DE5C-DE6A
    /// indexes these eight bytes with RNG &amp; 7 and writes the selected marker to eye variable B.
    /// </summary>
    /// <remarks>
    /// Issue1165 narrow nonsense exception: these are arbitrary random-bucket assignments,
    /// not direction, phase or geometric selectors. Native eye variable B has no reader;
    /// managed Phantoon reads of variable B address the other body/mouth/tentacle slots.
    /// The marker remains exposed state, so replacing this with a different 50/50 selection
    /// changes exact RNG-to-state behavior. Boolean cases would merely recite the same bucket
    /// choices. Retain only this mapping and preserve the existing RNG call/state write.
    /// </remarks>
    public static ReadOnlySpan<byte> ShotEyeMarkers => [6, 6, 8, 8, 6, 8, 6, 8];

    /// <summary>
    /// $A7:D40D-$D41E selects the eye program from Samus's relative octant. The unused
    /// code5 retains the native downward selection; all other values name a direction.
    /// </summary>
    /// <param name="direction">Native relative-direction selector, 0..8, with four and the unused five both selecting down.</param>
    /// <returns>The bank-$A7 eye instruction-list pointer; the caller owns publishing it and resetting its instruction timer.</returns>
    /// <exception cref="InvalidDataException">The direction exceeds the nine authored selectors.</exception>
    public static ushort EyeInstruction(ushort direction) => direction switch
    {
        0 => PhantoonInstructionProgramDefinitions.EyeLookingUp,
        1 => PhantoonInstructionProgramDefinitions.EyeLookingUpRight,
        2 => PhantoonInstructionProgramDefinitions.EyeLookingRight,
        3 => PhantoonInstructionProgramDefinitions.EyeLookingDownRight,
        4 or 5 => PhantoonInstructionProgramDefinitions.EyeLookingDown,
        6 => PhantoonInstructionProgramDefinitions.EyeLookingDownLeft,
        7 => PhantoonInstructionProgramDefinitions.EyeLookingLeft,
        8 => PhantoonInstructionProgramDefinitions.EyeLookingUpLeft,
        _ => throw new InvalidDataException($"Phantoon eye direction {direction} exceeds nine authored selectors."),
    };
}
