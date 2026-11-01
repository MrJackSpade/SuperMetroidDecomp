namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The falling-tube main functions stored in a tube's VariableA, order.</summary>
internal enum MotherBrainTubeFunction
{
    /// <summary>Mother Brain tubes behaviour: non main tube.</summary>
    NonMainTube = 1,

    /// <summary>Mother Brain tubes behaviour: main tube, waiting to fall.</summary>
    MainTube_WaitingToFall,

    /// <summary>Mother Brain tubes behaviour: main tube, falling.</summary>
    MainTube_Falling,
}
