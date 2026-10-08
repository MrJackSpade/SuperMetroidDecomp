namespace SuperMetroid.Core.Game;

/// <summary>Fixed Phantoon rain placements and shot-response markers.</summary>
public static class PhantoonPatternDefinitions
{
    /// <summary>$A7:CDAD, Phantoon_FlameRain_PositionTable: figure-eight cursor and world X/Y; each native record also has an unused zero word.</summary>
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

    public readonly struct RainColumnSequence : IReadOnlyList<byte>
    {
        public int Count => 8;
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