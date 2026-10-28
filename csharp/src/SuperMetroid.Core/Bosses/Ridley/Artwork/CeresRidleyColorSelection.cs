using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Ceres Ridley's selected colour families, with the retreat colours kept only where they differ from their owners.</summary>
internal sealed record CeresRidleyColorSelection
{
    /// <summary>Startup colors also supply the unchanged retreat door/container entries.</summary>
    public required CeresRidleyStartColors Start { get; init; }

    /// <summary>Three-color fade rows used by the eyes and the retreat background composition.</summary>
    public required CeresRidleyFade EyeFade { get; init; }

    /// <summary>Eleven-color fade rows shared by the body shadow and retreat background.</summary>
    public required CeresRidleyFade BodyFade { get; init; }

    /// <summary>Health paint resolves its unchanged colors through the body and eye fade families.</summary>
    public required CeresRidleyHealthPaint Health { get; init; }

    /// <summary>The reflected self-destruct text cycle is independent of Ridley's body fades.</summary>
    public required CeresRidleyAlarmColors Alarm { get; init; }

    /// <summary>Only retreat BG colors that differ from the body, eye, and neutral composition.</summary>
    public required Dictionary<ColourCell, RgbColor> RetreatBg { get; init; }

    /// <summary>Only supplied differences from the first eight door paints: native $A6:AA01-AA10 repeats $A6:E171-E180.</summary>
    public required Dictionary<ColourCell, RgbColor> RetreatShared { get; init; }

    /// <summary>Private Baby draw shades retain their link to the body fade family.</summary>
    public required CeresBabyPaint Baby { get; init; }
}
