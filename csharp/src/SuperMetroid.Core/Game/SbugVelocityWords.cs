namespace SuperMetroid.Core.Game;

/// <summary>
/// The exact two-word representation used for one signed 16.16 Sbug velocity. Keeping the
/// words separate is intentional: $A3:A52A negates each word independently, preserving a
/// documented cartridge bug that differs from negating the combined 32-bit value.
/// </summary>
/// <param name="Pixel">High word of a pixels-per-AI-update displacement; signed for side/custom velocities, unsigned for forward magnitudes.</param>
/// <param name="Subpixel">Low word of the displacement, in units of 1/65536 pixel.</param>
public readonly record struct SbugVelocityWords(ushort Pixel, ushort Subpixel)
{
    /// <summary>The two native words interpreted as one signed host displacement.</summary>
    public int SignedFixed => unchecked((int)SnesSixteenSixteen.Compose(Pixel, Subpixel));

    /// <summary>The unchanged 32 bits used by the cartridge's modular position additions.</summary>
    public uint RawFixed => SnesSixteenSixteen.Compose(Pixel, Subpixel);
}
