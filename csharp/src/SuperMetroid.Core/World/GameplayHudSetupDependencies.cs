using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="GameplayHudSetup"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record GameplayHudSetupDependencies
{
    /// <summary>The installed artwork the HUD's presentation and characters come from.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>The display state whose BG3 character base the HUD selects.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>The HUD tilemap initialization fills.</summary>
    public required HudState Hud { get; init; }

    /// <summary>The Samus whose inventory the HUD's presentation reflects.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The session whose OBJ character table enemy pictures are selected over.</summary>
    public required EnemySession EnemySession { get; init; }

    /// <summary>The room whose loaded enemy characters show again over the standard sprite characters.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Carries the HUD characters' and the loaded enemies' character uploads, which still go through the queue.</summary>
    public required VramWriteQueue VramWrites { get; init; }
}
