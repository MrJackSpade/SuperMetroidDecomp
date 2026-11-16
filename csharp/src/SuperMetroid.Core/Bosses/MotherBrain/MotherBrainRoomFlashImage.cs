namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One image of the flash over Mother Brain's room in her fake death (#2259): each shows for
/// <see cref="MotherBrainRoomFlashDefinitions.FlashImageTicks"/> updates, and the flash loops.
/// </summary>
public readonly record struct MotherBrainRoomFlashImage
{
    /// <summary>The image at <paramref name="step"/> of the cycle.</summary>
    private MotherBrainRoomFlashImage(int step) => Step = step;

    /// <summary>The image's place in the cycle, from zero: the step of its installed shade.</summary>
    public int Step { get; }

    /// <summary>The cycle's first image.</summary>
    public static MotherBrainRoomFlashImage First { get; } = new(0);

    /// <summary>Every image of the cycle, in order: the fourteen the flash loops through.</summary>
    public static IReadOnlyList<MotherBrainRoomFlashImage> All { get; } = [.. Enumerable.Range(0, MotherBrainRoomFlashDefinitions.FlashFrameCount).Select(step => new MotherBrainRoomFlashImage(step))];

    /// <summary>The image after this one; the last is followed by the first.</summary>
    public MotherBrainRoomFlashImage Next => new((Step + 1) % MotherBrainRoomFlashDefinitions.FlashFrameCount);
}
