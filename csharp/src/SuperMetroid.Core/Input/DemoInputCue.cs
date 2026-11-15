namespace SuperMetroid.Core.Input;

/// <summary>The private steps a demo input recording calls (#1300); each object's owner runs its own.</summary>
public enum DemoInputCue
{
    /// <summary>The Baby Metroid discovery stops Samus's input and movement and disables demo input.</summary>
    EndIntroBabyDiscovery,

    /// <summary>The Mother Brain flashback locks Samus standing facing left and disables demo input.</summary>
    EndIntroMotherBrainFlashback,
}
