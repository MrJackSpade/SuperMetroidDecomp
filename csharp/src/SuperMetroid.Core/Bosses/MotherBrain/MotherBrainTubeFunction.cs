namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The falling-tube main functions stored in a tube's VariableA, in bank $A9 order.</summary>
internal enum MotherBrainTubeFunction : ushort
{
    /// <summary><c>Function_MotherBrainTubes_NonMainTube</c> at $A9:8B88.</summary>
    NonMainTube = 1,

    /// <summary><c>Function_MotherBrainTubes_MainTube_WaitingToFall</c> at $A9:8BCB.</summary>
    MainTube_WaitingToFall,

    /// <summary><c>Function_MotherBrainTubes_MainTube_Falling</c> at $A9:8BD6.</summary>
    MainTube_Falling,
}
