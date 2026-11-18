using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The authored timing of Draygon's wall cannons, by facing (#1554): a whole cannon shows its shield; a hit that does not
/// destroy it flashes the shield three times; a destroyed cannon sparks for good.
/// </summary>
internal static class DraygonCannonAnimationDefinitions
{
    /// <summary>How long the resting shield shows before the cannon rests.</summary>
    internal const int ShieldDuration = 8;
    /// <summary>How long the first of each pair of hit-flash images shows.</summary>
    internal const int FlashDuration = 3;
    /// <summary>How long the second of each pair of hit-flash images shows.</summary>
    internal const int FlashAlternateDuration = 4;
    /// <summary>How long each picture of a destroyed cannon's sparks shows.</summary>
    internal const int SparkDuration = 6;

    /// <summary>What each facing's cannon shows.</summary>
    internal static IReadOnlyDictionary<DraygonCannonOrientation, DraygonCannonLook> Looks { get; } =
        new Dictionary<DraygonCannonOrientation, DraygonCannonLook>
        {
            [DraygonCannonOrientation.Right] = new(
                new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShield), ShieldDuration),
                [
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShield), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldFlash), FlashAlternateDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShield), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldFlash), FlashAlternateDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShield), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldFlash), FlashAlternateDuration),
                ],
                [
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightSparksPeakingAt11And20), SparkDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightSparksPeakingAt3And28), SparkDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightSparksPeakingAt6And25), SparkDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightSparksFlat), SparkDuration),
                ]),
            [DraygonCannonOrientation.Left] = new(
                new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShield), ShieldDuration),
                [
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShield), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldFlash), FlashAlternateDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShield), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldFlash), FlashAlternateDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShield), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldFlash), FlashAlternateDuration),
                ],
                [
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftSparksPeakingAt11And20), SparkDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftSparksPeakingAt3And28), SparkDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftSparksPeakingAt6And25), SparkDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftSparksFlat), SparkDuration),
                ]),
        };
}
