namespace SuperMetroid.Core.Game;

/// <summary>Ordered native transition lists $91:A90C..B00F; first matching condition wins.</summary>
internal static class SamusPoseInputRulesLate
{
    /// <summary>$91:A90C: 4 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA90C =
    [
        new(0x0800, 0x0000, 0x003d),
        new(0x0080, 0x0000, 0x007f),
        new(0x0000, 0x0100, 0x007b),
        new(0x0000, 0x0200, 0x007c),
    ];

    /// <summary>$91:A926: 4 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA926 =
    [
        new(0x0800, 0x0000, 0x003e),
        new(0x0080, 0x0000, 0x0080),
        new(0x0000, 0x0100, 0x007b),
        new(0x0000, 0x0200, 0x007c),
    ];

    /// <summary>$91:A940: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA940 =
    [
        new(0x0800, 0x0000, 0x003d),
        new(0x0000, 0x0200, 0x007e),
        new(0x0000, 0x0100, 0x007d),
    ];

    /// <summary>$91:A954: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA954 =
    [
        new(0x0800, 0x0000, 0x003e),
        new(0x0000, 0x0100, 0x007d),
        new(0x0000, 0x0200, 0x007e),
    ];

    /// <summary>$91:A968: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA968 =
    [
        new(0x0800, 0x0000, 0x003d),
        new(0x0000, 0x0100, 0x007f),
        new(0x0000, 0x0200, 0x0080),
    ];

    /// <summary>$91:A97C: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA97C =
    [
        new(0x0800, 0x0000, 0x003e),
        new(0x0000, 0x0100, 0x007f),
        new(0x0000, 0x0200, 0x0080),
    ];

    /// <summary>$91:A990: 1 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA990 =
    [
        new(0x0000, 0x0280, 0x0066),
    ];

    /// <summary>$91:A998: 1 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA998 =
    [
        new(0x0000, 0x0180, 0x0065),
    ];

    /// <summary>$91:A9A0: 6 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA9A0 =
    [
        new(0x0000, 0x0180, 0x0065),
        new(0x0000, 0x0010, 0x0069),
        new(0x0000, 0x0020, 0x006b),
        new(0x0000, 0x0040, 0x0013),
        new(0x0000, 0x0080, 0x0065),
        new(0x0000, 0x0100, 0x0065),
    ];

    /// <summary>$91:A9C6: 6 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA9C6 =
    [
        new(0x0000, 0x0280, 0x0066),
        new(0x0000, 0x0010, 0x006a),
        new(0x0000, 0x0020, 0x006c),
        new(0x0000, 0x0040, 0x0014),
        new(0x0000, 0x0080, 0x0066),
        new(0x0000, 0x0200, 0x0066),
    ];

    /// <summary>$91:A9EC: 6 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA9EC =
    [
        new(0x0400, 0x0000, 0x0037),
        new(0x0000, 0x0200, 0x001a),
        new(0x0000, 0x0010, 0x0069),
        new(0x0000, 0x0020, 0x006b),
        new(0x0000, 0x0040, 0x0013),
        new(0x0000, 0x0080, 0x0083),
    ];

    /// <summary>$91:AA12: 6 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAA12 =
    [
        new(0x0400, 0x0000, 0x0038),
        new(0x0000, 0x0100, 0x0019),
        new(0x0000, 0x0010, 0x006a),
        new(0x0000, 0x0020, 0x006c),
        new(0x0000, 0x0040, 0x0014),
        new(0x0000, 0x0080, 0x0084),
    ];

    /// <summary>$91:AA38: 11 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAA38 =
    [
        new(0x0080, 0x0000, 0x004b),
        new(0x0000, 0x0900, 0x000f),
        new(0x0000, 0x0500, 0x0011),
        new(0x0400, 0x0000, 0x0035),
        new(0x0000, 0x0220, 0x0078),
        new(0x0000, 0x0210, 0x0076),
        new(0x0000, 0x0800, 0x0003),
        new(0x0000, 0x0010, 0x0005),
        new(0x0000, 0x0020, 0x0007),
        new(0x0000, 0x0200, 0x0025),
        new(0x0000, 0x0100, 0x0009),
    ];

    /// <summary>$91:AA7C: 11 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAA7C =
    [
        new(0x0080, 0x0000, 0x004c),
        new(0x0000, 0x0a00, 0x0010),
        new(0x0000, 0x0600, 0x0012),
        new(0x0400, 0x0000, 0x0036),
        new(0x0000, 0x0120, 0x0077),
        new(0x0000, 0x0110, 0x0075),
        new(0x0000, 0x0800, 0x0004),
        new(0x0000, 0x0010, 0x0006),
        new(0x0000, 0x0020, 0x0008),
        new(0x0000, 0x0100, 0x0026),
        new(0x0000, 0x0200, 0x000a),
    ];

    /// <summary>$91:AAC0: 20 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAAC0 =
    [
        new(0x0000, 0x0980, 0x0069),
        new(0x0000, 0x0580, 0x006b),
        new(0x0000, 0x0190, 0x0069),
        new(0x0000, 0x01a0, 0x006b),
        new(0x0000, 0x0900, 0x0069),
        new(0x0000, 0x0500, 0x006b),
        new(0x0000, 0x0280, 0x002f),
        new(0x0000, 0x0880, 0x0015),
        new(0x0000, 0x0480, 0x0017),
        new(0x0000, 0x0090, 0x0069),
        new(0x0000, 0x00a0, 0x006b),
        new(0x0000, 0x0180, 0x0051),
        new(0x0000, 0x00c0, 0x0013),
        new(0x0000, 0x0200, 0x002f),
        new(0x0000, 0x0800, 0x0015),
        new(0x0000, 0x0400, 0x0017),
        new(0x0000, 0x0010, 0x0069),
        new(0x0000, 0x0020, 0x006b),
        new(0x0000, 0x0100, 0x0051),
        new(0x0000, 0x0040, 0x0013),
    ];

    /// <summary>$91:AB3A: 20 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAB3A =
    [
        new(0x0000, 0x0a80, 0x006a),
        new(0x0000, 0x0680, 0x006c),
        new(0x0000, 0x0290, 0x006a),
        new(0x0000, 0x02a0, 0x006c),
        new(0x0000, 0x0a00, 0x006a),
        new(0x0000, 0x0600, 0x006c),
        new(0x0000, 0x0180, 0x0030),
        new(0x0000, 0x0880, 0x0016),
        new(0x0000, 0x0480, 0x0018),
        new(0x0000, 0x0090, 0x006a),
        new(0x0000, 0x00a0, 0x006c),
        new(0x0000, 0x0280, 0x0052),
        new(0x0000, 0x00c0, 0x0014),
        new(0x0000, 0x0100, 0x0030),
        new(0x0000, 0x0800, 0x0016),
        new(0x0000, 0x0400, 0x0018),
        new(0x0000, 0x0010, 0x006a),
        new(0x0000, 0x0020, 0x006c),
        new(0x0000, 0x0200, 0x0052),
        new(0x0000, 0x0040, 0x0014),
    ];

    /// <summary>$91:ABB4: 23 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListABB4 =
    [
        new(0x0400, 0x0000, 0x0037),
        new(0x0000, 0x0980, 0x0069),
        new(0x0000, 0x0580, 0x006b),
        new(0x0000, 0x0190, 0x0069),
        new(0x0000, 0x01a0, 0x006b),
        new(0x0000, 0x01c0, 0x0013),
        new(0x0000, 0x0900, 0x0069),
        new(0x0000, 0x0500, 0x006b),
        new(0x0000, 0x0280, 0x002f),
        new(0x0000, 0x0880, 0x0015),
        new(0x0000, 0x0480, 0x0017),
        new(0x0000, 0x0090, 0x0069),
        new(0x0000, 0x00a0, 0x006b),
        new(0x0000, 0x0180, 0x0051),
        new(0x0000, 0x00c0, 0x0013),
        new(0x0000, 0x0200, 0x002f),
        new(0x0000, 0x0800, 0x0015),
        new(0x0000, 0x0400, 0x0017),
        new(0x0000, 0x0010, 0x0069),
        new(0x0000, 0x0020, 0x006b),
        new(0x0000, 0x0100, 0x0051),
        new(0x0000, 0x0080, 0x0017),
        new(0x0000, 0x0040, 0x0013),
    ];

    /// <summary>$91:AC40: 23 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAC40 =
    [
        new(0x0400, 0x0000, 0x0038),
        new(0x0000, 0x0a80, 0x006a),
        new(0x0000, 0x0680, 0x006c),
        new(0x0000, 0x0290, 0x006a),
        new(0x0000, 0x02a0, 0x006c),
        new(0x0000, 0x02a0, 0x006c),
        new(0x0000, 0x0a00, 0x006a),
        new(0x0000, 0x0600, 0x006c),
        new(0x0000, 0x0180, 0x0030),
        new(0x0000, 0x0880, 0x0016),
        new(0x0000, 0x0480, 0x0018),
        new(0x0000, 0x0090, 0x006a),
        new(0x0000, 0x00a0, 0x006c),
        new(0x0000, 0x0280, 0x0052),
        new(0x0000, 0x00c0, 0x0014),
        new(0x0000, 0x0100, 0x0030),
        new(0x0000, 0x0800, 0x0016),
        new(0x0000, 0x0400, 0x0018),
        new(0x0000, 0x0010, 0x006a),
        new(0x0000, 0x0020, 0x006c),
        new(0x0000, 0x0200, 0x0052),
        new(0x0000, 0x0080, 0x0018),
        new(0x0000, 0x0040, 0x0014),
    ];

    /// <summary>$91:ACCC: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListACCC =
    [
        new(0x0000, 0x0140, 0x0067),
        new(0x0000, 0x0840, 0x002b),
        new(0x0000, 0x0440, 0x002d),
    ];

    /// <summary>$91:ACE0: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListACE0 =
    [
        new(0x0000, 0x0240, 0x0068),
        new(0x0000, 0x0840, 0x002c),
        new(0x0000, 0x0440, 0x002e),
    ];

    /// <summary>$91:ACF4: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListACF4 =
    [
        new(0x0000, 0x0280, 0x001a),
        new(0x0080, 0x0000, 0x004c),
        new(0x0000, 0x0200, 0x0025),
    ];

    /// <summary>$91:AD08: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAD08 =
    [
        new(0x0000, 0x0180, 0x0019),
        new(0x0080, 0x0000, 0x004b),
        new(0x0000, 0x0100, 0x0026),
    ];

    /// <summary>$91:AD1C: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAD1C =
    [
        new(0x0080, 0x0200, 0x001a),
        new(0x0080, 0x0000, 0x004c),
        new(0x0000, 0x0200, 0x008b),
    ];

    /// <summary>$91:AD30: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAD30 =
    [
        new(0x0080, 0x0100, 0x0019),
        new(0x0080, 0x0000, 0x004b),
        new(0x0000, 0x0100, 0x008c),
    ];

    /// <summary>$91:AD44: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAD44 =
    [
        new(0x0080, 0x0200, 0x001a),
        new(0x0080, 0x0000, 0x004c),
        new(0x0000, 0x0200, 0x008d),
    ];

    /// <summary>$91:AD58: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAD58 =
    [
        new(0x0080, 0x0100, 0x0019),
        new(0x0080, 0x0000, 0x004b),
        new(0x0000, 0x0100, 0x008e),
    ];

    /// <summary>$91:AD6C: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAD6C =
    [
        new(0x0000, 0x0880, 0x00cb),
        new(0x0000, 0x0090, 0x00cd),
        new(0x0000, 0x0180, 0x00c9),
    ];

    /// <summary>$91:AD80: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAD80 =
    [
        new(0x0000, 0x0880, 0x00cc),
        new(0x0000, 0x0090, 0x00ce),
        new(0x0000, 0x0280, 0x00ca),
    ];

    /// <summary>$91:AD94: 10 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAD94 =
    [
        new(0x0400, 0x0000, 0x0037),
        new(0x0000, 0x0900, 0x006d),
        new(0x0000, 0x0500, 0x006f),
        new(0x0000, 0x0800, 0x002b),
        new(0x0000, 0x0400, 0x002d),
        new(0x0000, 0x0200, 0x0087),
        new(0x0000, 0x0010, 0x006d),
        new(0x0000, 0x0020, 0x006f),
        new(0x0000, 0x0040, 0x0067),
        new(0x0000, 0x0100, 0x0029),
    ];

    /// <summary>$91:ADD2: 10 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListADD2 =
    [
        new(0x0400, 0x0000, 0x0038),
        new(0x0000, 0x0a00, 0x006e),
        new(0x0000, 0x0600, 0x0070),
        new(0x0000, 0x0800, 0x002c),
        new(0x0000, 0x0400, 0x002e),
        new(0x0000, 0x0100, 0x0088),
        new(0x0000, 0x0010, 0x006e),
        new(0x0000, 0x0020, 0x0070),
        new(0x0000, 0x0040, 0x0068),
        new(0x0000, 0x0200, 0x002a),
    ];

    /// <summary>$91:AE10: 1 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAE10 =
    [
        new(0x0800, 0x0000, 0x00de),
    ];

    /// <summary>$91:AE18: 10 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAE18 =
    [
        new(0x0000, 0x0a40, 0x00bb),
        new(0x0000, 0x0640, 0x00bd),
        new(0x0000, 0x0240, 0x00bc),
        new(0x0000, 0x0010, 0x00bb),
        new(0x0000, 0x0020, 0x00bd),
        new(0x0000, 0x0040, 0x00bc),
        new(0x0000, 0x0200, 0x00be),
        new(0x0000, 0x0100, 0x00be),
        new(0x0000, 0x0800, 0x00be),
        new(0x0000, 0x0400, 0x00be),
    ];

    /// <summary>$91:AE56: 10 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAE56 =
    [
        new(0x0000, 0x0940, 0x00ed),
        new(0x0000, 0x0540, 0x00ef),
        new(0x0000, 0x0140, 0x00ee),
        new(0x0000, 0x0010, 0x00ed),
        new(0x0000, 0x0020, 0x00ef),
        new(0x0000, 0x0040, 0x00ee),
        new(0x0000, 0x0200, 0x00f0),
        new(0x0000, 0x0100, 0x00f0),
        new(0x0000, 0x0800, 0x00f0),
        new(0x0000, 0x0400, 0x00f0),
    ];

    /// <summary>$91:AE94: 12 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAE94 =
    [
        new(0x0400, 0x0000, 0x0035),
        new(0x0080, 0x0000, 0x0019),
        new(0x0000, 0x0110, 0x000f),
        new(0x0000, 0x0120, 0x0011),
        new(0x0000, 0x0900, 0x000f),
        new(0x0000, 0x0500, 0x0011),
        new(0x0000, 0x0140, 0x000b),
        new(0x0000, 0x0100, 0x000b),
        new(0x0000, 0x0200, 0x0025),
        new(0x0000, 0x0800, 0x0003),
        new(0x0000, 0x0010, 0x0005),
        new(0x0000, 0x0020, 0x0007),
    ];

    /// <summary>$91:AEDE: 12 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAEDE =
    [
        new(0x0400, 0x0000, 0x0036),
        new(0x0080, 0x0000, 0x001a),
        new(0x0000, 0x0210, 0x0010),
        new(0x0000, 0x0220, 0x0012),
        new(0x0000, 0x0a00, 0x0010),
        new(0x0000, 0x0600, 0x0012),
        new(0x0000, 0x0240, 0x000c),
        new(0x0000, 0x0200, 0x000c),
        new(0x0000, 0x0100, 0x0026),
        new(0x0000, 0x0800, 0x0004),
        new(0x0000, 0x0010, 0x0006),
        new(0x0000, 0x0020, 0x0008),
    ];

    /// <summary>$91:AF28: 9 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAF28 =
    [
        new(0x0000, 0x0900, 0x006d),
        new(0x0000, 0x0500, 0x006f),
        new(0x0000, 0x0800, 0x002b),
        new(0x0000, 0x0400, 0x002d),
        new(0x0000, 0x0200, 0x0087),
        new(0x0000, 0x0010, 0x006d),
        new(0x0000, 0x0020, 0x006f),
        new(0x0000, 0x0040, 0x0067),
        new(0x0000, 0x0100, 0x0067),
    ];

    /// <summary>$91:AF60: 9 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAF60 =
    [
        new(0x0000, 0x0a00, 0x006e),
        new(0x0000, 0x0600, 0x0070),
        new(0x0000, 0x0800, 0x002c),
        new(0x0000, 0x0400, 0x002e),
        new(0x0000, 0x0100, 0x0088),
        new(0x0000, 0x0010, 0x006e),
        new(0x0000, 0x0020, 0x0070),
        new(0x0000, 0x0040, 0x0068),
        new(0x0000, 0x0200, 0x0068),
    ];

    /// <summary>$91:AF98: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAF98 =
    [
        new(0x0000, 0x0280, 0x001a),
        new(0x0080, 0x0000, 0x004c),
        new(0x0000, 0x0200, 0x00bf),
    ];

    /// <summary>$91:AFAC: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAFAC =
    [
        new(0x0000, 0x0180, 0x0019),
        new(0x0080, 0x0000, 0x004b),
        new(0x0000, 0x0100, 0x00c0),
    ];

    /// <summary>$91:AFC0: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAFC0 =
    [
        new(0x0080, 0x0200, 0x001a),
        new(0x0080, 0x0000, 0x004c),
        new(0x0000, 0x0200, 0x00c1),
    ];

    /// <summary>$91:AFD4: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAFD4 =
    [
        new(0x0080, 0x0100, 0x0019),
        new(0x0080, 0x0000, 0x004b),
        new(0x0000, 0x0100, 0x00c2),
    ];

    /// <summary>$91:AFE8: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAFE8 =
    [
        new(0x0080, 0x0200, 0x001a),
        new(0x0080, 0x0000, 0x004c),
        new(0x0000, 0x0200, 0x00c3),
    ];

    /// <summary>$91:AFFC: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListAFFC =
    [
        new(0x0080, 0x0100, 0x0019),
        new(0x0080, 0x0000, 0x004b),
        new(0x0000, 0x0100, 0x00c4),
    ];

    internal static ReadOnlySpan<SamusPoseInputRule> Resolve(ushort pointer) => pointer switch
    {
        0xa90c => ListA90C,
        0xa926 => ListA926,
        0xa940 => ListA940,
        0xa954 => ListA954,
        0xa968 => ListA968,
        0xa97c => ListA97C,
        0xa990 => ListA990,
        0xa998 => ListA998,
        0xa9a0 => ListA9A0,
        0xa9c6 => ListA9C6,
        0xa9ec => ListA9EC,
        0xaa12 => ListAA12,
        0xaa38 => ListAA38,
        0xaa7c => ListAA7C,
        0xaac0 => ListAAC0,
        0xab3a => ListAB3A,
        0xabb4 => ListABB4,
        0xac40 => ListAC40,
        0xaccc => ListACCC,
        0xace0 => ListACE0,
        0xacf4 => ListACF4,
        0xad08 => ListAD08,
        0xad1c => ListAD1C,
        0xad30 => ListAD30,
        0xad44 => ListAD44,
        0xad58 => ListAD58,
        0xad6c => ListAD6C,
        0xad80 => ListAD80,
        0xad94 => ListAD94,
        0xadd2 => ListADD2,
        0xae10 => ListAE10,
        0xae18 => ListAE18,
        0xae56 => ListAE56,
        0xae94 => ListAE94,
        0xaede => ListAEDE,
        0xaf28 => ListAF28,
        0xaf60 => ListAF60,
        0xaf98 => ListAF98,
        0xafac => ListAFAC,
        0xafc0 => ListAFC0,
        0xafd4 => ListAFD4,
        0xafe8 => ListAFE8,
        0xaffc => ListAFFC,
        _ => throw new InvalidOperationException($"Unknown compiled pose-input list ${pointer:X4}."),
    };
}
