namespace SuperMetroid.Core.Game;

/// <summary>Ordered native transition lists $91:A0DC..A90B; first matching condition wins.</summary>
internal static class SamusPoseInputRulesEarly
{
    /// <summary>$91:A0DC: 0 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA0DC =
    [
    ];

    /// <summary>$91:A0DE: 2 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA0DE =
    [
        new(0x0000, 0x0100, 0x0026),
        new(0x0000, 0x0200, 0x0025),
    ];

    /// <summary>$91:A0EC: 22 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA0EC =
    [
        new(0x0080, 0x0800, 0x0055),
        new(0x0080, 0x0010, 0x0057),
        new(0x0080, 0x0020, 0x0059),
        new(0x0080, 0x0000, 0x004b),
        new(0x0400, 0x0030, 0x00f1),
        new(0x0400, 0x0010, 0x00f3),
        new(0x0400, 0x0020, 0x00f5),
        new(0x0400, 0x0000, 0x0035),
        new(0x0000, 0x0260, 0x0078),
        new(0x0000, 0x0250, 0x0076),
        new(0x0000, 0x0230, 0x0025),
        new(0x0000, 0x0030, 0x0003),
        new(0x0000, 0x0110, 0x000f),
        new(0x0000, 0x0120, 0x0011),
        new(0x0000, 0x0900, 0x000f),
        new(0x0000, 0x0500, 0x0011),
        new(0x0000, 0x0240, 0x004a),
        new(0x0000, 0x0200, 0x0025),
        new(0x0000, 0x0800, 0x0003),
        new(0x0000, 0x0010, 0x0005),
        new(0x0000, 0x0020, 0x0007),
        new(0x0000, 0x0100, 0x0009),
    ];

    /// <summary>$91:A172: 22 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA172 =
    [
        new(0x0080, 0x0800, 0x0056),
        new(0x0080, 0x0010, 0x0058),
        new(0x0080, 0x0020, 0x005a),
        new(0x0080, 0x0000, 0x004c),
        new(0x0400, 0x0030, 0x00f2),
        new(0x0400, 0x0010, 0x00f4),
        new(0x0400, 0x0020, 0x00f6),
        new(0x0400, 0x0000, 0x0036),
        new(0x0000, 0x0160, 0x0077),
        new(0x0000, 0x0150, 0x0075),
        new(0x0000, 0x0130, 0x0026),
        new(0x0000, 0x0030, 0x0004),
        new(0x0000, 0x0210, 0x0010),
        new(0x0000, 0x0220, 0x0012),
        new(0x0000, 0x0a00, 0x0010),
        new(0x0000, 0x0600, 0x0012),
        new(0x0000, 0x0140, 0x0049),
        new(0x0000, 0x0100, 0x0026),
        new(0x0000, 0x0800, 0x0004),
        new(0x0000, 0x0010, 0x0006),
        new(0x0000, 0x0020, 0x0008),
        new(0x0000, 0x0200, 0x000a),
    ];

    /// <summary>$91:A1F8: 12 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA1F8 =
    [
        new(0x0400, 0x0000, 0x0035),
        new(0x0080, 0x0000, 0x0019),
        new(0x0000, 0x0110, 0x000f),
        new(0x0000, 0x0120, 0x0011),
        new(0x0000, 0x0900, 0x000f),
        new(0x0000, 0x0500, 0x0011),
        new(0x0000, 0x0140, 0x000b),
        new(0x0000, 0x0100, 0x0009),
        new(0x0000, 0x0200, 0x0025),
        new(0x0000, 0x0800, 0x0003),
        new(0x0000, 0x0010, 0x0005),
        new(0x0000, 0x0020, 0x0007),
    ];

    /// <summary>$91:A242: 12 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA242 =
    [
        new(0x0400, 0x0000, 0x0036),
        new(0x0080, 0x0000, 0x001a),
        new(0x0000, 0x0210, 0x0010),
        new(0x0000, 0x0220, 0x0012),
        new(0x0000, 0x0a00, 0x0010),
        new(0x0000, 0x0600, 0x0012),
        new(0x0000, 0x0240, 0x000c),
        new(0x0000, 0x0200, 0x000a),
        new(0x0000, 0x0100, 0x0026),
        new(0x0000, 0x0800, 0x0004),
        new(0x0000, 0x0010, 0x0006),
        new(0x0000, 0x0020, 0x0008),
    ];

    /// <summary>$91:A28C: 8 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA28C =
    [
        new(0x0000, 0x0280, 0x002f),
        new(0x0000, 0x0880, 0x0015),
        new(0x0000, 0x0480, 0x0017),
        new(0x0000, 0x0090, 0x0069),
        new(0x0000, 0x00a0, 0x006b),
        new(0x0000, 0x0180, 0x0051),
        new(0x0000, 0x00c0, 0x0013),
        new(0x0000, 0x0040, 0x0013),
    ];

    /// <summary>$91:A2BE: 9 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA2BE =
    [
        new(0x0000, 0x0180, 0x0030),
        new(0x0000, 0x0880, 0x0016),
        new(0x0000, 0x0480, 0x0018),
        new(0x0000, 0x0090, 0x006a),
        new(0x0000, 0x00a0, 0x006c),
        new(0x0000, 0x0280, 0x0052),
        new(0x0000, 0x00c0, 0x0014),
        new(0x0000, 0x0100, 0x0030),
        new(0x0000, 0x0040, 0x0014),
    ];

    /// <summary>$91:A2F6: 21 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA2F6 =
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
        new(0x0000, 0x0080, 0x004d),
        new(0x0000, 0x0040, 0x0013),
    ];

    /// <summary>$91:A376: 21 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA376 =
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
        new(0x0000, 0x0080, 0x004e),
        new(0x0000, 0x0040, 0x0014),
    ];

    /// <summary>$91:A3F6: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA3F6 =
    [
        new(0x0000, 0x0280, 0x0052),
        new(0x0000, 0x0180, 0x004f),
        new(0x0000, 0x0080, 0x004e),
    ];

    /// <summary>$91:A40A: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA40A =
    [
        new(0x0000, 0x0280, 0x0050),
        new(0x0000, 0x0180, 0x0051),
        new(0x0000, 0x0080, 0x004d),
    ];

    /// <summary>$91:A41E: 13 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA41E =
    [
        new(0x0040, 0x0000, 0x0013),
        new(0x0040, 0x0100, 0x0013),
        new(0x0000, 0x0840, 0x0015),
        new(0x0000, 0x0440, 0x0017),
        new(0x0000, 0x0050, 0x0069),
        new(0x0000, 0x0060, 0x006b),
        new(0x0000, 0x0180, 0x0019),
        new(0x0000, 0x0800, 0x0015),
        new(0x0000, 0x0010, 0x0069),
        new(0x0000, 0x0020, 0x006b),
        new(0x0000, 0x0400, 0x0017),
        new(0x0000, 0x0100, 0x0019),
        new(0x0000, 0x0200, 0x001a),
    ];

    /// <summary>$91:A46E: 13 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA46E =
    [
        new(0x0040, 0x0000, 0x0014),
        new(0x0040, 0x0200, 0x0014),
        new(0x0000, 0x0840, 0x0016),
        new(0x0000, 0x0440, 0x0018),
        new(0x0000, 0x0050, 0x006a),
        new(0x0000, 0x0060, 0x006c),
        new(0x0000, 0x0280, 0x001a),
        new(0x0000, 0x0800, 0x0016),
        new(0x0000, 0x0010, 0x006a),
        new(0x0000, 0x0020, 0x006c),
        new(0x0000, 0x0400, 0x0018),
        new(0x0000, 0x0200, 0x001a),
        new(0x0000, 0x0100, 0x0019),
    ];

    /// <summary>$91:A4BE: 13 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA4BE =
    [
        new(0x0040, 0x0000, 0x0013),
        new(0x0040, 0x0100, 0x0013),
        new(0x0000, 0x0840, 0x0015),
        new(0x0000, 0x0440, 0x0017),
        new(0x0000, 0x0050, 0x0069),
        new(0x0000, 0x0060, 0x006b),
        new(0x0000, 0x0180, 0x001b),
        new(0x0000, 0x0800, 0x0015),
        new(0x0000, 0x0010, 0x0069),
        new(0x0000, 0x0020, 0x006b),
        new(0x0000, 0x0400, 0x0017),
        new(0x0000, 0x0100, 0x001b),
        new(0x0000, 0x0200, 0x001c),
    ];

    /// <summary>$91:A50E: 13 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA50E =
    [
        new(0x0040, 0x0000, 0x0014),
        new(0x0040, 0x0200, 0x0014),
        new(0x0000, 0x0840, 0x0016),
        new(0x0000, 0x0440, 0x0018),
        new(0x0000, 0x0050, 0x006a),
        new(0x0000, 0x0060, 0x006c),
        new(0x0000, 0x0280, 0x001c),
        new(0x0000, 0x0800, 0x0016),
        new(0x0000, 0x0010, 0x006a),
        new(0x0000, 0x0020, 0x006c),
        new(0x0000, 0x0400, 0x0018),
        new(0x0000, 0x0200, 0x001c),
        new(0x0000, 0x0100, 0x001b),
    ];

    /// <summary>$91:A55E: 13 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA55E =
    [
        new(0x0040, 0x0000, 0x0013),
        new(0x0040, 0x0100, 0x0013),
        new(0x0000, 0x0840, 0x0015),
        new(0x0000, 0x0440, 0x0017),
        new(0x0000, 0x0050, 0x0069),
        new(0x0000, 0x0060, 0x006b),
        new(0x0000, 0x0180, 0x0081),
        new(0x0000, 0x0800, 0x0015),
        new(0x0000, 0x0010, 0x0069),
        new(0x0000, 0x0020, 0x006b),
        new(0x0000, 0x0400, 0x0017),
        new(0x0000, 0x0100, 0x0081),
        new(0x0000, 0x0200, 0x0082),
    ];

    /// <summary>$91:A5AE: 13 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA5AE =
    [
        new(0x0040, 0x0000, 0x0014),
        new(0x0040, 0x0200, 0x0014),
        new(0x0000, 0x0840, 0x0016),
        new(0x0000, 0x0440, 0x0018),
        new(0x0000, 0x0050, 0x006a),
        new(0x0000, 0x0060, 0x006c),
        new(0x0000, 0x0280, 0x0082),
        new(0x0000, 0x0800, 0x0016),
        new(0x0000, 0x0010, 0x006a),
        new(0x0000, 0x0020, 0x006c),
        new(0x0000, 0x0400, 0x0018),
        new(0x0000, 0x0200, 0x0082),
        new(0x0000, 0x0100, 0x0081),
    ];

    /// <summary>$91:A5FE: 4 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA5FE =
    [
        new(0x0800, 0x0000, 0x003d),
        new(0x0080, 0x0000, 0x003d),
        new(0x0000, 0x0100, 0x001e),
        new(0x0000, 0x0200, 0x001f),
    ];

    /// <summary>$91:A618: 4 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA618 =
    [
        new(0x0800, 0x0000, 0x003d),
        new(0x0080, 0x0000, 0x003d),
        new(0x0000, 0x0100, 0x001e),
        new(0x0000, 0x0200, 0x001f),
    ];

    /// <summary>$91:A632: 4 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA632 =
    [
        new(0x0800, 0x0000, 0x003e),
        new(0x0080, 0x0000, 0x003e),
        new(0x0000, 0x0100, 0x001e),
        new(0x0000, 0x0200, 0x001f),
    ];

    /// <summary>$91:A64C: 4 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA64C =
    [
        new(0x0800, 0x0000, 0x003e),
        new(0x0080, 0x0000, 0x003e),
        new(0x0000, 0x0100, 0x001e),
        new(0x0000, 0x0200, 0x001f),
    ];

    /// <summary>$91:A666: 0 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA666 =
    [
    ];

    /// <summary>$91:A668: 0 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA668 =
    [
    ];

    /// <summary>$91:A66A: 0 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA66A =
    [
    ];

    /// <summary>$91:A66C: 13 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA66C =
    [
        new(0x0800, 0x0030, 0x00f7),
        new(0x0800, 0x0010, 0x00f9),
        new(0x0800, 0x0020, 0x00fb),
        new(0x0800, 0x0000, 0x003b),
        new(0x0200, 0x0000, 0x0043),
        new(0x0400, 0x0000, 0x0037),
        new(0x0080, 0x0000, 0x004b),
        new(0x0000, 0x0030, 0x0085),
        new(0x0000, 0x0110, 0x0001),
        new(0x0000, 0x0120, 0x0001),
        new(0x0000, 0x0010, 0x0071),
        new(0x0000, 0x0020, 0x0073),
        new(0x0000, 0x0100, 0x0001),
    ];

    /// <summary>$91:A6BC: 13 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA6BC =
    [
        new(0x0800, 0x0030, 0x00f8),
        new(0x0800, 0x0010, 0x00fa),
        new(0x0800, 0x0020, 0x00fc),
        new(0x0800, 0x0000, 0x003c),
        new(0x0100, 0x0000, 0x0044),
        new(0x0400, 0x0000, 0x0038),
        new(0x0080, 0x0000, 0x004c),
        new(0x0000, 0x0030, 0x0086),
        new(0x0000, 0x0220, 0x0002),
        new(0x0000, 0x0210, 0x0002),
        new(0x0000, 0x0010, 0x0072),
        new(0x0000, 0x0020, 0x0074),
        new(0x0000, 0x0200, 0x0002),
    ];

    /// <summary>$91:A70C: 11 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA70C =
    [
        new(0x0000, 0x0900, 0x006d),
        new(0x0000, 0x0500, 0x006f),
        new(0x0000, 0x0a00, 0x0087),
        new(0x0000, 0x0600, 0x0087),
        new(0x0000, 0x0200, 0x0087),
        new(0x0000, 0x0800, 0x002b),
        new(0x0000, 0x0400, 0x002d),
        new(0x0000, 0x0010, 0x006d),
        new(0x0000, 0x0020, 0x006f),
        new(0x0000, 0x0040, 0x0067),
        new(0x0000, 0x0100, 0x0029),
    ];

    /// <summary>$91:A750: 11 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA750 =
    [
        new(0x0000, 0x0a00, 0x006e),
        new(0x0000, 0x0600, 0x0070),
        new(0x0000, 0x0900, 0x0088),
        new(0x0000, 0x0500, 0x0088),
        new(0x0000, 0x0100, 0x0088),
        new(0x0000, 0x0800, 0x002c),
        new(0x0000, 0x0400, 0x002e),
        new(0x0000, 0x0010, 0x006e),
        new(0x0000, 0x0020, 0x0070),
        new(0x0000, 0x0040, 0x0068),
        new(0x0000, 0x0200, 0x002a),
    ];

    /// <summary>$91:A794: 4 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA794 =
    [
        new(0x0800, 0x0000, 0x003d),
        new(0x0080, 0x0000, 0x003d),
        new(0x0000, 0x0100, 0x0031),
        new(0x0000, 0x0200, 0x0032),
    ];

    /// <summary>$91:A7AE: 4 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA7AE =
    [
        new(0x0800, 0x0000, 0x003e),
        new(0x0080, 0x0000, 0x003e),
        new(0x0000, 0x0200, 0x0032),
        new(0x0000, 0x0100, 0x0031),
    ];

    /// <summary>$91:A7C8: 0 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA7C8 =
    [
    ];

    /// <summary>$91:A7CA: 0 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA7CA =
    [
    ];

    /// <summary>$91:A7CC: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA7CC =
    [
        new(0x0000, 0x0240, 0x0045),
        new(0x0000, 0x0100, 0x0009),
        new(0x0000, 0x0200, 0x0025),
    ];

    /// <summary>$91:A7E0: 3 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA7E0 =
    [
        new(0x0000, 0x0140, 0x0046),
        new(0x0000, 0x0200, 0x000a),
        new(0x0000, 0x0100, 0x0026),
    ];

    /// <summary>$91:A7F4: 0 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA7F4 =
    [
    ];

    /// <summary>$91:A834: 0 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA834 =
    [
    ];

    /// <summary>$91:A874: 9 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA874 =
    [
        new(0x0400, 0x0000, 0x0036),
        new(0x0080, 0x0000, 0x00c0),
        new(0x0080, 0x0010, 0x00c2),
        new(0x0080, 0x0020, 0x00c4),
        new(0x0000, 0x0160, 0x0077),
        new(0x0000, 0x0150, 0x0075),
        new(0x0000, 0x0140, 0x0049),
        new(0x0000, 0x0200, 0x000a),
        new(0x0000, 0x0100, 0x0026),
    ];

    /// <summary>$91:A8AC: 9 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA8AC =
    [
        new(0x0400, 0x0000, 0x0035),
        new(0x0080, 0x0000, 0x00bf),
        new(0x0080, 0x0010, 0x00c1),
        new(0x0080, 0x0020, 0x00c3),
        new(0x0000, 0x0250, 0x0076),
        new(0x0000, 0x0260, 0x0078),
        new(0x0000, 0x0240, 0x004a),
        new(0x0000, 0x0100, 0x0009),
        new(0x0000, 0x0200, 0x0025),
    ];

    /// <summary>$91:A8E4: 1 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA8E4 =
    [
        new(0x0000, 0x0280, 0x0050),
    ];

    /// <summary>$91:A8EC: 1 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA8EC =
    [
        new(0x0000, 0x0180, 0x004f),
    ];

    /// <summary>$91:A8FC: 1 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA8FC =
    [
        new(0x0000, 0x0280, 0x0066),
    ];

    /// <summary>$91:A904: 1 ordered conditions followed by the native terminator.</summary>
    private static readonly SamusPoseInputRule[] ListA904 =
    [
        new(0x0000, 0x0180, 0x0065),
    ];

    internal static ReadOnlySpan<SamusPoseInputRule> Resolve(ushort pointer) => pointer switch
    {
        0xa0dc => ListA0DC,
        0xa0de => ListA0DE,
        0xa0ec => ListA0EC,
        0xa172 => ListA172,
        0xa1f8 => ListA1F8,
        0xa242 => ListA242,
        0xa28c => ListA28C,
        0xa2be => ListA2BE,
        0xa2f6 => ListA2F6,
        0xa376 => ListA376,
        0xa3f6 => ListA3F6,
        0xa40a => ListA40A,
        0xa41e => ListA41E,
        0xa46e => ListA46E,
        0xa4be => ListA4BE,
        0xa50e => ListA50E,
        0xa55e => ListA55E,
        0xa5ae => ListA5AE,
        0xa5fe => ListA5FE,
        0xa618 => ListA618,
        0xa632 => ListA632,
        0xa64c => ListA64C,
        0xa666 => ListA666,
        0xa668 => ListA668,
        0xa66a => ListA66A,
        0xa66c => ListA66C,
        0xa6bc => ListA6BC,
        0xa70c => ListA70C,
        0xa750 => ListA750,
        0xa794 => ListA794,
        0xa7ae => ListA7AE,
        0xa7c8 => ListA7C8,
        0xa7ca => ListA7CA,
        0xa7cc => ListA7CC,
        0xa7e0 => ListA7E0,
        0xa7f4 => ListA7F4,
        0xa834 => ListA834,
        0xa874 => ListA874,
        0xa8ac => ListA8AC,
        0xa8e4 => ListA8E4,
        0xa8ec => ListA8EC,
        0xa8fc => ListA8FC,
        0xa904 => ListA904,
        _ => throw new InvalidOperationException($"Unknown compiled pose-input list ${pointer:X4}."),
    };
}
