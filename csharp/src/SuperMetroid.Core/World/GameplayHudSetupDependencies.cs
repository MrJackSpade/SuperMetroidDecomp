using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="GameplayHudSetup"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record GameplayHudSetupDependencies
{
    /// <summary>The installed artwork the HUD's presentation and characters come from.</summary>
    public required RuntimeArtwork Artwork { get; init; }


    /// <summary>The HUD state initialization fills.</summary>
    public required HudState Hud { get; init; }
}
