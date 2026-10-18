namespace SuperMetroid.Core.Enemies.SpacePirates.Artwork;

/// <summary>
/// The presentation boundary of the laser and claw programs (#641): the installed artwork key of each
/// <see cref="SpacePirateProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class SpacePirateProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 42;

    /// <summary>The installed artwork key of each pose.</summary>
    internal static IReadOnlyDictionary<SpacePirateProjectileFrame, ushort> PresentationKeys { get; } = new Dictionary<SpacePirateProjectileFrame, ushort>
    {
        [new(0)] = 0x9f43,
        [new(1)] = 0x9f47,
        [new(2)] = 0x9f4b,
        [new(3)] = 0x9f53,
        [new(4)] = 0x9f57,
        [new(5)] = 0x9f5b,
        [new(6)] = 0x9f5f,
        [new(7)] = 0x9f63,
        [new(8)] = 0x9f67,
        [new(9)] = 0x9f6b,
        [new(10)] = 0x9f6f,
        [new(11)] = 0x9f73,
        [new(12)] = 0x9f77,
        [new(13)] = 0x9f7f,
        [new(14)] = 0x9f83,
        [new(15)] = 0x9f87,
        [new(16)] = 0x9f8f,
        [new(17)] = 0x9f93,
        [new(18)] = 0x9f97,
        [new(19)] = 0x9f9b,
        [new(20)] = 0x9f9f,
        [new(21)] = 0x9fa3,
        [new(22)] = 0x9fa7,
        [new(23)] = 0x9fab,
        [new(24)] = 0x9faf,
        [new(25)] = 0x9fb3,
        [new(26)] = 0x9fbf,
        [new(27)] = 0x9fc3,
        [new(28)] = 0x9fc7,
        [new(29)] = 0x9fcb,
        [new(30)] = 0x9fcf,
        [new(31)] = 0x9fd3,
        [new(32)] = 0x9fd7,
        [new(33)] = 0x9fdb,
        [new(34)] = 0x9fe7,
        [new(35)] = 0x9feb,
        [new(36)] = 0x9fef,
        [new(37)] = 0x9ff3,
        [new(38)] = 0x9ff7,
        [new(39)] = 0x9ffb,
        [new(40)] = 0x9fff,
        [new(41)] = 0xa003,
    };
}
