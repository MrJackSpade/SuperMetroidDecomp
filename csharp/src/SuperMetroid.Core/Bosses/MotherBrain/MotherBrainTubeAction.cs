namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The falling-tube main actions, in order.</summary>
internal enum MotherBrainTubeAction
{
    /// <summary>Mother Brain tubes behaviour: non main tube.</summary>
    NonMainTube = 1,

    /// <summary>Mother Brain tubes behaviour: main tube, waiting to fall.</summary>
    MainTube_WaitingToFall,

    /// <summary>Mother Brain tubes behaviour: main tube, falling.</summary>
    MainTube_Falling,
}
