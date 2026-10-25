namespace SuperMetroid.Core.Input;

/// <summary>The private routines a demo-input program calls (#1300); each object's owner runs its own.</summary>
public enum DemoInputCue
{
    /// <summary>The Baby Metroid discovery ends Samus's handlers and disables demo input.</summary>
    EndIntroBabyDiscovery,

    /// <summary>The Mother Brain flashback locks Samus in pose two and disables demo input.</summary>
    EndIntroMotherBrainFlashback,
}
