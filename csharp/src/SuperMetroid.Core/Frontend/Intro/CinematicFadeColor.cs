namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// One colour of bank-$8B's cinematic cross-fader as its three 8.8 component accumulators. The native code keeps the
/// three components in separate WRAM tables; the arithmetic on each is the same, so a colour steps as one value.
/// </summary>
/// <param name="Red">The red 8.8 component.</param>
/// <param name="Green">The green 8.8 component.</param>
/// <param name="Blue">The blue 8.8 component.</param>
[EncodingOwner("cinematic cross-fade 8.8 colour components")]
internal readonly record struct CinematicFadeColor(ushort Red, ushort Green, ushort Blue)
{
    /// <summary>The live 8.8 value of a target colour: each five-bit component as the whole part.</summary>
    internal static CinematicFadeColor Target(Bgr555 color) => new(
        (ushort)SnesEightEight.FromWhole((ushort)color.Red),
        (ushort)SnesEightEight.FromWhole((ushort)color.Green),
        (ushort)SnesEightEight.FromWhole((ushort)color.Blue));

    /// <summary>One thirty-second of a target colour in 8.8 form: <c>component &lt;&lt; 3</c>, the literal shifts at $8B:8C19-$8C59.</summary>
    internal static CinematicFadeColor Step(Bgr555 color) => new(
        (ushort)(color.Red << 3), (ushort)(color.Green << 3), (ushort)(color.Blue << 3));

    /// <summary>One step toward black. Native SBC stores a wrapping 16-bit result in each component.</summary>
    internal CinematicFadeColor Minus(CinematicFadeColor step) => new(
        unchecked((ushort)(Red - step.Red)), unchecked((ushort)(Green - step.Green)), unchecked((ushort)(Blue - step.Blue)));

    /// <summary>
    /// One step toward the target. The wrap to thirteen bits (the $1FFF mask) is not a generic clamp: each of the three
    /// retail ADC paths applies it after every addition.
    /// </summary>
    internal CinematicFadeColor PlusWrapped(CinematicFadeColor step) => new(
        (ushort)((Red + step.Red) % 8192), (ushort)((Green + step.Green) % 8192), (ushort)((Blue + step.Blue) % 8192));

    /// <summary>The visible colour: each component's high byte, as the following NMI upload writes it into CGRAM.</summary>
    internal Bgr555 Visible => new(
        SnesLittleEndian.HighByte(Red) & Bgr555.MaxChannel,
        SnesLittleEndian.HighByte(Green) & Bgr555.MaxChannel,
        SnesLittleEndian.HighByte(Blue) & Bgr555.MaxChannel);
}
