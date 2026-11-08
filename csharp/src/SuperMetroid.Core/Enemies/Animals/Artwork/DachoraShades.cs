using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The shades a Dachora's pictures are pre-coloured in (#1512): its normal colours, and each frame of its speed and
/// stored-shine glows. The import colours each from Dachora's installed colours.
/// </summary>
internal static class DachoraShades
{
    /// <summary>The effect each colour phase shows: its normal colours, the speed glow or the stored-shine glow.</summary>
    internal static IReadOnlyDictionary<DachoraColourPhase, string> Effects { get; } = new Dictionary<DachoraColourPhase, string>
    {
        [DachoraColourPhase.Default] = "dachora-normal",
        [DachoraColourPhase.Speed] = "dachora-speed",
        [DachoraColourPhase.Shine] = "dachora-shine",
    };

    /// <summary>The shade a Dachora shows in <paramref name="phase"/> at glow frame <paramref name="frame"/>.</summary>
    internal static PictureShade Of(DachoraColourPhase phase, int frame) => new(Effects[phase], frame);
}
