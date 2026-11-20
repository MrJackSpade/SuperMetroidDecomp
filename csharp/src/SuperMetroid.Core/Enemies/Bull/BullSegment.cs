namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's idle and immune-shot animations (#641, #2477).
/// Their steps are <see cref="BullAnimations"/>.
/// </summary>
internal enum BullSegment
{
    /// <summary>Its idle sway (#2477): its tentacles upright, tilted, upright and tilted further, repeating.</summary>
    Idle,
    /// <summary>The quick cycle of its poses an immune shot sets off.</summary>
    Shot,
}
