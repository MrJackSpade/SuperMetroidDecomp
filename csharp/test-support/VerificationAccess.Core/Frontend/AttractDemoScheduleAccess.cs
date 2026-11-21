namespace SuperMetroid.Core.Frontend;

/// <summary>Reads the still-frame hold of the attract demo's holding scene.</summary>
internal static class AttractDemoScheduleAccess
{
    /// <summary>Reads the schedule's scene.</summary>
    extension(AttractDemoSchedule self)
    {
        /// <summary>The holding scene's still-frame hold; over while no scene holds.</summary>
        internal UpdateCountdown Hold => self.Scene is AttractDemoSchedule.SceneState.Holding holding ? holding.Hold : UpdateCountdown.Ended();
    }
}
