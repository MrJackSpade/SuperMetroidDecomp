namespace SuperMetroid.Core.Assets;

/// <summary>
/// Phantoon's bank-$A7 extended frames selected by the compiled $CC41-$CCFB
/// instruction programs. These identities select BG2 tilemap writes, not OAM.
/// Component hitbox pointers and collision rectangles are engine definitions.
/// </summary>
internal static class PhantoonBg2FrameDefinitions
{
    internal const int Version = 1;
    internal const string FileName = "phantoon-bg2-frames.json";
    /// <summary>Phantoon's tentacle frames have at most two BG2 components.</summary>
    internal const int MaximumComponents = 2;
    /// <summary>Phantoon's extended frames and BG2 streams reside in bank $A7.</summary>
    internal const byte Bank = 0xa7;
    /// <summary>$A0:96CA identifies an extended BG2 command stream by this first word.</summary>
    internal const ushort StreamMarker = EnemyBg2FrameLayout.StreamMarker;
    internal const ushort VramBase = EnemyBg2FrameLayout.VramBase;
    internal const ushort WorkingRamBase = EnemyBg2FrameLayout.WorkingRamBase;
    internal const int TilemapWidth = EnemyBg2FrameLayout.TilemapWidth;
    internal const int TilemapHeight = EnemyBg2FrameLayout.TilemapHeight;
    /// <summary>Body with its complete five-rectangle hitbox, $A7:DEE7.</summary>
    internal const ushort BodyFullHitbox = 0xdee7;
    /// <summary>Body with only the vulnerable eye hitbox, $A7:DEF1.</summary>
    internal const ushort BodyEyeHitboxOnly = 0xdef1;
    /// <summary>First two-component tentacle frame, $A7:DFB3.</summary>
    internal const ushort Tentacles0 = 0xdfb3;
    /// <summary>Second two-component tentacle frame, $A7:DFC5.</summary>
    internal const ushort Tentacles1 = 0xdfc5;
    /// <summary>Third two-component tentacle frame, $A7:DFD7.</summary>
    internal const ushort Tentacles2 = 0xdfd7;

    private static readonly EnemyBg2FrameDefinition[] FrameDefinitions =
    [
        new(0xdedd, "body_invulnerable"),
        new(BodyFullHitbox, "body_full_hitbox"),
        new(BodyEyeHitboxOnly, "body_eye_hitbox_only"),
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
        new(Tentacles0, "tentacles_0"),
        new(Tentacles1, "tentacles_1"),
        new(Tentacles2, "tentacles_2"),
        new(0xdfe9, "mouth_0"),
        new(0xdff3, "mouth_1"),
        new(0xdffd, "mouth_2"),
    ];

    internal static ReadOnlySpan<EnemyBg2FrameDefinition> Frames => FrameDefinitions;

    internal static bool IsFrame(ushort pointer)
    {
        foreach (EnemyBg2FrameDefinition frame in FrameDefinitions)
            if (frame.Pointer == pointer)
                return true;
        return false;
    }
}
