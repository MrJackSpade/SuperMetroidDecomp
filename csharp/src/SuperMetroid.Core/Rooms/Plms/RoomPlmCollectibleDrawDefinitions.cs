
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Compiled collectible draw geometry and block. Only the low visual
/// twelve bits may be overridden; pickup, collision, and persistence stay native.
/// </summary>
internal static class RoomPlmCollectibleDrawDefinitions
{
    /// <summary>The visual block of an orb's first frame; an orb is shootable.</summary>
    internal const int OrbFirstBlock = 114;
    /// <summary>The visual block of an orb's burst; the burst is solid.</summary>
    internal const int OrbBurstBlock = 117;
    /// <summary>The visual block of a tank's first frame; a tank is a special block.</summary>
    internal const int TankFirstBlock = 74;
    /// <summary>The visual block of a dynamic collectible's first frame; it is a special block.</summary>
    internal const int DynamicFirstBlock = 142;
    /// <summary>The visual block of a shot reveal's first frame; the reveal is solid.</summary>
    internal const int ShotRevealFirstBlock = 83;

    /// <summary>The six collectible draw groups, in the order their frames are published.</summary>
    internal enum DrawGroup
    {
        /// <summary>Air tile after an item is removed.</summary>
        Empty,
        /// <summary>Three shootable Chozo orb animation frames.</summary>
        Orb,
        /// <summary>Solid burst frame when a Chozo orb opens.</summary>
        OrbBurst,
        /// <summary>Two visible frames for each of four tank pickup kinds.</summary>
        Tank,
        /// <summary>Two frames for each dynamically allocated item graphics slot.</summary>
        Dynamic,
        /// <summary>Three solid frames of a shot-block item reveal.</summary>
        ShotReveal,
    }

    /// <summary>One collectible draw: a group and the frame within it.</summary>
    /// <param name="Group">The group of frames the draw belongs to.</param>
    /// <param name="Index">The frame within the group, counted from zero.</param>
    internal readonly record struct Draw(DrawGroup Group, [property: IntegerQuantity("frame within the collectible draw group")] int Index);

    /// <summary>The air tile left after an item is removed.</summary>
    internal static PlmFrameDraw<Draw> Empty { get; } = PlmDraw.Of(new Draw(DrawGroup.Empty, 0));

    /// <summary>The solid burst frame drawn when a Chozo orb opens.</summary>
    internal static PlmFrameDraw<Draw> OrbBurst { get; } = PlmDraw.Of(new Draw(DrawGroup.OrbBurst, 0));

    /// <summary>The tank kinds in the order of their draws.</summary>
    internal static readonly InWorldCollectibleKind[] TankKinds =
    [
        InWorldCollectibleKind.EnergyTank, InWorldCollectibleKind.MissileTank,
        InWorldCollectibleKind.SuperMissileTank, InWorldCollectibleKind.PowerBombTank,
    ];
}
