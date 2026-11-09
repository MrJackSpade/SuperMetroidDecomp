using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and physical motion for Ceres destruction and Zebes reveal.</summary>
internal static class CeresDestructionActorDefinitions
{
    /// <summary>Number of persistent scene actors allocated when the destruction begins.</summary>
    public const int InitialActorCount = 3;

    /// <summary>Number of reveal actors allocated when Zebes is revealed.</summary>
    public const int ZebesActorCount = 6;

    /// <summary>The Zebes planet actor: its initializer,
    /// waiting callback and list; its operands set its placement/palette.
    /// Its slide callback accelerates by 64/256 pixels.</summary>
    internal static CeresDestructionActorDefinition ZebesPlanet =>
        new(CinematicSpriteFrameBehavior.Zebes, CinematicSpriteProgram.ZebesPlanet,
            new(136, 111), CeresSpriteColouring.Zebes, 0, false, 64, CinematicSpriteFrameBehavior.ZebesSlideSceneAway, false);

    /// <summary>The PLANET ZEBES text actor: its initializer,
    /// an empty callback and its list. Its operands center the title at (128,186), in the cinematic's first sprite colours.
    /// Its program deletes before scene sliding, so it has no slide callback or acceleration.</summary>
    internal static CeresDestructionActorDefinition ZebesTitle =>
        new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteProgram.ZebesTitle,
            new(128, 186), CeresSpriteColouring.ZebesTitle, 0, false, 0, null, false);

    /// <summary>The four star sheets' programs, indexed by row-major quadrant.</summary>
    internal static readonly CinematicSpriteProgram[] ZebesStarSheets =
    [
        CinematicSpriteProgram.ZebesStarSheet0, CinematicSpriteProgram.ZebesStarSheet1,
        CinematicSpriteProgram.ZebesStarSheet2, CinematicSpriteProgram.ZebesStarSheet3,
    ];
}
