namespace SuperMetroid.Core.Saves;

/// <summary>The attract-demo sets that save RAM's completed-game marker unlocks.</summary>
public static class DemoSetUnlockRomData
{
    /// <summary>$80:8261 enables three sets before the completed-game SRAM marker.</summary>
    public const int DefaultSetCount = 3;

    /// <summary>Number of physical entries in each retail demo-set pointer table, all enabled after completion.</summary>
    public const int SetCount = 4;
}
