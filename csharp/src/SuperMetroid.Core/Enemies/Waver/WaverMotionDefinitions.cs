namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>How a Waver flies and waves (#2156).</summary>
internal static class WaverMotionDefinitions
{
    /// <summary>How fast a Waver flies sideways, a pixel and a half an update.</summary>
    internal static SignedSixteenSixteen HorizontalSpeed { get; } = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(384);

    /// <summary>How far above and below its line a Waver's wave reaches, in pixels.</summary>
    internal const int WaveHeight = 4;
}
