namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Cross-system gunship transitions produced during the most recent enemy frame.</summary>
public enum GunshipFrameEvent
{
    /// <summary>No gunship transition has been published for the current enemy pass.</summary>
    None,
    /// <summary>Post-Ceres landing has started the entrance-pad opening animation before Samus is ejected.</summary>
    LandingPadOpened,
    /// <summary>Post-Ceres ejection has reached its target and started the entrance-pad closing animation.</summary>
    LandingPadClosed,
    /// <summary>Post-Ceres landing and ejection have completed, the gunship is idle, and Samus input is unlocked.</summary>
    LandingCompleted,
    /// <summary>Idle-gunship entry was accepted, locking Samus input and starting the entrance-pad opening animation.</summary>
    EntryStarted,
    /// <summary>Samus has been lowered inside the ship and the entrance-pad closing animation has started.</summary>
    EntryPadClosing,
    /// <summary>Ordinary restoration has finished and the gunship requests the save-confirmation prompt.</summary>
    SavePromptRequested,
    /// <summary>Samus has risen to the exit target and the pad begins closing before input is restored.</summary>
    ExitPadClosing = 8,
    /// <summary>Ordinary gunship exit has completed, restoring idle gunship behavior and unlocking Samus input.</summary>
    ExitCompleted = 9,
    /// <summary>The active Zebes timebomb bypasses restoration and saving, starting escape takeoff with Samus attached inside.</summary>
    EscapeTakeoffStarted = 10,
    /// <summary>Gunship escape takeoff has reached its completion boundary, allowing the outer game to begin the ending sequence.</summary>
    EscapeTakeoffCompleted = 11,
}
