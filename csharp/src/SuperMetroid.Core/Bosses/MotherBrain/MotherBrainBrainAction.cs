namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The actions Mother Brain's separately listed brain runs.</summary>
public enum MotherBrainBrainAction
{
    /// <summary>Advances neck geometry unless time is frozen, attaches the head to joint 4, and draws her brain and neck.</summary>
    SetupBrainAndNeckToBeDrawn = 1,
    /// <summary>Draws her brain alone, without her articulated neck.</summary>
    SetupBrainToBeDrawn,
}
