namespace SuperMetroid.Core.Input;

/// <summary>The private bank-$91 routines a demo-input program calls (#1300); each object's owner runs its own.</summary>
public enum DemoInputCue
{
    /// <summary><c>$91:8682</c>: the Baby Metroid discovery ends Samus's handlers and disables demo input.</summary>
    EndIntroBabyDiscovery,

    /// <summary><c>$91:8739</c>: the Mother Brain flashback locks Samus in pose two and disables demo input.</summary>
    EndIntroMotherBrainFlashback,
}
