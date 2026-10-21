namespace SuperMetroid.RenderVerification.Hud;

/// <summary>Reserve capacity and mode used to exercise the native automatic recovery path.</summary>
internal static class ReserveCaptureFixtureDefinitions
{
    /// <summary>One acquired reserve tank supplies 100 energy units for the focused recovery fixture.</summary>
    internal const ushort Capacity = 100;
    /// <summary>WRAM reserve mode one selects the native automatic-recovery path.</summary>
    internal const SuperMetroid.Core.Progression.ReserveTankMode AutomaticMode = SuperMetroid.Core.Progression.ReserveTankMode.Auto;
}
