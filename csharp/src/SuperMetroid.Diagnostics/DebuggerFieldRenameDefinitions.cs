using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Desktop;

/// <summary>
/// Exact historical field names whose meaning was generalized under a new name. A renamed
/// field remains part of the current layout, so count-based migrations are unaffected.
/// </summary>
internal static class DebuggerFieldRenameDefinitions
{
    // An explicit inventory, not permission to remap arbitrary unknown fields.
    private static readonly Dictionary<(Type DeclaringType, string Name), string> Renames = new()
    {
        // The post-Ceres countdown became the NMI-wait count of every $82:8000 load.
        // Its legacy -1 "not started" value reads as no pending wait.
        [(typeof(SuperMetroidGame), "postCeresLoadFramesRemaining")] = "gameLoadingWaitsRemaining",
        // Native word $0FF2 serves the death explosions and the escape-door dust alike.
        [(typeof(MotherBrainRainbowBeamAttackSequence), "<DeathExplosionIndex>k__BackingField")] =
            "<DeathAndEscapeExplosionIndex>k__BackingField",
        // #142: IDE0032 replaced these serialized fields with auto-properties; states saved
        // before then name the original fields.
        [(CoreType("SuperMetroid.Core.Assets.CeresDoorNormalPaintDefinitions"), "warm")] = "<WarmTargets>k__BackingField",
        [(CoreType("SuperMetroid.Core.Assets.GameplayHudPresentation+CompiledIcon"), "anchorOverride")] = "<Anchor>k__BackingField",
        [(CoreType("SuperMetroid.Core.Assets.MotherBrainHealthPalettePresentation+BasePalette"), "gray")] = "<Gray>k__BackingField",
        [(CoreType("SuperMetroid.Core.Assets.MotherBrainHealthPalettePresentation+BasePalette"), "outline")] = "<Outline>k__BackingField",
        [(typeof(SuperMetroid.Core.Assets.MotherBrainRainbowPalettePresentation), "beamInitial")] = "<BeamInitialColor>k__BackingField",
        [(CoreType("SuperMetroid.Core.Assets.MotherBrainRainbowPalettePresentation+PaletteFrame"), "trailing")] = "<TrailingColor>k__BackingField",
        [(typeof(SuperMetroid.Core.Assets.SamusBodyTileDefinition), "standaloneSecondSize")] = "<SecondSize>k__BackingField",
        [(typeof(SuperMetroid.Core.Audio.ExtractedAudioAssetCatalog), "contentIdentity")] = "<ContentIdentity>k__BackingField",
        [(typeof(SuperMetroid.Core.Audio.ExtractedAudioAssetCatalog), "soundLibraries")] = "<SoundLibraries>k__BackingField",
        [(typeof(SuperMetroid.Core.Audio.ExtractedAudioAssetCatalog), "soundPrograms")] = "<SoundPrograms>k__BackingField",
        [(typeof(SuperMetroid.Core.Frontend.CeresDepartureState), "brightness")] = "<Brightness>k__BackingField",
        [(typeof(SuperMetroid.Core.Frontend.CeresDepartureState), "holdFramesRemaining")] = "<HoldFramesRemaining>k__BackingField",
        [(CoreType("SuperMetroid.Core.Frontend.CreditsObjectState"), "enabled")] = "<Enabled>k__BackingField",
        [(CoreType("SuperMetroid.Core.Frontend.CreditsObjectState"), "scrollWhole")] = "<VerticalScroll>k__BackingField",
        [(CoreType("SuperMetroid.Core.Frontend.EndingBackgroundTextState"), "installedCompleted")] = "<Completed>k__BackingField",
        [(CoreType("SuperMetroid.Core.Frontend.IntroCinematicObjectSystem"), "caretX")] = "<CaretX>k__BackingField",
        [(CoreType("SuperMetroid.Core.Frontend.IntroCinematicObjectSystem"), "caretY")] = "<CaretY>k__BackingField",
        [(CoreType("SuperMetroid.Core.Frontend.IntroCinematicObjectSystem"), "spriteMapPointer")] = "<SpriteMapPointer>k__BackingField",
        [(typeof(SuperMetroid.Core.Frontend.SuperMetroidGame), "gameOptions")] = "<ConfiguredOptions>k__BackingField",
        [(typeof(SuperMetroid.Core.Frontend.SuperMetroidGame), "legacyPixels")] = "<lastPixels>k__BackingField",
        [(typeof(SuperMetroid.Core.Frontend.TitleSequenceState), "phase")] = "<Phase>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.CrocomireEnemyState), "_body")] = "<Body>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.KiHunterEnemyState), "_isWing")] = "<IsWing>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.PaletteFxHeatProgramDefinition), "frames")] = "<Frames>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.PhantoonEnemyState), "_blending")] = "<Blending>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.PhantoonEnemyState), "_wave")] = "<Wave>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.RoomEnemySystem), "_crocomire")] = "<Crocomire>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.RoomEnemySystem), "_crocomireDeath")] = "<CrocomireDeath>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.RoomEnemySystem), "_draygon")] = "<Draygon>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.RoomEnemySystem), "_kraidState")] = "<Kraid>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.RoomEnemySystem), "_motherBrain")] = "<MotherBrain>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.RoomEnemySystem), "_phantoonState")] = "<Phantoon>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.RoomEnemySystem), "_ridleyState")] = "<Ridley>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.SamusState), "_healthWarning")] = "<HealthWarning>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.SamusState), "_pose")] = "<Pose>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.SamusState), "_poseHistory")] = "<PoseHistory>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.ScrollBoundaryCamera), "_scrolls")] = "<Scrolls>k__BackingField",
        [(CoreType("SuperMetroid.Core.Game.ShaktoolInstructionProgramDefinitions+WordSelector"), "selected")] = "<Value>k__BackingField",
        [(CoreType("SuperMetroid.Core.Game.ShaktoolProjectileInstructionProgramDefinitions+WordSelector"), "selected")] = "<Value>k__BackingField",
        [(typeof(SuperMetroid.Core.Game.TourianStatueAnimatedTileProgramDefinition), "sourceOperandPointers")] = "<SourceOperandPointers>k__BackingField",
        [(CoreType("SuperMetroid.Core.Game.WorkRobotInstructionProgramDefinitions+WordSelector"), "selected")] = "<Value>k__BackingField",
        [(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime), "_roomSpikes")] = "<RoomSpikes>k__BackingField",
        [(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime), "_tourianStatues")] = "<TourianStatues>k__BackingField",
    };

    // Core-internal declaring types resolve by name; a missing name fails type initialization.
    private static Type CoreType(string fullName) =>
        typeof(SuperMetroidGame).Assembly.GetType(fullName, throwOnError: true)!;

    /// <summary>Returns the current name of a renamed serialized field.</summary>
    internal static bool TryGetCurrentName(Type declaringType, string serializedName, out string currentName) =>
        Renames.TryGetValue((declaringType, serializedName), out currentName!);
}
