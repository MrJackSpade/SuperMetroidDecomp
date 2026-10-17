namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The three COLDATA writes, each with its channel-select bits, as one producer published them
/// for an accepted NMI. NMI <c>$80:91EE</c> uploads the WRAM shadows to <c>$2132</c>, so the
/// main loop's writes become visible only with the following displayed frame.
/// </summary>
/// <param name="Red">The first COLDATA write, 0 through <see cref="MaximumWrite"/>; its low five bits are its intensity.</param>
/// <param name="Green">The second COLDATA write, 0 through <see cref="MaximumWrite"/>; its low five bits are its intensity.</param>
/// <param name="Blue">The third COLDATA write, 0 through <see cref="MaximumWrite"/>; its low five bits are its intensity.</param>
public readonly record struct FixedColorRegisterSnapshot(int Red, int Green, int Blue)
{
    /// <summary>The largest COLDATA write: three channel-select bits over a five-bit intensity.</summary>
    public const int MaximumWrite = byte.MaxValue;

    /// <summary>The intensity bits of one COLDATA write.</summary>
    private const int IntensityMask = Bgr555.MaxChannel;

    /// <summary>The first COLDATA write.</summary>
    public int Red
    {
        get => red;
        init => red = Ensure.BetweenInclusive(0, MaximumWrite, value);
    }

    /// <summary>Validated store of <see cref="Red"/>.</summary>
    private readonly int red = Ensure.BetweenInclusive(0, MaximumWrite, Red);

    /// <summary>The second COLDATA write.</summary>
    public int Green
    {
        get => green;
        init => green = Ensure.BetweenInclusive(0, MaximumWrite, value);
    }

    /// <summary>Validated store of <see cref="Green"/>.</summary>
    private readonly int green = Ensure.BetweenInclusive(0, MaximumWrite, Green);

    /// <summary>The third COLDATA write.</summary>
    public int Blue
    {
        get => blue;
        init => blue = Ensure.BetweenInclusive(0, MaximumWrite, value);
    }

    /// <summary>Validated store of <see cref="Blue"/>.</summary>
    private readonly int blue = Ensure.BetweenInclusive(0, MaximumWrite, Blue);

    /// <summary>The five-bit intensity of <see cref="Red"/>, 0 through <see cref="Bgr555.MaxChannel"/>.</summary>
    public int RedIntensity => Red & IntensityMask;

    /// <summary>The five-bit intensity of <see cref="Green"/>, 0 through <see cref="Bgr555.MaxChannel"/>.</summary>
    public int GreenIntensity => Green & IntensityMask;

    /// <summary>The five-bit intensity of <see cref="Blue"/>, 0 through <see cref="Bgr555.MaxChannel"/>.</summary>
    public int BlueIntensity => Blue & IntensityMask;
}
