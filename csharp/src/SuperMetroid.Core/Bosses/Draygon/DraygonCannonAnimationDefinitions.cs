namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The authored timing of Draygon's wall cannons, by facing (#1554): a whole cannon shows its shield; a hit that does not
/// destroy it flashes the shield three times; a destroyed cannon smokes for good.
/// </summary>
internal static class DraygonCannonAnimationDefinitions
{
    /// <summary>How long the resting shield shows before the cannon rests.</summary>
    internal const int ShieldDuration = 8;
    /// <summary>How long the first of each pair of hit-flash images shows.</summary>
    internal const int FlashDuration = 3;
    /// <summary>How long the second of each pair of hit-flash images shows.</summary>
    internal const int FlashAlternateDuration = 4;
    /// <summary>How long each smoke image shows.</summary>
    internal const int SmokeDuration = 6;

    /// <summary>What each facing's cannon shows.</summary>
    internal static IReadOnlyDictionary<DraygonCannonOrientation, DraygonCannonLook> Looks { get; } =
        new Dictionary<DraygonCannonOrientation, DraygonCannonLook>
        {
            [DraygonCannonOrientation.Right] = new(
                new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldA), ShieldDuration),
                [
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldA), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldB), FlashAlternateDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldA), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldB), FlashAlternateDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldA), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightShieldB), FlashAlternateDuration),
                ],
                [
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightDamagedA), SmokeDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightDamagedB), SmokeDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightDamagedC), SmokeDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.RightDamagedD), SmokeDuration),
                ]),
            [DraygonCannonOrientation.Left] = new(
                new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldA), ShieldDuration),
                [
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldA), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldB), FlashAlternateDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldA), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldB), FlashAlternateDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldA), FlashDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftShieldB), FlashAlternateDuration),
                ],
                [
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftDamagedA), SmokeDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftDamagedB), SmokeDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftDamagedC), SmokeDuration),
                    new(DraygonCannonDraws.Family.Of(DraygonCannonDraw.LeftDamagedD), SmokeDuration),
                ]),
        };
}
