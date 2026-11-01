namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Function pointers used by Mother Brain's separate brain record.</summary>
public enum MotherBrainBrainFunction
{
    /// <summary>Advances neck geometry unless time is frozen, attaches the head to joint 4, and installs the combined draw hook.</summary>
    SetupBrainAndNeckToBeDrawn = 1,
    /// <summary>Installs the head-only custom draw hook without articulated neck drawing.</summary>
    SetupBrainToBeDrawn,
}
