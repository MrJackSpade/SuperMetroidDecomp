using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>
/// Installed-content types the audit covered under a resource-suffixed name before #1452 renamed them, and that no
/// reviewed closed contract names. They are declared by type, so renaming one breaks the build instead of silently
/// dropping its consumers from the audit.
/// </summary>
internal static class InstalledContentTypes
{
    /// <summary>The full names of the declared installed-content types.</summary>
    internal static IReadOnlySet<string> Names { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        typeof(CeresDestructionArtwork).FullName!,
        typeof(CeresFlightArtwork).FullName!,
        typeof(EndingFloorArtwork).FullName!,
        typeof(EndingPaletteSet).FullName!,
        typeof(GameOverBabyPaint).FullName!,
        typeof(IntroCinematicArtwork).FullName!,
        typeof(MapSpriteCompositions).FullName!,
        typeof(InstalledRoomArtwork).FullName!,
    };
}
