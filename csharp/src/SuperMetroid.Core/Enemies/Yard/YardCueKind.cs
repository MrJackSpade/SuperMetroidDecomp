namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>What a cue of the Yard's programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum YardCueKind
{
    /// <summary>Moves the Yard by a signed pixel offset in X and Y (<c>MoveByPixelsInY</c>, $A3:CC5F).</summary>
    MoveBy,
    /// <summary>While hiding, or on a 50% chance, shows the previous pose again; otherwise runs on (<c>GoBack4BytesIfHidingOr50PercentChance</c>, $A3:CC78).</summary>
    RepeatPreviousPoseUnlessContinuing,
    /// <summary>Sets the attachment direction and the airborne facing it implies (<c>DirectionInY</c>, $A3:CC48).</summary>
    SetDirection,
    /// <summary>Names the animation the Yard hides with, or none (<c>HidingInstListInY</c>, $A3:CC3F).</summary>
    SetHidingProgram,
    /// <summary>Selects the movement function main AI runs (<c>MovementFunctionInY</c>, $A3:CC36).</summary>
    SetMovementFunction,
}
