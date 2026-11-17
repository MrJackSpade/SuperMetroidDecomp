namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The actions Mother Brain's separately listed brain runs.</summary>
public enum MotherBrainBrainAction
{
    /// <summary>Advances neck geometry unless time is frozen, sets her head on the neck's last joint, and draws her brain and neck.</summary>
    SetupBrainAndNeckToBeDrawn,
    /// <summary>Draws her brain alone, without her articulated neck.</summary>
    SetupBrainToBeDrawn,
}
