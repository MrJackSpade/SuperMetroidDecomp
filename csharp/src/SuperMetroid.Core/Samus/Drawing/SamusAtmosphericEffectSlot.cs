namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>One debugger-visible packed atmospheric slot.</summary>
public sealed class SamusAtmosphericEffectSlot
{
    /// <summary>Low byte animation frame, high byte graphics type; 0 through 65535.</summary>
    [PackedFields("packed slot word: low byte animation frame, high byte graphics type; zero marks an empty slot")]
    public int FrameAndType
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The zero-based frame within the current graphics type, held in the packed word's low byte.</summary>
    public int AnimationFrame => unchecked((byte)FrameAndType);

    /// <summary>
    /// The graphics types in the order the packed word's high byte numbers them, the order of the native per-type
    /// tables: a type is held as its position here.
    /// </summary>
    private static readonly SamusAtmosphericEffectType[] TypeOrder =
    [
        SamusAtmosphericEffectType.Inactive, SamusAtmosphericEffectType.FootstepSplash,
        SamusAtmosphericEffectType.AlternateFootstepSplash, SamusAtmosphericEffectType.DivingSplash,
        SamusAtmosphericEffectType.LavaSurfaceDamage, SamusAtmosphericEffectType.Bubbles, SamusAtmosphericEffectType.Dust,
        SamusAtmosphericEffectType.AlternateDust,
    ];

    /// <summary>The slot's graphics type, held in the packed word's high byte; <see cref="SamusAtmosphericEffectType.Inactive"/> when cleared.</summary>
    public SamusAtmosphericEffectType Type => TypeOrder[SnesLittleEndian.HighByte(checked((ushort)FrameAndType))];

    /// <summary>Packs a graphics type and its animation frame into the slot word.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The type is not one the word can hold.</exception>
    internal void SetFrameAndType(SamusAtmosphericEffectType type, int animationFrame)
    {
        int position = Array.IndexOf(TypeOrder, type);
        if (position < 0)
            throw new ArgumentOutOfRangeException(nameof(type), type, "Atmospheric type must be one of the eight native types.");
        FrameAndType = SnesLittleEndian.Word(checked((byte)animationFrame), checked((byte)position));
    }

    /// <summary>$0AD4 array countdown in update calls; negative words suppress motion/drawing until decrement reaches $8000 and reloads the current frame duration.</summary>
    public WrappingCountdown AnimationTimer { get; internal set; }

    /// <summary>
    /// $0ADC/$0AE4 array anchor in whole room pixels; camera subtraction occurs only when
    /// drawing, motion wraps as native words, diving splashes follow the live FX surface and
    /// lava spray/dust rise one pixel per active update.
    /// </summary>
    public Position16<RoomPixels> Position { get; internal set; }

    /// <summary>Resets the packed word, delayed timer, and anchor during room or Samus initialization.</summary>
    internal void Clear()
    {
        FrameAndType = 0;
        AnimationTimer = WrappingCountdown.Start(0);
        Position = Position16<RoomPixels>.Zero;
    }
}
