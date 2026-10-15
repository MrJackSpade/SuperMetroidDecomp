namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The four bank-$8B reward actor initialization routines.</summary>
internal enum EndingRewardInitialization : ushort
{
    /// <summary>$8B:F143 initializes suitless actors at (120,136), OBJ palette five.</summary>
    Suitless = 0xf143,
    /// <summary>$8B:F156 initializes suited body/arm at (120,152), OBJ palette six.</summary>
    SuitedBody = 0xf156,
    /// <summary>$8B:F169 initializes the helmeted head at (124,108), OBJ palette six.</summary>
    HelmetedHead = 0xf169,
    /// <summary>$8B:F17C initializes the helmetless head at (121,107), OBJ palette five.</summary>
    HelmetlessHead = 0xf17c,
}
