namespace SuperMetroid.Core.Enemies;

/// <summary>Whose colour effects a boss attaches to the room (#2194).</summary>
internal enum BossShadingLayer
{
    /// <summary>Ridley's colours, in Ceres or Lower Norfair (#2231).</summary>
    Ridley,
    /// <summary>Ceres's escape's colours: the getaway floor's zoom and the self-destruct warning's alarm (#2228).</summary>
    CeresEscape,
    /// <summary>Spore Spawn's colours.</summary>
    SporeSpawn,
    /// <summary>Phantoon's colours.</summary>
    Phantoon,
    /// <summary>Draygon's colours.</summary>
    Draygon,
    /// <summary>Crocomire's colours.</summary>
    Crocomire,
    /// <summary>Kraid's room backdrop's colours.</summary>
    KraidBackdrop,
    /// <summary>Kraid's own colours.</summary>
    Kraid,
}
