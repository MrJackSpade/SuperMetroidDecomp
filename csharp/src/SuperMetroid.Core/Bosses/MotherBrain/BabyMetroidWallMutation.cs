namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What the Baby Metroid's invisible-wall room object does to its wall column: the only spawns the Baby Metroid's wall spawner performs.</summary>
public enum BabyMetroidWallMutation
{
    /// <summary>Makes every block of the wall column air, letting Samus through.</summary>
    Clear,
    /// <summary>Makes every block of the wall column solid.</summary>
    Create,
}
