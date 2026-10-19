namespace SuperMetroid.RenderVerification.Areas;

/// <summary>Holds the Ridley departure state that activates Ceres door earthquake producers.</summary>
internal static class CeresQuakeFixture
{
    /// <summary>The Ceres escape stage after Ridley's departure that enables door earthquake producers.</summary>
    internal const SuperMetroid.Core.Enemies.CeresEscapeStatus EscapeStatus = SuperMetroid.Core.Enemies.CeresEscapeStatus.SelfDestructStarted;
}
