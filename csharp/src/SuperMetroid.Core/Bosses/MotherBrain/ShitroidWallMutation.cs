namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What the Shitroid's invisible-wall PLM does to its wall column: the only spawns the Shitroid's wall spawner performs.</summary>
public enum ShitroidWallMutation
{
    /// <summary>Makes every block of the wall column air, letting Samus through.</summary>
    Clear,
    /// <summary>Makes every block of the wall column solid.</summary>
    Create,
}
