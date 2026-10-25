namespace SuperMetroid.Core.Saves;

/// <summary>How many attract-demo sets the title plays, and how completing the game unlocks the last one.</summary>
public static class DemoSetUnlockDefinitions
{
    /// <summary>The demo sets the title cycles through until a save slot records that the game was completed.</summary>
    public const int SetsBeforeCompletion = 3;

    /// <summary>The number of demo sets; every one plays once a save slot records that the game was completed.</summary>
    public const int SetCount = 4;
}
