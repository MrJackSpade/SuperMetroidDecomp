namespace SuperMetroid.Core.Assets;

/// <summary>
/// Fixed wing and articulated-tail visual identities selected by Ridley's
/// $A6:DADE/$A6:DB2A hooks. Pose cadence and segment motion remain gameplay state.
/// </summary>
internal static class RidleySupplementalVisualDefinitions
{
    /// <summary>$A6, the native bank containing Ridley's supplemental OAM maps.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>$A6:DD4A..DDE1, six distinct left-facing wing elevations selected by DrawRidleyWings.</summary>
    private enum WingPose : ushort
    {
        /// <summary>$A6:DD4A, Spritemap_RidleyWings_FacingLeft_FullyRaised.</summary>
        FullyRaised = 0xdd4a,
        /// <summary>$A6:DD6A, Spritemap_RidleyWings_FacingLeft_MostlyRaised.</summary>
        MostlyRaised = 0xdd6a,
        /// <summary>$A6:DD85, Spritemap_RidleyWings_FacingLeft_SlightlyRaised.</summary>
        SlightlyRaised = 0xdd85,
        /// <summary>$A6:DD96, Spritemap_RidleyWings_FacingLeft_SlightlyLowered.</summary>
        SlightlyLowered = 0xdd96,
        /// <summary>$A6:DDA7, Spritemap_RidleyWings_FacingLeft_MostlyLowered.</summary>
        MostlyLowered = 0xdda7,
        /// <summary>$A6:DDC2, Spritemap_RidleyWings_FacingLeft_FullyLowered.</summary>
        FullyLowered = 0xddc2,
    }
    /// <summary>$A6:DDE2..DE79, right-facing records have the same lengths/order and begin $98 bytes after their left-facing counterparts.</summary>
    private const int RightWingFrameOffset = 0xdde2 - (int)WingPose.FullyRaised;
    /// <summary>$A6:DB02..DB14: six downstroke poses followed by the four interior poses in reverse order.</summary>
    private const int WingCycleLength = 10;
    /// <summary>$A6:DCDA, Spritemap_RidleyTailTip_PointingLeft, first of sixteen consecutive tail-tip records.</summary>
    private const ushort FirstTailTipFrame = 0xdcda;
    /// <summary>$A6:DCDA..DD49: each one-object spritemap has a two-byte count and five-byte OAM entry.</summary>
    private const int TailTipFrameBytes = 2 + 5;
    /// <summary>$A6:DCD2 selects left in slot12 of the sixteen direction sectors (down=0).</summary>
    private const int LeftTailTipDirection = 12;
    /// <summary>$A6:DC90, the large base-segment OAM map.</summary>
    internal const ushort LargeSegment = 0xdc90;

    /// <summary>$A6:DC97, the medium middle-segment OAM map.</summary>
    internal const ushort MediumSegment = 0xdc97;

    /// <summary>$A6:DC9E, the small tip-side segment OAM map.</summary>
    internal const ushort SmallSegment = 0xdc9e;

    /// <summary>$A6:DB02..DB28 contains one ten-phase cycle for each of the two facings.</summary>
    internal const int WingPointerCount = WingCycleLength * 2;
    /// <summary>$A6:DCBA..DCD8 selects one frame for each sixteenth-turn sector.</summary>
    internal const int TailTipPointerCount = 16;

    internal static ushort WingFrameAt(int index)
    {
        if ((uint)index >= WingPointerCount)
            throw new InvalidDataException($"Ridley wing frame {index} is outside the native table.");
        int phase = index % WingCycleLength;
        WingPose pose = Math.Min(phase, WingCycleLength - phase) switch
        {
            0 => WingPose.FullyRaised,
            1 => WingPose.MostlyRaised,
            2 => WingPose.SlightlyRaised,
            3 => WingPose.SlightlyLowered,
            4 => WingPose.MostlyLowered,
            _ => WingPose.FullyLowered,
        };
        return (ushort)((int)pose + index / WingCycleLength * RightWingFrameOffset);
    }
    internal static ushort TailTipFrameAt(int index) =>
        (uint)index < TailTipPointerCount
            ? (ushort)(FirstTailTipFrame + ((LeftTailTipDirection - index) & (TailTipPointerCount - 1)) * TailTipFrameBytes)
            : throw new InvalidDataException($"Ridley tail-tip direction {index} is outside the native table.");

    internal static ushort SegmentFrameAt(int index) => index switch
    {
        0 or 1 => LargeSegment,
        2 or 3 => MediumSegment,
        4 or 5 => SmallSegment,
        _ => throw new InvalidDataException($"Ridley tail segment {index} is outside six drawn links."),
    };

    internal static EnemySpritemapDefinition[] Frames()
    {
        var pointers = new SortedSet<ushort>
        {
            LargeSegment, MediumSegment, SmallSegment,
        };
        for (int direction = 0; direction < TailTipPointerCount; direction++)
            pointers.Add(TailTipFrameAt(direction));
        for (int phase = 0; phase < WingPointerCount; phase++)
            pointers.Add(WingFrameAt(phase));
        return [.. pointers.Select(pointer => new EnemySpritemapDefinition(
            Bank, pointer, $"ridley_supplement_a6_{pointer:x4}"))];
    }
}
