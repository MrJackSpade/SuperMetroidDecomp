using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Enemies.WreckedShip.WreckedShipAtticProgram, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectNoDraw, SuperMetroid.Core.Enemies.WreckedShip.WreckedShipAtticCue>;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship attic PLM's program (#1296), in place of its list.</summary>
internal static class WreckedShipAtticObjectProgramDefinitions
{
    /// <summary>The attic program.</summary>
    internal static AnimationProgramSet<WreckedShipAtticProgram, RoomObjectNoDraw, WreckedShipAtticCue> Programs { get; } = new(
        new Dictionary<WreckedShipAtticProgram, Step[]>
        {
            [WreckedShipAtticProgram.Attic] = [Step.Publish(WreckedShipAtticCue.InstallInertFrameBehavior), Step.Hold],
        });
}
