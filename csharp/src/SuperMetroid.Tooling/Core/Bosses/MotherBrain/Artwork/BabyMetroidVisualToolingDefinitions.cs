
namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Development-tool members of <see cref="BabyMetroidVisualDefinitions"/>; never linked by player hosts.</summary>
internal static class BabyMetroidVisualToolingDefinitions
{
    /// <summary>$A9:F9A8: the closed pulse pose.</summary>
    internal const ushort Closed = 0xf9a8;
    /// <summary>$A9:FA40: the half-open pulse pose.</summary>
    internal const ushort HalfOpen = 0xfa40;
    /// <summary>$A9:FAD8: the wide-open pulse pose.</summary>
    internal const ushort WideOpen = 0xfad8;
    /// <summary>The spritemap of pulse pose <paramref name="pose"/>.</summary>
    internal static ushort Spritemap(BabyMetroidFrame pose) => pose switch
    {
        BabyMetroidFrame.Closed => Closed,
        BabyMetroidFrame.HalfOpen => HalfOpen,
        BabyMetroidFrame.WideOpen => WideOpen,
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, "The Baby Metroid has three pulse poses."),
    };
}
