namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>What the gunship's phases do beside running (<see cref="GunshipBehavior"/>).</summary>
internal static class GunshipPhaseDefinitions
{
    /// <summary>
    /// The phases the landed ship bobs before running: every phase from waiting for the landing entrance through waiting
    /// for the engines, Idle among them; firing up the engines is the first that does not.
    /// </summary>
    internal static readonly GunshipPhase[] BobbingWhileLanded =
    [
        GunshipPhase.WaitForLandingEntranceToOpen, GunshipPhase.EjectSamus, GunshipPhase.FinishLanding,
        GunshipPhase.Idle, GunshipPhase.WaitForEntranceToOpen, GunshipPhase.LowerSamus,
        GunshipPhase.WaitForEntranceToClose, GunshipPhase.BeginLiftoffOrRestoreSamus,
        GunshipPhase.HandleSaveConfirmation, GunshipPhase.WaitForExitPadToOpen, GunshipPhase.RaiseSamus,
        GunshipPhase.FinishSamusExit, GunshipPhase.WaitForEngines,
    ];
}
