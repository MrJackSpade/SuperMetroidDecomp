using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The compositions shown by the fourteen fixed bank-$8B ending completion-text lists. Counts preserve the cartridge's
/// progressively revealed two-part-per-letter compositions without treating those visuals as timing data.
/// </summary>
public static class EndingCompletionTextSpriteDefinitions
{
    /// <summary>
    /// Fifteen THE OPERATION WAS prefixes, 21 COMPLETED SUCCESSFULLY prefixes and nine CLEAR TIME prefixes, each revealing
    /// one more letter, then the ten two-part digits and the two-part colon.
    /// </summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("operation-00", new(99), 2),
        new("operation-01", new(100), 4),
        new("operation-02", new(101), 6),
        new("operation-03", new(102), 8),
        new("operation-04", new(103), 10),
        new("operation-05", new(104), 12),
        new("operation-06", new(105), 14),
        new("operation-07", new(106), 16),
        new("operation-08", new(107), 18),
        new("operation-09", new(108), 20),
        new("operation-10", new(109), 22),
        new("operation-11", new(110), 24),
        new("operation-12", new(111), 26),
        new("operation-13", new(112), 28),
        new("operation-14", new(113), 30),
        new("completed-00", new(114), 2),
        new("completed-01", new(115), 4),
        new("completed-02", new(116), 6),
        new("completed-03", new(117), 8),
        new("completed-04", new(118), 10),
        new("completed-05", new(119), 12),
        new("completed-06", new(120), 14),
        new("completed-07", new(121), 16),
        new("completed-08", new(122), 18),
        new("completed-09", new(123), 20),
        new("completed-10", new(124), 22),
        new("completed-11", new(125), 24),
        new("completed-12", new(126), 26),
        new("completed-13", new(127), 28),
        new("completed-14", new(128), 30),
        new("completed-15", new(129), 32),
        new("completed-16", new(130), 34),
        new("completed-17", new(131), 36),
        new("completed-18", new(132), 38),
        new("completed-19", new(133), 40),
        new("completed-20", new(134), 42),
        new("clear-time-00", new(135), 2),
        new("clear-time-01", new(136), 4),
        new("clear-time-02", new(137), 6),
        new("clear-time-03", new(138), 8),
        new("clear-time-04", new(139), 10),
        new("clear-time-05", new(140), 12),
        new("clear-time-06", new(141), 14),
        new("clear-time-07", new(142), 16),
        new("clear-time-08", new(143), 18),
        new("digit-00", new(144), 2),
        new("digit-01", new(145), 2),
        new("digit-02", new(146), 2),
        new("digit-03", new(147), 2),
        new("digit-04", new(148), 2),
        new("digit-05", new(149), 2),
        new("digit-06", new(150), 2),
        new("digit-07", new(151), 2),
        new("digit-08", new(152), 2),
        new("digit-09", new(153), 2),
        new("colon-00", new(154), 2),
    ];
}
