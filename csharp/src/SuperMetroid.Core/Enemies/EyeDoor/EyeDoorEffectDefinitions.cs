using SuperMetroid.Core.Enemies.Projectiles;
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
    internal static readonly EightEightVector<RoomPixels> LeftSweatLaunch = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-64, 512);

    /// <summary>A right-facing eye's sweat drifts right a quarter pixel and falls two pixels per update.</summary>
    internal static readonly EightEightVector<RoomPixels> RightSweatLaunch = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(64, 512);

    /// <summary>A falling sweat drop's downward acceleration per update: twelve 256ths of a pixel per update.</summary>
    internal static readonly SignedEightEight SweatGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(12);

    /// <summary>How far a sweat drop that lands on the floor rises, in pixels, as it splashes.</summary>
    internal const int SweatSplashRise = 4;

    /// <summary>The middle of the eye's block, from which every smoke puff's placement is measured.</summary>
    internal static readonly Offset16<RoomPixels> SmokeCentre = new(8, 8);

    /// <summary>The dust animation of the opening's scattered smoke puffs.</summary>
    internal const DustExplosionAnimation ScatteredSmokeAnimation = DustExplosionAnimation.CorpseDustCloud;

    /// <summary>The scattered puffs land anywhere in a centred box 16 pixels wide and 32 tall.</summary>
    internal static DustExplosionPlacement ScatteredSmokePlacement => SmokePuffPlacements.CentredSixteenByThirtyTwoBox;

    /// <summary>The dust animation of the smoke puff centred on the eye.</summary>
    internal const DustExplosionAnimation CenteredSmokeAnimation = DustExplosionAnimation.EyeDoorSmokePuff;

    /// <summary>The centred puff appears at a single point.</summary>
    internal static DustExplosionPlacement CenteredSmokePlacement => SmokePuffPlacements.Point;
}
