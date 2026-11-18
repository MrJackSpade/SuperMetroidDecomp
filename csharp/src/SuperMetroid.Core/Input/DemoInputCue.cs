namespace SuperMetroid.Core.Input;

/// <summary>What a demo recording signals as it ends (#1300): the intro scene that plays it ends on its cue.</summary>
public enum DemoInputCue
{
    /// <summary>The Baby Metroid discovery stops Samus's input and movement and disables demo input.</summary>
    EndIntroBabyDiscovery,

    /// <summary>The Mother Brain flashback locks Samus standing facing left and disables demo input.</summary>
    EndIntroMotherBrainFlashback,
}
