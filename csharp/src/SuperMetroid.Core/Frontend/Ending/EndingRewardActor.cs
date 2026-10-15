namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ten bank-$8B reward gesture and jump cinematic-object definitions.</summary>
internal enum EndingRewardActor : ushort
{
    /// <summary>$8B:EF33, suitless upper hair-release actor spawned by E342.</summary>
    SuitlessUpper = 0xef33,
    /// <summary>$8B:EF39, suitless lower hair-release actor spawned by E342.</summary>
    SuitlessLower = 0xef39,
    /// <summary>$8B:EF3F, suitless jumping actor spawned by F51D.</summary>
    JumpSuitlessBody = 0xef3f,
    /// <summary>$8B:EF57, suited gesture body.</summary>
    SuitedBody = 0xef57,
    /// <summary>$8B:EF5D, suited moving thumbs-up arm.</summary>
    SuitedArm = 0xef5d,
    /// <summary>$8B:EF63, helmeted gesture head.</summary>
    HelmetedHead = 0xef63,
    /// <summary>$8B:EF69, helmetless gesture head.</summary>
    HelmetlessHead = 0xef69,
    /// <summary>$8B:EF6F, suited jump body spawned by F554.</summary>
    JumpSuitedBody = 0xef6f,
    /// <summary>$8B:EF75, helmeted jump head spawned by F554.</summary>
    JumpHelmetedHead = 0xef75,
    /// <summary>$8B:EF7B, helmetless jump head spawned by F554.</summary>
    JumpHelmetlessHead = 0xef7b,
}
