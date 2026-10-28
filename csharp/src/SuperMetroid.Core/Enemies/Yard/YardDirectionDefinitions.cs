namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Fixed direction and airborne-animation definitions for Yard.</summary>
internal static class YardDirectionDefinitions
{
    /// <summary>
    /// $A3:CD42, $CDC2 and $CDD2: each of the eight surface orientations' crawling and hiding animations, airborne
    /// facing, opposite direction and native movement callback, by direction number: right-side wall moving up and
    /// down, left-side wall moving up and down, ceiling moving left and right, floor moving left and right. Direction
    /// pairs reverse motion on the same surface. Property bit zero selects positive motion and bit one vertical motion.
    /// </summary>
    internal static readonly YardDirectionDefinition[] Directions =
    [
        new(YardProgram.CrawlingUpsideRightMovingUp, 2,
            YardProgram.HidingUpsideRightMovingUp, YardAirborneFacing.Left, 1,
            YardMovementFunction.CrawlingUpsideDownMovingLeft),
        new(YardProgram.CrawlingUpsideRightMovingDown, 3,
            YardProgram.HidingUpsideRightMovingDown, YardAirborneFacing.Right, 0,
            YardMovementFunction.CrawlingUpsideRightMovingDown),
        new(YardProgram.CrawlingUpsideLeftMovingUp, 2,
            YardProgram.HidingUpsideLeftMovingUp, YardAirborneFacing.Right, 3,
            YardMovementFunction.CrawlingUpsideLeftMovingUp),
        new(YardProgram.CrawlingUpsideLeftMovingDown, 3,
            YardProgram.HidingUpsideLeftMovingDown, YardAirborneFacing.Left, 2,
            YardMovementFunction.CrawlingUpsideLeftMovingDown),
        new(YardProgram.CrawlingUpsideDownMovingLeft, 0,
            YardProgram.HidingUpsideDownMovingLeft, YardAirborneFacing.Right, 5,
            YardMovementFunction.CrawlingUpsideRightMovingUp),
        new(YardProgram.CrawlingUpsideDownMovingRight, 1,
            YardProgram.HidingUpsideDownMovingRight, YardAirborneFacing.Left, 4,
            YardMovementFunction.CrawlingUpsideDownMovingRight),
        new(YardProgram.CrawlingUpsideUpMovingLeft, 0,
            YardProgram.HidingUpsideUpMovingLeft, YardAirborneFacing.Left, 7,
            YardMovementFunction.CrawlingUpsideUpMovingLeft),
        new(YardProgram.CrawlingUpsideUpMovingRight, 1,
            YardProgram.HidingUpsideUpMovingRight, YardAirborneFacing.Right, 6,
            YardMovementFunction.CrawlingUpsideUpMovingRight),
    ];

    /// <summary>
    /// The visible and hidden airborne animations by facing. Native detach, contact-kick and shot-launch repeat these
    /// pairs at $A3:D1AB, $D50F and $D5A4.
    /// </summary>
    internal static readonly Dictionary<YardAirborneFacing, YardAirborneInstructionDefinition> AirborneFacings = new()
    {
        [YardAirborneFacing.Left] = new(YardProgram.AirborneFacingLeft, YardProgram.HiddenUpsideUpMovingLeft),
        [YardAirborneFacing.Right] = new(YardProgram.AirborneFacingRight, YardProgram.HiddenUpsideUpMovingRight),
    };
}
