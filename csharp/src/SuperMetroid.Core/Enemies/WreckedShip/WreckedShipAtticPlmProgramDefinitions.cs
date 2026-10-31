using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Enemies.WreckedShip.WreckedShipAtticProgram, SuperMetroid.Core.Rooms.Plms.PlmNoDraw, SuperMetroid.Core.Enemies.WreckedShip.WreckedShipAtticCue>;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship attic PLM's program (#1296), in place of its list.</summary>
internal static class WreckedShipAtticPlmProgramDefinitions
{
    /// <summary>The attic program.</summary>
    internal static AnimationProgramSet<WreckedShipAtticProgram, PlmNoDraw, WreckedShipAtticCue> Programs { get; } = new(
        new Dictionary<WreckedShipAtticProgram, Step[]>
        {
            [WreckedShipAtticProgram.Attic] = [Step.Publish(WreckedShipAtticCue.InstallInertFrameBehavior), Step.Hold],
        });
}
