namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Native function pointers used by Mother Brain's separate brain record.</summary>
public enum MotherBrainBrainFunction
{
    /// <summary><c>$A9:87A2 Function_MotherBrain_SetupBrainAndNeckToBeDrawn</c>: advances neck geometry unless time is frozen, attaches the head to joint 4, and installs the combined draw hook.</summary>
    SetupBrainAndNeckToBeDrawn = 1,
    /// <summary><c>$A9:87D0 Function_MotherBrain_SetupBrainToBeDrawn</c>: installs the head-only custom draw hook without articulated neck drawing.</summary>
    SetupBrainToBeDrawn,
}
