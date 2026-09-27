namespace SuperMetroid.Core.Assets;

/// <summary>One physical Phantoon extended-frame identity and its editable visual name.</summary>
internal readonly record struct PhantoonBg2FrameDefinition(ushort Pointer, string Name);

/// <summary>
/// Phantoon's bank-$A7 extended frames selected by the compiled $CC41-$CCFB
/// instruction programs. These identities select BG2 tilemap writes, not OAM.
/// Component hitbox pointers and collision rectangles are engine definitions.
/// </summary>
internal static class PhantoonBg2FrameDefinitions
{
    internal const int Version = 1;
    internal const string FileName = "phantoon-bg2-frames.json";
    /// <summary>Phantoon's extended frames and BG2 streams reside in bank $A7.</summary>
    internal const byte Bank = 0xa7;
    /// <summary>$A0:96CA identifies an extended BG2 command stream by this first word.</summary>
    internal const ushort StreamMarker = 0xfffe;
    internal const ushort VramBase = 0x4800;
    internal const ushort WorkingRamBase = 0x2000;
    internal const int TilemapWidth = 32;
    internal const int TilemapHeight = 64;

    private static readonly PhantoonBg2FrameDefinition[] FrameDefinitions =
    [
        new(0xdedd, "body_invulnerable"),
        new(0xdee7, "body_full_hitbox"),
        new(0xdef1, "body_eye_hitbox_only"),
        new(0xdefb, "eye_closed"),
        new(0xdf05, "eye_open_0"),
        new(0xdf0f, "eye_open_1"),
        new(0xdf19, "eye_open_2"),
        new(0xdf23, "eyeball_center"),
        new(0xdf2d, "eyeball_up"),
        new(0xdf37, "eyeball_down"),
        new(0xdf41, "eyeball_left"),
        new(0xdf4b, "eyeball_right"),
        new(0xdf55, "eyeball_down_left"),
        new(0xdf5f, "eyeball_down_right"),
        new(0xdf69, "eyeball_up_left"),
        new(0xdf73, "eyeball_up_right"),
        new(0xdfb3, "tentacles_0"),
        new(0xdfc5, "tentacles_1"),
        new(0xdfd7, "tentacles_2"),
        new(0xdfe9, "mouth_0"),
        new(0xdff3, "mouth_1"),
        new(0xdffd, "mouth_2"),
    ];

    internal static ReadOnlySpan<PhantoonBg2FrameDefinition> Frames => FrameDefinitions;
}
