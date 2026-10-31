using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// Where an eye door's attack, sweat and smoke start, how the sweat is launched and how the smoke looks (#1414).
/// Offsets are from the top-left corner of the eye's block.
/// </summary>
internal static class EyeDoorEffectDefinitions
{
    /// <summary>A left-facing eye's attack starts half a block left of the eye, at the top of the block below.</summary>
    internal static readonly Offset16<RoomPixels> LeftShotOrigin = new(-8, 16);

    /// <summary>A right-facing eye's attack starts half a block right of the eye, at the top of the block below.</summary>
    internal static readonly Offset16<RoomPixels> RightShotOrigin = new(24, 16);

    /// <summary>The sweat starts eight pixels left of the eye block, at the top of the block below it.</summary>
    internal static readonly Offset16<RoomPixels> SweatOrigin = new(-8, 16);

    /// <summary>A left-facing eye's sweat drifts left a quarter pixel and falls two pixels per update.</summary>
    internal static readonly EightEightVector<RoomPixels> LeftSweatLaunch = new(-64, 512);

    /// <summary>A right-facing eye's sweat drifts right a quarter pixel and falls two pixels per update.</summary>
    internal static readonly EightEightVector<RoomPixels> RightSweatLaunch = new(64, 512);

    /// <summary>The dust animation of the opening's scattered smoke puffs.</summary>
    internal const MiscDustAnimation ScatteredSmokeAnimation = MiscDustAnimation.CorpseDustCloud;

    /// <summary>The scattered puffs land anywhere in a centred 16x32-pixel box (misc-dust placement three).</summary>
    internal const int ScatteredSmokePlacement = 3;

    /// <summary>The dust animation of the smoke puff centred on the eye.</summary>
    internal const MiscDustAnimation CenteredSmokeAnimation = MiscDustAnimation.EyeDoorSweatDrop;

    /// <summary>The centred puff appears at a single point (misc-dust placement zero).</summary>
    internal const int CenteredSmokePlacement = 0;
}
