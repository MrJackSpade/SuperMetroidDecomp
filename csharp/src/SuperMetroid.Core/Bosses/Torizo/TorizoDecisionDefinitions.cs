namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The health, distances, counts and places at which Bomb and Golden Torizo's AI and animation branches decide (#1945).
/// Distances are whole pixels between Torizo's and Samus's X; a range's nearest value is included and its farthest is not.
/// </summary>
internal static partial class TorizoDecisionDefinitions
{
    /// <summary>Bomb Torizo's gut blows up once its health falls below this, and below it either Torizo drools.</summary>
    internal const int GutBlowUpHealth = 350;

    /// <summary>Bomb Torizo's face blows up once its health falls below this.</summary>
    internal const int FaceBlowUpHealth = 100;

    /// <summary>
    /// The corner of Golden Torizo's room past which Samus wakes it: she must be below its Y and right of its X.
    /// </summary>
    internal static readonly Position16<RoomPixels> GoldenWakeCorner = new(368, 320);

    /// <summary>Golden Torizo's Y standing on its room's floor: its awakening's drop replays until it stands there.</summary>
    internal const int GoldenFloorY = 375;

    /// <summary>Golden Torizo's radii once its fight starts.</summary>
    internal static readonly Extent16<RoomPixels> GoldenFightRadius = new(18, 48);

    /// <summary>Bomb Torizo takes the first of its two attacks while Samus carries fewer missiles than this.</summary>
    internal const int BombFirstAttackMissiles = 5;

    /// <summary>Golden Torizo takes the first of its two attacks while Samus carries fewer missiles than this.</summary>
    internal const int GoldenFirstAttackMissiles = 32;

    /// <summary>Golden Torizo releases its eggs only at or below this health.</summary>
    internal const int GoldenEggsHealth = 1928;

    /// <summary>A stunned Golden Torizo staggers only above this health.</summary>
    internal const int GoldenStaggerHealth = 10800;

    /// <summary>Golden Torizo sits on Samus morphed behind it from this distance.</summary>
    internal const int MorphedBehindNearest = 4;

    /// <summary>Golden Torizo sits on Samus morphed behind it below this distance.</summary>
    internal const int MorphedBehindFarthest = 40;

    /// <summary>Golden Torizo's medium-range attack can be taken from this distance.</summary>
    internal const int MediumRangeNearest = 32;

    /// <summary>Golden Torizo's medium-range attack can be taken below this distance.</summary>
    internal const int MediumRangeFarthest = 96;

    /// <summary>Golden Torizo leaps forward at Samus from this distance.</summary>
    internal const int ForwardLeapDistance = 112;

    /// <summary>
    /// The consecutive frames Samus may space-jump before Golden Torizo's forward leap goes without its input and random
    /// gates: past this many it always goes.
    /// </summary>
    internal const int SpaceJumpLeapFrames = 360;

    /// <summary>Golden Torizo jumps back after this many stepping cues since its last decision, wherever Samus is.</summary>
    internal const int RepeatedJumpSteps = 8;

    /// <summary>Either Torizo jumps back from Samus in front of it below this distance.</summary>
    internal const int CloseInFrontDistance = 32;

    /// <summary>Bomb Torizo's close-behind branch is taken below this distance.</summary>
    internal const int CloseBehindDistance = 56;

    /// <summary>The updates a walking Golden Torizo waits before it turns toward Samus behind it.</summary>
    internal const int GoldenTurnDelay = 16;

    /// <summary>The updates a walking Bomb Torizo waits before it turns toward Samus behind it.</summary>
    internal const int BombTurnDelay = 72;
}
