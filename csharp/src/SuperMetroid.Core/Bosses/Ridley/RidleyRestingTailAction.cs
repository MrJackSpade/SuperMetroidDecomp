namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's tail with nothing steering it (<see cref="RidleyTailAction.None"/>): its segments stay where they are.</summary>
internal sealed partial class RidleyRestingTailAction() : RidleyTailAction(nameof(None));
