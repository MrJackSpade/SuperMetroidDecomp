namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The falling-tube main functions stored in a tube's VariableA, order.</summary>
internal enum MotherBrainTubeFunction
{
    /// <summary><c>Function_MotherBrainTubes_NonMainTube</c>.</summary>
    NonMainTube = 1,

    /// <summary><c>Function_MotherBrainTubes_MainTube_WaitingToFall</c>.</summary>
    MainTube_WaitingToFall,

    /// <summary><c>Function_MotherBrainTubes_MainTube_Falling</c>.</summary>
    MainTube_Falling,
}
