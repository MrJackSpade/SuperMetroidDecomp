using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>The room FX blend colours per effect (#633).</summary>
internal sealed record RoomFxPaletteBlendParts
{
    /// <summary>Lava.</summary>
    public required RoomFxBlendColors Lava { get; init; }

    /// <summary>Landing site rain.</summary>
    public required RoomFxBlendColors LandingSiteRain { get; init; }

    /// <summary>Maridia water a.</summary>
    public required RoomFxBlendColors MaridiaWaterA { get; init; }

    /// <summary>Water and acid.</summary>
    public required RoomFxBlendColors WaterAndAcid { get; init; }

    /// <summary>Fog.</summary>
    public required RoomFxBlendColors Fog { get; init; }

    /// <summary>Maridia water b.</summary>
    public required RoomFxBlendColors MaridiaWaterB { get; init; }

    /// <summary>Maridia water c.</summary>
    public required RoomFxBlendColors MaridiaWaterC { get; init; }

    /// <summary>Maridia water d.</summary>
    public required RoomFxBlendColors MaridiaWaterD { get; init; }

    /// <summary>Cosmetic fixed-color tint selected when Ceres Ridley is alive.</summary>
    public required PaletteRgb5 CeresHazeBlue { get; init; }

    /// <summary>Cosmetic fixed-color tint selected after Ceres Ridley is defeated.</summary>
    public required PaletteRgb5 CeresHazeRed { get; init; }
}
