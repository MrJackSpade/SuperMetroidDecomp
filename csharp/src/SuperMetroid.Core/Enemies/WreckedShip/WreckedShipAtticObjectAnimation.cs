using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship attic PLM's typed program (#1296).</summary>
internal sealed class WreckedShipAtticObjectAnimation()
    : RoomObjectProgramAnimation<WreckedShipAtticProgram, RoomObjectNoDraw, WreckedShipAtticCue>(WreckedShipAtticProgram.Attic)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<WreckedShipAtticProgram, RoomObjectNoDraw, WreckedShipAtticCue> Programs =>
        WreckedShipAtticObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(RoomObjectNoDraw frame) =>
        throw new InvalidOperationException($"The attic program never draws; got {frame}.");

    /// <inheritdoc/>
    private protected override WreckedShipAtticProgram? Publish(WreckedShipAtticCue cue, RoomObjectSlot slot, RoomObjectStepContext context)
    {
        if (cue != WreckedShipAtticCue.InstallInertFrameBehavior)
            throw new InvalidOperationException($"Undefined {nameof(WreckedShipAtticCue)} {cue}.");
        slot.FrameBehavior = WreckedShipAtticFrameBehaviors.NoOpCallback;
        return null;
    }
}
