using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One tube the collapse about Mother Brain's tank drops in her fake death (#2258): the blocks clearing its place leaves.
/// What falls, and whether a tube follows it, is the drop's own kind.
/// </summary>
/// <param name="Clears">The room mutation clearing the tube's place, and the block it is spawned at.</param>
internal abstract partial record MotherBrainTubeDrop(RoomObjectSpawnRequest<MotherBrainRoomMutation> Clears)
{
    /// <summary>
    /// Whether the tube falls once enough enemy projectiles are free, as the collapse's first does, rather than once the
    /// wait its predecessor's clear started runs out.
    /// </summary>
    internal bool WaitsForFreeProjectiles { get; init; }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this drop, through the drop it is (#2553).</summary>
    /// <typeparam name="TContext">What the drop's reaction reads besides the drop.</typeparam>
    /// <param name="reactions">What the collapse does on each drop.</param>
    /// <param name="context">What the drop's reaction reads besides the drop, handed to it so the reactions are built once.</param>
    internal abstract void React<TContext>(MotherBrainTubeDropReactions<TContext> reactions, TContext context);
}
