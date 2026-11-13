namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's normal and immune-shot animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="BullAnimations"/>.
/// </summary>
internal enum BullSegment
{
    /// <summary>The normal animation.</summary>
    Normal,
    /// <summary>The shot animation.</summary>
    Shot,
    /// <summary>The shot loop animation.</summary>
    ShotLoop,
}
