using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and physical motion for Ceres destruction and Zebes reveal.</summary>
internal static class CeresDestructionActorDefinitions
{
    /// <summary>Number of persistent scene actors allocated by <c>$8B:C27C-$C295</c>.</summary>
    public const int InitialActorCount = 3;

    /// <summary>Number of reveal actors allocated by <c>$8B:C810-$C831</c>.</summary>
    public const int ZebesActorCount = 6;

    /// <summary>$8B:CEA3, CinematicSpriteObjectDefinitions_Zebes: initializer C83B,
    /// waiting callback C84E, list CCAB; operands C83C/C842/C848 set its placement/palette.
    /// C857 selects slide callback C85D, whose C862 operand accelerates by 64/256 pixels.</summary>
    internal static CeresDestructionActorDefinition ZebesPlanet =>
        new(CinematicSpritePreInstruction.Zebes, CinematicSpriteProgram.ZebesPlanet,
            new(136, 111), 7, 0, false, 64, CinematicSpritePreInstruction.ZebesSlideSceneAway, false);

    /// <summary>$8B:CEAF, CinematicSpriteObjectDefinitions_PlanetZebesText: initializer C992,
    /// no-op 93D9, list CCBB. Operands C993/C999/C99F center the title at (128,186), palette zero.
    /// Its program deletes before scene sliding, so it has no slide callback or acceleration.</summary>
    internal static CeresDestructionActorDefinition ZebesTitle =>
        new(CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.ZebesTitle,
            new(128, 186), 0, 0, false, 0, null, false);

    /// <summary>The four star sheets' programs, indexed by row-major quadrant.</summary>
    internal static readonly CinematicSpriteProgram[] ZebesStarSheets =
    [
        CinematicSpriteProgram.ZebesStarSheet0, CinematicSpriteProgram.ZebesStarSheet1,
        CinematicSpriteProgram.ZebesStarSheet2, CinematicSpriteProgram.ZebesStarSheet3,
    ];
}
