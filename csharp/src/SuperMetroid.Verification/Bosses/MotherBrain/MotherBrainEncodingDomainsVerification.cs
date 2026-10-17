using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the typed domains that replaced Mother Brain's stored words: the wrapping walk credit and its two
/// readings, the hitbox-parts codec, the neck angles' full-turn wrap in the production dispatcher, and the snapshot
/// import's rejection of words outside each domain.
/// </summary>
internal static class MotherBrainEncodingDomainsVerification
{
    /// <summary>Number of distinct values a cartridge word holds; native word arithmetic is modulo this.</summary>
    private const int WordRange = 65536;

    /// <summary>The first word value whose sign bit is set.</summary>
    private const int SignedWordLimit = 32768;

    /// <summary>Runs every Mother Brain typed-domain check.</summary>
    internal static void VerifyMotherBrainEncodingDomains()
    {
        VerifyWalkCreditEarningWraps();
        VerifyWalkCreditSpendingReadsSigned();
        VerifyShotReactionAgainstNativeArithmetic();
        VerifyHitboxPartsCodec();
        VerifyNeckAngleWrapThroughDispatcher();
        VerifySnapshotDecodesRejectUndefinedWords();
        Console.WriteLine(
            "Mother Brain encoding domains: walk-credit wrap and signed spending, shot reactions, hitbox-parts codec, neck-angle wrap and snapshot decodes match native arithmetic.");
    }

    /// <summary>The native unsigned add: the sum wraps to the low sixteen bits.</summary>
    private static int NativeAdd(int word, int amount) => (word + amount) % WordRange;

    /// <summary>The native shot spend: subtract, then keep the word only when its sign bit is clear.</summary>
    private static int NativeSignedSpend(int word, int cost)
    {
        int remainder = ((word - cost) % WordRange + WordRange) % WordRange;
        return remainder >= SignedWordLimit ? 0 : remainder;
    }

    /// <summary>Earning adds without saturation and compares unsigned, so a wrapped credit is below one step again.</summary>
    private static void VerifyWalkCreditEarningWraps()
    {
        foreach ((int start, int earned) in new[] { (65530, 32), (65535, 6), (250, 6), (224, 32), (40000, 6) })
        {
            MotherBrainWalkCredit credit = MotherBrainWalkCredit.FromNativeWord(checked((ushort)start)).AddCredit(earned);
            int expected = NativeAdd(start, earned);
            AssertEqual(expected, credit.Word, $"walk credit {start} plus {earned} wraps like the native add");
            AssertEqual(expected < MotherBrainWalkCredit.StepCredit, credit.BelowStep,
                $"walk credit {start} plus {earned} compares unsigned against one step");
            AssertEqual(expected == 0, credit.IsEmpty, $"walk credit {start} plus {earned} emptiness");
        }
    }

    /// <summary>A shot's cost reads the credit as signed; a bomb's step reads it unsigned; the two disagree above the sign bit.</summary>
    private static void VerifyWalkCreditSpendingReadsSigned()
    {
        foreach (int start in new[] { 0, 100, 255, 256, 300, 4096, 32767, 32768, 33000, 40000, 65535 })
        {
            MotherBrainWalkCredit credit = MotherBrainWalkCredit.FromNativeWord(checked((ushort)start));
            foreach (int cost in new[] { MotherBrainWalkCredit.StepCredit, MotherBrainWalkCredit.HyperBeamShotCost })
            {
                int expected = NativeSignedSpend(start, cost);
                AssertEqual(expected, credit.SpendForShot(cost).Word, $"walk credit {start} less shot cost {cost}");
                int remainder = ((start - cost) % WordRange + WordRange) % WordRange;
                AssertEqual(remainder < SignedWordLimit, credit.CoversShot(cost),
                    $"walk credit {start} covers shot cost {cost} exactly when the signed remainder is non-negative");
            }

            int bombExpected = start < MotherBrainWalkCredit.StepCredit ? 0 : start - MotherBrainWalkCredit.StepCredit;
            AssertEqual(bombExpected, credit.SpendStepForBomb().Word, $"walk credit {start} less one bomb step");
        }

        // 40,000 lies above the sign bit: a shot empties it while a bomb leaves 39,744.
        MotherBrainWalkCredit high = MotherBrainWalkCredit.FromNativeWord(40000);
        AssertEqual(MotherBrainWalkCredit.Empty, high.SpendForShot(MotherBrainWalkCredit.StepCredit),
            "a shot reads a credit above the sign bit as overdrawn");
        AssertEqual(MotherBrainWalkCredit.Of(39744), high.SpendStepForBomb(),
            "a bomb reads the same credit unsigned and spends one step");
    }

    /// <summary>The production shot reaction spends credit and recoils exactly as the native table and branches do.</summary>
    private static void VerifyShotReactionAgainstNativeArithmetic()
    {
        foreach (int start in new[] { 0, 200, 265, 266, 300, 4096, 40000 })
        {
            MotherBrainWalkCredit credit = MotherBrainWalkCredit.FromNativeWord(checked((ushort)start));

            MotherBrainShotReactionResult hyper = MotherBrainShotReaction.Resolve(
                MotherBrainForm.ThirdPhase, MotherBrainProjectileType.Beam, credit);
            int hyperRemainder = ((start - MotherBrainWalkCredit.HyperBeamShotCost) % WordRange + WordRange) % WordRange;
            bool overdrawn = hyperRemainder >= SignedWordLimit;
            AssertEqual(overdrawn ? 0 : hyperRemainder, hyper.WalkCredit.Word, $"final-phase beam from credit {start}");
            AssertEqual(overdrawn, hyper.HyperBeamRecoil, $"final-phase beam from credit {start} recoils only on overdraw");

            MotherBrainShotReactionResult beam = MotherBrainShotReaction.Resolve(
                MotherBrainForm.SecondPhase, MotherBrainProjectileType.Beam, credit);
            AssertEqual(NativeSignedSpend(start, MotherBrainWalkCredit.StepCredit), beam.WalkCredit.Word,
                $"second-phase beam from credit {start}");
            AssertEqual(false, beam.HyperBeamRecoil, $"second-phase beam from credit {start} never recoils");

            MotherBrainShotReactionResult missile = MotherBrainShotReaction.Resolve(
                MotherBrainForm.ThirdPhase, MotherBrainProjectileType.Missile, credit);
            AssertEqual(MotherBrainWalkCredit.Empty, missile.WalkCredit, $"missile from credit {start} empties the credit");

            MotherBrainShotReactionResult powerBomb = MotherBrainShotReaction.Resolve(
                MotherBrainForm.SecondPhase, MotherBrainProjectileType.PowerBomb, credit);
            AssertEqual(NativeSignedSpend(start, MotherBrainWalkCredit.StepCredit), powerBomb.WalkCredit.Word,
                $"power bomb from credit {start} takes the wrapped reaction-zero branch");
        }
    }

    /// <summary>Every composition of the three parts round-trips, and a word with any other bit is rejected.</summary>
    private static void VerifyHitboxPartsCodec()
    {
        MotherBrainHitboxParts[] parts = [MotherBrainHitboxParts.Body, MotherBrainHitboxParts.Brain, MotherBrainHitboxParts.Neck];
        for (int mask = 0; mask < 8; mask++)
        {
            MotherBrainHitboxParts composed = MotherBrainHitboxParts.None;
            for (int bit = 0; bit < parts.Length; bit++)
                if ((mask & (1 << bit)) != 0)
                    composed |= parts[bit];
            ushort word = composed.ToNativeWord();
            AssertEqual(mask, word, $"hitbox parts {composed} encode body, brain and neck as the three low bits");
            AssertEqual(composed, MotherBrainHitboxPartsCodec.FromNativeWord(word), $"hitbox parts {composed} round trip");
        }

        AssertEqual(MotherBrainHitboxParts.Brain, MotherBrainHitboxPartsCodec.FromNativeWord(2),
            "the room-entry word enables the brain alone");
        AssertEqual(MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck,
            MotherBrainHitboxPartsCodec.FromNativeWord(7), "the ascent word enables every part");
        foreach (ushort undefined in new ushort[] { 8, 15, 256, 32775 })
        {
            AssertThrows<InvalidDataException>(() => MotherBrainHitboxPartsCodec.FromNativeWord(undefined),
                $"hitbox-enable word {undefined} with a bit beyond the neck is rejected");
        }
    }

    /// <summary>The production dispatcher moves both neck angles with full-turn wraparound and unsigned limits.</summary>
    private static void VerifyNeckAngleWrapThroughDispatcher()
    {
        // Bobbing down from just above zero wraps past a full turn, which compares above the floor, so it keeps going.
        SnesAngle lower = SnesAngle.FromRaw(100);
        SnesAngle upper = SnesAngle.FromRaw(20480);
        MotherBrainNeckMovement lowerMovement = MotherBrainNeckMovement.BobDown;
        MotherBrainNeckMovement upperMovement = MotherBrainNeckMovement.None;
        MotherBrainNeckKinematics.StepAngles(ref lower, ref upper, ref lowerMovement, ref upperMovement,
            angleDelta: 1280, brainY: 100, samusY: 100);
        AssertEqual(((100 - 1280) % WordRange + WordRange) % WordRange, lower.RawValue,
            "lower neck bobbing down wraps through a full turn");
        AssertEqual(MotherBrainNeckMovement.BobDown, lowerMovement, "a wrapped lower angle is above the bob floor");
        AssertEqual(20480, upper.RawValue, "an idle upper neck does not move");

        // The upper neck's raised limit leads the lower angle; near a full turn that target wraps below the candidate.
        lower = SnesAngle.FromRaw(64000);
        upper = SnesAngle.FromRaw(65000);
        lowerMovement = MotherBrainNeckMovement.None;
        upperMovement = MotherBrainNeckMovement.BobUp;
        MotherBrainNeckKinematics.StepAngles(ref lower, ref upper, ref lowerMovement, ref upperMovement,
            angleDelta: 1000, brainY: 100, samusY: 100);
        int target = NativeAdd(64000, 2048);
        int candidate = NativeAdd(65000, 1000);
        AssertEqual(candidate >= target ? target : candidate, upper.RawValue,
            "upper neck bobbing up wraps its angle and its lower-led limit");
        AssertEqual(candidate >= target ? MotherBrainNeckMovement.BobDown : MotherBrainNeckMovement.BobUp, upperMovement,
            "upper neck reverses only when the wrapped candidate reaches the wrapped limit");
        AssertEqual(64000, lower.RawValue, "an idle lower neck does not move");
    }

    /// <summary>The snapshot import accepts every defined word and rejects words outside each domain.</summary>
    private static void VerifySnapshotDecodesRejectUndefinedWords()
    {
        foreach (MotherBrainForm form in Enum.GetValues<MotherBrainForm>())
            AssertEqual(form, NativeSnapshotImportMotherBrainVerification.DecodeForm((ushort)form), $"form {form} decodes");
        AssertThrows<InvalidDataException>(() => NativeSnapshotImportMotherBrainVerification.DecodeForm(5),
            "a form word past the final phase is rejected");

        foreach (MotherBrainPhase2CorpseState corpse in Enum.GetValues<MotherBrainPhase2CorpseState>())
        {
            AssertEqual(corpse, NativeSnapshotImportMotherBrainVerification.DecodePhase2CorpseState((ushort)corpse),
                $"corpse state {corpse} decodes");
        }
        AssertThrows<InvalidDataException>(() => NativeSnapshotImportMotherBrainVerification.DecodePhase2CorpseState(3),
            "a corpse-handshake word past revived is rejected");

        AssertEqual(0, NativeSnapshotImportMotherBrainVerification.DecodeLegTilePage(0),
            "a leg-transfer list that is not running starts from the first page");
        for (int page = 0; page < MotherBrainLegTileTransferDefinitions.PageCount; page++)
        {
            ushort pointer = checked((ushort)(MotherBrainLegTileTransferDefinitions.NativeListPointer +
                page * MotherBrainLegTileTransferDefinitions.RecordByteCount));
            AssertEqual(page, NativeSnapshotImportMotherBrainVerification.DecodeLegTilePage(pointer),
                $"leg-transfer record {page} decodes to its page");
        }
        AssertThrows<InvalidDataException>(() => NativeSnapshotImportMotherBrainVerification.DecodeLegTilePage(
                checked((ushort)(MotherBrainLegTileTransferDefinitions.NativeListPointer +
                    MotherBrainLegTileTransferDefinitions.PageCount * MotherBrainLegTileTransferDefinitions.RecordByteCount))),
            "the leg-transfer terminator is not a page");

        AssertEqual(SnesObjPalettes.Index7, SnesObjAttributeWord.FromPaletteBits(SnesObjPalettes.Index7.PaletteBits),
            "a palette-only word decodes to its palette selection");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SnesObjAttributeWord.FromPaletteBits(checked((ushort)(SnesObjPalettes.Index1.PaletteBits + 1))),
            "a palette word carrying tile bits is rejected");
    }

    /// <summary>Runs the focused Mother Brain typed-domain checks.</summary>
    [VerificationCommand("--mother-brain-encoding-domains")]
    internal static int MotherBrainEncodingDomainsCommand()
    {
        VerifyMotherBrainEncodingDomains();
        return 0;
    }
}
