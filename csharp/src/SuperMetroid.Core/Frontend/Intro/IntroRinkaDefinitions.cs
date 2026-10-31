namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-object and physical launch definitions for the intro Rinkas.</summary>
internal static class IntroRinkaDefinitions
{

    /// <summary><c>$8B:CF21</c>, intro Rinka initialization, no-op pre-instruction, and initial list.</summary>
    /// <remarks>
    /// Issues #625 and #990: its $8B:CDEB..CE0C instruction program has
    /// 17 words, all matched to pinned NTSC J/U v1.0 ROM and bank_8B.asm.
    /// Three ten-call frames select bank-$8C spritemaps $8C8D, $8CA3,
    /// $8CB9, followed by start-moving opcode $B8C5. The four-frame loop
    /// at $CDF9 then selects $8CA3, $8C8D, $8CA3, $8CB9 for ten calls
    /// each before goto $94BC returns to $CDF9. The generic interpreter
    /// loads the first frame on relative call 0, the callback and first
    /// loop frame on call 30, and subsequent loop frames on calls 40,
    /// 50, and 60. Because pre-instruction runs before list advancement,
    /// movement begins on the following actor call. The frame order is
    /// authored animation policy; retain the bounded two-phase stream.
    ///
    /// Issues #625 and #991: the three selected bank-$8C spritemaps at
    /// $8C8D+$16*f, f=0..2, each have four five-byte entries and tile
    /// $196+f. All 66 bytes match pinned NTSC J/U v1.0 ROM and bank_8C.asm.
    /// Entry j=0..3 uses X=0 for even j or $01F8 for odd j, Y=0 for j&lt;2
    /// or $F8 otherwise, and attributes $3E00 | ($196+f), plus bit $8000
    /// for j&lt;2 and bit $4000 for even j. This is an exact bounded
    /// four-quadrant construction; the frame program selects no other
    /// spritemaps or adjacent visual data.
    /// </remarks>
    public static CinematicActorDefinition RinkaActor =>
        new(CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.IntroRinka);

    /// <summary><c>$8B:CF27</c>, Rinka-spawner no-op callbacks and initial list.</summary>
    /// <remarks>
    /// Issues #625 and #989: its $8B:CE0D..CE1A list contains seven words:
    /// wait $004A, null spritemap, spawn-0/1 opcode $BA21, wait $0080,
    /// null spritemap, spawn-2/3 opcode $BA36, then delete $9438.
    /// All seven match pinned NTSC J/U v1.0 ROM and bank_8B.asm. The generic
    /// interpreter loads the first wait on zero-based call 0, spawns the
    /// first pair on call 74, then the second pair and deletes on call 202.
    /// IntroRinkaSystem follows this native list and verifies both waves.
    /// The unequal waits and two distinct callbacks are authored timing
    /// policy; retain the explicit bounded schedule rather than fit a
    /// recurrence or infer a third wave from adjacent data.
    /// </remarks>
    public static CinematicActorDefinition SpawnerActor =>
        new(CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.IntroRinkaSpawner);

    /// <summary>The values <see cref="IntroRinkaLaunches.Rinka"/> selects, in index order.</summary>
    internal static readonly IntroRinkaPhysicalDefinition[] Rinkas =
    [
        new(new(112, 72), 0),
        new(new(192, 56), -1),
        new(new(128, 48), 0),
        new(new(232, 80), -1),
    ];

    /// <summary>The Rinkas' shared half-pixel X/Y velocity.</summary>
    public static SignedSixteenSixteen HalfPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);
}
