namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Cartridge values and helpers of <see cref="CrocomireBodyVisualDefinitions"/> that only verification reads.</summary>
internal static class CrocomireBodyVisualDefinitionsConstants
{
    /// <summary>Enumerates selected body roots without caching a duplicate pointer table.</summary>
    internal static IReadOnlyList<ushort> Frames => [.. Enumerable.Range(0, CrocomireBodyVisualDefinitions.BodyFrameCount).Select(CrocomireBodyVisualToolingDefinitions.FramePointer)];
    /// <summary>Accepts only the forty-two selected mixed OAM/BG2 roots; gaps,
    /// component interiors, pure-OAM transition frames and skeletons are excluded.</summary>
    internal static bool HasBg2(ushort pointer) =>
        InRun(pointer, CrocomireBodyVisualToolingDefinitions.ChargeStepStart, 12, 0x32) ||
        InRun(pointer, CrocomireBodyVisualToolingDefinitions.AttackStart, 6, 0x3a) ||
        InRun(pointer, CrocomireBodyVisualToolingDefinitions.AttackClawStart, 6, 0x32) ||
        InRun(pointer, CrocomireBodyVisualToolingDefinitions.BodyStart, 3, 0x3a) || pointer == CrocomireBodyVisualToolingDefinitions.IsolatedBodyFrame ||
        InRun(pointer, CrocomireBodyVisualToolingDefinitions.LaterBodyStart, 14, 0x3a);
    /// <summary>Checks exact record alignment inside one contiguous native frame run.</summary>
    internal static bool InRun(ushort pointer, ushort start, int count, int stride)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < count * stride && offset % stride == 0;
    }
}
