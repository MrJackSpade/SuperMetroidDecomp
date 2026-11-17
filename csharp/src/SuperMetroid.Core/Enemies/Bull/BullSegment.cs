namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's normal and immune-shot animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="BullAnimations"/>.
/// </summary>
internal enum BullSegment
{
    /// <summary>Its tentacles swaying: upright, tilted, upright and tilted further, repeating.</summary>
    Normal,
    /// <summary>The quick cycle of its poses an immune shot sets off.</summary>
    Shot,
}
