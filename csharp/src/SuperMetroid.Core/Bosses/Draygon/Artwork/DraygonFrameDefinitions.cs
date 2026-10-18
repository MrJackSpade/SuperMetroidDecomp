using SuperMetroid.Core.Enemies.Artwork;


namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Draygon's presentation boundary (#641): the bank-$A5 extended spritemaps its typed programs show, in ascending
/// address order. Gameplay animates <see cref="DraygonFrame"/> indexes; only this catalog knows the spritemaps.
/// Each facing authors 27 one-component roots (arms, body, eye), eight two-component tail roots, then the tail
/// whip's roots in pairs of four, six and eight components.
/// </summary>
internal static class DraygonFrameDefinitions
{

    /// <summary>The roots of one facing.</summary>
    /// <param name="OneComponent">The first of the 27 ten-byte one-component roots.</param>
    /// <param name="Tail">The first of the 8 eighteen-byte two-component tail roots.</param>
    /// <param name="WhipFour">The first of the 2 four-component whip roots.</param>
    /// <param name="WhipSix">The first of the 2 six-component whip roots.</param>
    /// <param name="WhipEight">The first of the 2 eight-component whip roots.</param>
    internal readonly record struct FacingRoots(ushort OneComponent, ushort Tail, ushort WhipFour, ushort WhipSix, ushort WhipEight);

    /// <summary>The installed frame each <see cref="DraygonFrame"/> shows.</summary>
    internal static EnemyFrameTable<DraygonFrame> ShownFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.DraygonOamA2DF },
        { new(Index: 1), EnemyFrameId.DraygonOamA2E9 },
        { new(Index: 10), EnemyFrameId.DraygonBg2A343 },
        { new(Index: 11), EnemyFrameId.DraygonBg2A34D },
        { new(Index: 12), EnemyFrameId.DraygonBg2A357 },
        { new(Index: 13), EnemyFrameId.DraygonBg2A361 },
        { new(Index: 14), EnemyFrameId.DraygonBg2A36B },
        { new(Index: 15), EnemyFrameId.DraygonBg2A375 },
        { new(Index: 16), EnemyFrameId.DraygonBg2A37F },
        { new(Index: 17), EnemyFrameId.DraygonBg2A389 },
        { new(Index: 18), EnemyFrameId.DraygonBg2A393 },
        { new(Index: 19), EnemyFrameId.DraygonBg2A39D },
        { new(Index: 2), EnemyFrameId.DraygonOamA2F3 },
        { new(Index: 20), EnemyFrameId.DraygonBg2A3A7 },
        { new(Index: 21), EnemyFrameId.DraygonBg2A3B1 },
        { new(Index: 22), EnemyFrameId.DraygonBg2A3BB },
        { new(Index: 23), EnemyFrameId.DraygonOamA3C5 },
        { new(Index: 24), EnemyFrameId.DraygonOamA3CF },
        { new(Index: 25), EnemyFrameId.DraygonOamA3D9 },
        { new(Index: 26), EnemyFrameId.DraygonOamA3E3 },
        { new(Index: 27), EnemyFrameId.DraygonOamA40B },
        { new(Index: 28), EnemyFrameId.DraygonOamA41D },
        { new(Index: 29), EnemyFrameId.DraygonOamA42F },
        { new(Index: 3), EnemyFrameId.DraygonOamA2FD },
        { new(Index: 30), EnemyFrameId.DraygonOamA441 },
        { new(Index: 31), EnemyFrameId.DraygonOamA453 },
        { new(Index: 32), EnemyFrameId.DraygonOamA465 },
        { new(Index: 33), EnemyFrameId.DraygonOamA477 },
        { new(Index: 34), EnemyFrameId.DraygonOamA489 },
        { new(Index: 35), EnemyFrameId.DraygonOamA4A3 },
        { new(Index: 36), EnemyFrameId.DraygonOamA4C5 },
        { new(Index: 37), EnemyFrameId.DraygonOamA4EF },
        { new(Index: 38), EnemyFrameId.DraygonOamA521 },
        { new(Index: 39), EnemyFrameId.DraygonOamA55B },
        { new(Index: 4), EnemyFrameId.DraygonOamA307 },
        { new(Index: 40), EnemyFrameId.DraygonOamA59D },
        { new(Index: 41), EnemyFrameId.DraygonOamA607 },
        { new(Index: 42), EnemyFrameId.DraygonOamA611 },
        { new(Index: 43), EnemyFrameId.DraygonOamA61B },
        { new(Index: 44), EnemyFrameId.DraygonOamA625 },
        { new(Index: 45), EnemyFrameId.DraygonOamA62F },
        { new(Index: 46), EnemyFrameId.DraygonOamA639 },
        { new(Index: 47), EnemyFrameId.DraygonBg2A643 },
        { new(Index: 48), EnemyFrameId.DraygonBg2A64D },
        { new(Index: 49), EnemyFrameId.DraygonBg2A657 },
        { new(Index: 5), EnemyFrameId.DraygonOamA311 },
        { new(Index: 50), EnemyFrameId.DraygonBg2A661 },
        { new(Index: 51), EnemyFrameId.DraygonBg2A66B },
        { new(Index: 52), EnemyFrameId.DraygonBg2A675 },
        { new(Index: 53), EnemyFrameId.DraygonBg2A67F },
        { new(Index: 54), EnemyFrameId.DraygonBg2A689 },
        { new(Index: 55), EnemyFrameId.DraygonBg2A693 },
        { new(Index: 56), EnemyFrameId.DraygonBg2A69D },
        { new(Index: 57), EnemyFrameId.DraygonBg2A6A7 },
        { new(Index: 58), EnemyFrameId.DraygonBg2A6B1 },
        { new(Index: 59), EnemyFrameId.DraygonBg2A6BB },
        { new(Index: 6), EnemyFrameId.DraygonBg2A31B },
        { new(Index: 60), EnemyFrameId.DraygonBg2A6C5 },
        { new(Index: 61), EnemyFrameId.DraygonBg2A6CF },
        { new(Index: 62), EnemyFrameId.DraygonBg2A6D9 },
        { new(Index: 63), EnemyFrameId.DraygonBg2A6E3 },
        { new(Index: 64), EnemyFrameId.DraygonOamA6ED },
        { new(Index: 65), EnemyFrameId.DraygonOamA6F7 },
        { new(Index: 66), EnemyFrameId.DraygonOamA701 },
        { new(Index: 67), EnemyFrameId.DraygonOamA70B },
        { new(Index: 68), EnemyFrameId.DraygonOamA779 },
        { new(Index: 69), EnemyFrameId.DraygonOamA78B },
        { new(Index: 7), EnemyFrameId.DraygonBg2A325 },
        { new(Index: 70), EnemyFrameId.DraygonOamA79D },
        { new(Index: 71), EnemyFrameId.DraygonOamA7AF },
        { new(Index: 72), EnemyFrameId.DraygonOamA7C1 },
        { new(Index: 73), EnemyFrameId.DraygonOamA7D3 },
        { new(Index: 74), EnemyFrameId.DraygonOamA7E5 },
        { new(Index: 75), EnemyFrameId.DraygonOamA7F7 },
        { new(Index: 76), EnemyFrameId.DraygonOamA811 },
        { new(Index: 77), EnemyFrameId.DraygonOamA833 },
        { new(Index: 78), EnemyFrameId.DraygonOamA85D },
        { new(Index: 79), EnemyFrameId.DraygonOamA88F },
        { new(Index: 8), EnemyFrameId.DraygonBg2A32F },
        { new(Index: 80), EnemyFrameId.DraygonOamA8C9 },
        { new(Index: 81), EnemyFrameId.DraygonOamA90B },
        { new(Index: 9), EnemyFrameId.DraygonBg2A339 },
    };
}
