using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The Ceres Ridley colour tables a Ceres Ridley colour catalog is built from (#633).</summary>
internal sealed record CeresRidleyColorParts
{
    /// <summary>Start.</summary>
    public required Bgr555[] Start { get; init; }

    /// <summary>Eye fade.</summary>
    public required CeresRidleyFadeColorDefinitions EyeFade { get; init; }

    /// <summary>Body fade.</summary>
    public required CeresRidleyFadeColorDefinitions BodyFade { get; init; }

    /// <summary>Health.</summary>
    public required Bgr555[][] Health { get; init; }

    /// <summary>Alarm.</summary>
    public required CeresRidleyAlarmColorDefinitions Alarm { get; init; }

    /// <summary>Retreat bg.</summary>
    public required Bgr555[] RetreatBg { get; init; }

    /// <summary>Retreat shared.</summary>
    public required Bgr555[] RetreatShared { get; init; }

    /// <summary>Baby.</summary>
    public required CeresBabyPaintDefinitions Baby { get; init; }
}
