using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Areas.Ceres.CeresDoorNativeListAddresses;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>#1290: the Ceres door's typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class CeresDoorAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyCeresDoorAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, CeresDoorAnimations.Segments, CeresDoorAnimations.StartingAt,
            CeresDoorFrameDefinitionsConstants.Spritemap,
            new NativeEnemyCueRules<CeresDoorCue>(word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null, (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertTrue(instruction.Kinds.Contains(cue.Kind), $"{at}: cue {cue.Kind} is one the native instruction signals");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    // Each native branch names the animation the door's owner starts on that event (#2189).
                    'T' => NativeEnemyAnimationOracle.EntryOf(Entries, StartedAnimation(cue.Kind)),
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            }, ShownUnseen: UnseenFrames.Contains), onlyRunOnInto: OnlyRunOnInto);
        Console.WriteLine($"  Ceres door animations: {Entries.Length} segments ({beats} beats) match bank $A6.");
    }

    /// <summary>The animation <see cref="CeresDoorBehavior"/> starts on a waiting door's or wall's event.</summary>
    private static CeresDoorSegment StartedAnimation(CeresDoorCueKind kind) => kind switch
    {
        CeresDoorCueKind.RightDoorAppears => CeresDoorBehavior.RightDoorClosedAnimation,
        CeresDoorCueKind.LeftDoorAppears => CeresDoorBehavior.LeftDoorClosedAnimation,
        CeresDoorCueKind.RightDoorStandsOpen => CeresDoorBehavior.RightDoorClosingAnimation,
        CeresDoorCueKind.LeftDoorStandsOpen => CeresDoorBehavior.LeftDoorClosingAnimation,
        CeresDoorCueKind.RightDoorStandsClosed => CeresDoorBehavior.RightDoorWaitingAnimation,
        CeresDoorCueKind.LeftDoorStandsClosed => CeresDoorBehavior.LeftDoorWaitingAnimation,
        CeresDoorCueKind.RidleyRoomDoorWaits => CeresDoorBehavior.RidleyRoomDoorWaitingAnimation,
        CeresDoorCueKind.ElevatorRoomWallAppears => CeresDoorBehavior.ElevatorRoomWallDoorAnimation,
        CeresDoorCueKind.ShowOverlay or CeresDoorCueKind.MakeIntangible or CeresDoorCueKind.MakeInvisible or
            CeresDoorCueKind.MakeTangible or CeresDoorCueKind.MakeVisible or CeresDoorCueKind.MakeVisibleAndShowActor or
            CeresDoorCueKind.PlayOpeningSound or _ => throw new InvalidOperationException($"Ceres door cue {kind} starts no animation."),
    };
}
