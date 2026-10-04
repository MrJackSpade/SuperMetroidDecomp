using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable OAM compositions for the 37 post-credits Samus reward frames.</summary>
public sealed class EndingRewardSpritePresentation : IIntroCinematicSpritePresentation
{
    private readonly Dictionary<ushort, SpriteComposition> frames;

    private EndingRewardSpritePresentation(Dictionary<ushort, SpriteComposition> frames) =>
        this.frames = frames;

    /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromCompositions(nameof(EndingRewardSpritePresentation), frames);

    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y,
        ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pointer, out SpriteComposition? frame))
            throw new InvalidDataException(
                $"Ending reward sprite frame $8C:{pointer:X4} is not installed.");
        if (originIsOnScreen)
            frame.DrawOnScreen(oam, x, y, paletteBits);
        else
            frame.DrawOffScreen(oam, x, y, paletteBits);
    }

    public static EndingRewardSpritePresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        EndingRewardSpriteDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EndingRewardSpriteDocument>(
                MapPresentationFormat.JsonOptions) ??
                throw new InvalidDataException("Ending reward sprite JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid ending reward sprite JSON.", error);
        }
        IReadOnlyList<EndingRewardSpriteFrameDefinition> definitions =
            EndingRewardSpriteDefinitions.Frames;
        if (document.Version != EndingRewardSpriteFormat.Version ||
            document.Frames is null || document.Frames.Count != definitions.Count)
            throw new InvalidDataException(
                $"Ending reward requires exactly {definitions.Count} named visual frames.");
        var frames = new Dictionary<ushort, SpriteComposition>();
        foreach (EndingRewardSpriteFrameDefinition definition in definitions)
        {
            if (!document.Frames.TryGetValue(definition.Name, out SpriteVisualPart[]? visual) ||
                visual is null)
                throw new InvalidDataException(
                    $"Ending reward frame {definition.Name} is missing.");
            SpriteComposition composition = IntroCinematicSpriteCompiler.Compile(visual, definition.Name);
            composition = EndingRewardHeadParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardStandingParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardPrepareJumpParts.CalculateIfMatching(definition.Pointer, composition);
            composition = EndingRewardJumpParts.CalculateIfMatching(definition.Pointer, composition);
            frames.Add(definition.Pointer, composition);
        }
        return new EndingRewardSpritePresentation(frames);
    }

    public static void Write(Stream json, EndingRewardSpriteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record EndingRewardSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}

/// <summary>
/// Cartridge OAM record identities selected by the suitless and suited reward
/// lists at $8B:ED1D..EE5C. Counts describe visuals, not actor timing.
/// </summary>
public static class EndingRewardSpriteDefinitions
{
    /// <summary>$8C:99D6, LargeSamusFromEndingStanding, first of37 consecutive reward OAM records.</summary>
    private const ushort FirstRecord = 0x99d6;
    private const int FrameCount = 37;
    public static IReadOnlyList<EndingRewardSpriteFrameDefinition> Frames { get; } = new FrameView();

    internal static ushort FramePointer(EndingRewardSpriteFrame frame)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        int address = FirstRecord;
        for (int preceding = 0; preceding < (int)frame; preceding++)
            address += sizeof(ushort) + 5 * Parts((EndingRewardSpriteFrame)preceding);
        return (ushort)address;
    }

    private static int Parts(EndingRewardSpriteFrame frame) => frame switch
    {
        EndingRewardSpriteFrame.SamusHeadFromEndingFrame1 or
            EndingRewardSpriteFrame.SamusHeadFromEndingFrame2 or
            EndingRewardSpriteFrame.SamusHeadFromEndingFrame3 or
            EndingRewardSpriteFrame.SamusHeadFromEndingFrame4 => 2,
        EndingRewardSpriteFrame.JumpingSamusHeadFromEnding => 3,
        EndingRewardSpriteFrame.SamusHeadWithHelmetFromEnding => 4,
        EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame1 or
            EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame2 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame1 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame3 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame4 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame5 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame6 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame7 or
            EndingRewardSpriteFrame.SamusArmFromEndingFrame8 => 5,
        EndingRewardSpriteFrame.SamusArmFromEndingFrame2 => 6,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame1 or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame5 => 9,
        EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame2 or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame3 or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame4 or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame6 => 10,
        EndingRewardSpriteFrame.SamusLanding or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame8 => 13,
        EndingRewardSpriteFrame.SuitlessSamusLowerBody => 14,
        EndingRewardSpriteFrame.SamusFalling or
            EndingRewardSpriteFrame.SamusShooting or
            EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame7 => 15,
        EndingRewardSpriteFrame.SuitlessSamusJumping => 19,
        EndingRewardSpriteFrame.LargeSamusFromEndingJumping or
            EndingRewardSpriteFrame.SuitlessSamusPreparingToJump => 20,
        EndingRewardSpriteFrame.SamusLanded => 21,
        EndingRewardSpriteFrame.LargeSamusFromEndingPreparingToJump => 22,
        EndingRewardSpriteFrame.SuitlessSamusStanding or
            EndingRewardSpriteFrame.SuitlessSamusStandingArmsStraight => 28,
        EndingRewardSpriteFrame.HeadlessArmlessSuitedSamus => 30,
        EndingRewardSpriteFrame.LargeSamusFromEndingStanding => 34,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    private static EndingRewardSpriteFrameDefinition Get(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index is >= 2 and <= 9)
            return Define("suitless-hair-" + (index - 2 + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                (EndingRewardSpriteFrame)((int)EndingRewardSpriteFrame.SuitlessSamusOpeningHairFrame1 + index - 2));
        if (index is >= 21 and <= 28)
            return Define("suited-arm-" + (index - 21 + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                (EndingRewardSpriteFrame)((int)EndingRewardSpriteFrame.SamusArmFromEndingFrame1 + index - 21));
        if (index is >= 29 and <= 31)
            return Define("helmetless-head-" + (index - 29 + 2).ToString(System.Globalization.CultureInfo.InvariantCulture),
                (EndingRewardSpriteFrame)((int)EndingRewardSpriteFrame.SamusHeadFromEndingFrame2 + index - 29));
        return (AssetRole)index switch
        {
            AssetRole.SuitlessIdleUpper => Define("suitless-idle-upper", EndingRewardSpriteFrame.SuitlessSamusStandingArmsStraight),
            AssetRole.SuitlessLower => Define("suitless-lower", EndingRewardSpriteFrame.SuitlessSamusLowerBody),
            AssetRole.SuitlessStanding => Define("suitless-standing", EndingRewardSpriteFrame.SuitlessSamusStanding),
            AssetRole.SuitlessPrepareJump => Define("suitless-prepare-jump", EndingRewardSpriteFrame.SuitlessSamusPreparingToJump),
            AssetRole.SuitlessJumping => Define("suitless-jumping", EndingRewardSpriteFrame.SuitlessSamusJumping),
            AssetRole.SamusFalling => Define("samus-falling", EndingRewardSpriteFrame.SamusFalling),
            AssetRole.SamusLanding => Define("samus-landing", EndingRewardSpriteFrame.SamusLanding),
            AssetRole.SamusLanded => Define("samus-landed", EndingRewardSpriteFrame.SamusLanded),
            AssetRole.SamusShooting => Define("samus-shooting", EndingRewardSpriteFrame.SamusShooting),
            AssetRole.SuitedIdleBody => Define("suited-idle-body", EndingRewardSpriteFrame.LargeSamusFromEndingStanding),
            AssetRole.SuitedHelmetHead => Define("suited-helmet-head", EndingRewardSpriteFrame.SamusHeadWithHelmetFromEnding),
            AssetRole.HelmetlessHead1 => Define("helmetless-head-1", EndingRewardSpriteFrame.SamusHeadFromEndingFrame1),
            AssetRole.SuitedHeadlessBody => Define("suited-headless-body", EndingRewardSpriteFrame.HeadlessArmlessSuitedSamus),
            AssetRole.SuitedPrepareJump => Define("suited-prepare-jump", EndingRewardSpriteFrame.LargeSamusFromEndingPreparingToJump),
            AssetRole.SuitedJumping => Define("suited-jumping", EndingRewardSpriteFrame.LargeSamusFromEndingJumping),
            AssetRole.SuitedHelmetJumpHead1 => Define("suited-helmet-jump-head-1", EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame1),
            AssetRole.SuitedHelmetJumpHead2 => Define("suited-helmet-jump-head-2", EndingRewardSpriteFrame.LargeSamusHelmetFromEndingFrame2),
            AssetRole.HelmetlessJumpHead => Define("helmetless-jump-head", EndingRewardSpriteFrame.JumpingSamusHeadFromEnding),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    private enum AssetRole
    {
        SuitlessIdleUpper = 0,
        SuitlessLower = 1,
        SuitlessStanding = 10,
        SuitlessPrepareJump = 11,
        SuitlessJumping = 12,
        SamusFalling = 13,
        SamusLanding = 14,
        SamusLanded = 15,
        SamusShooting = 16,
        SuitedIdleBody = 17,
        SuitedHelmetHead = 18,
        HelmetlessHead1 = 19,
        SuitedHeadlessBody = 20,
        SuitedPrepareJump = 32,
        SuitedJumping = 33,
        SuitedHelmetJumpHead1 = 34,
        SuitedHelmetJumpHead2 = 35,
        HelmetlessJumpHead = 36,
    }

    private static EndingRewardSpriteFrameDefinition Define(string name, EndingRewardSpriteFrame frame) =>
        new(name, FramePointer(frame), Parts(frame));

    private sealed class FrameView : IReadOnlyList<EndingRewardSpriteFrameDefinition>
    {
        public int Count => FrameCount;
        public EndingRewardSpriteFrameDefinition this[int index] => Get(index);
        public IEnumerator<EndingRewardSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return Get(index);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

}

public readonly record struct EndingRewardSpriteFrameDefinition(
    string Name, ushort Pointer, int StockPartCount);

public static class EndingRewardSpriteFormat
{
    public const int Version = 1;
    public const string FileName = "ending-reward-sprites.json";
}

/// <summary>Reward visual poses in native OAM storage order, distinct from published asset order.</summary>
internal enum EndingRewardSpriteFrame
{
    /// <summary>$8C:99D6, EndingSequenceSpritemaps_LargeSamusFromEndingStanding.</summary>
    LargeSamusFromEndingStanding,
    /// <summary>$8C:9A82, EndingSequenceSpritemaps_LargeSamusFromEndingPreparingToJump.</summary>
    LargeSamusFromEndingPreparingToJump,
    /// <summary>$8C:9AF2, EndingSequenceSpritemaps_LargeSamusFromEndingJumping.</summary>
    LargeSamusFromEndingJumping,
    /// <summary>$8C:9B58, EndingSequenceSpritemaps_LargeSamusHelmetFromEndingFrame1.</summary>
    LargeSamusHelmetFromEndingFrame1,
    /// <summary>$8C:9B73, EndingSequenceSpritemaps_LargeSamusHelmetFromEndingFrame2.</summary>
    LargeSamusHelmetFromEndingFrame2,
    /// <summary>$8C:9B8E, EndingSequenceSpritemaps_JumpingSamusHeadFromEnding.</summary>
    JumpingSamusHeadFromEnding,
    /// <summary>$8C:9B9F, EndingSequenceSpritemaps_SamusArmFromEndingFrame1.</summary>
    SamusArmFromEndingFrame1,
    /// <summary>$8C:9BBA, EndingSequenceSpritemaps_SamusArmFromEndingFrame2.</summary>
    SamusArmFromEndingFrame2,
    /// <summary>$8C:9BDA, EndingSequenceSpritemaps_SamusArmFromEndingFrame3.</summary>
    SamusArmFromEndingFrame3,
    /// <summary>$8C:9BF5, EndingSequenceSpritemaps_SamusArmFromEndingFrame4.</summary>
    SamusArmFromEndingFrame4,
    /// <summary>$8C:9C10, EndingSequenceSpritemaps_SamusArmFromEndingFrame5.</summary>
    SamusArmFromEndingFrame5,
    /// <summary>$8C:9C2B, EndingSequenceSpritemaps_SamusArmFromEndingFrame6.</summary>
    SamusArmFromEndingFrame6,
    /// <summary>$8C:9C46, EndingSequenceSpritemaps_SamusArmFromEndingFrame7.</summary>
    SamusArmFromEndingFrame7,
    /// <summary>$8C:9C61, EndingSequenceSpritemaps_SamusArmFromEndingFrame8.</summary>
    SamusArmFromEndingFrame8,
    /// <summary>$8C:9C7C, EndingSequenceSpritemaps_SamusHeadFromEndingFrame1.</summary>
    SamusHeadFromEndingFrame1,
    /// <summary>$8C:9C88, EndingSequenceSpritemaps_SamusHeadFromEndingFrame2.</summary>
    SamusHeadFromEndingFrame2,
    /// <summary>$8C:9C94, EndingSequenceSpritemaps_SamusHeadFromEndingFrame3.</summary>
    SamusHeadFromEndingFrame3,
    /// <summary>$8C:9CA0, EndingSequenceSpritemaps_SamusHeadFromEndingFrame4.</summary>
    SamusHeadFromEndingFrame4,
    /// <summary>$8C:9CAC, EndingSequenceSpritemaps_SamusHeadWithHelmetFromEnding.</summary>
    SamusHeadWithHelmetFromEnding,
    /// <summary>$8C:9CC2, EndingSequenceSpritemaps_HeadlessArmlessSuitedSamus.</summary>
    HeadlessArmlessSuitedSamus,
    /// <summary>$8C:9D5A, EndingSequenceSpritemaps_SamusFalling.</summary>
    SamusFalling,
    /// <summary>$8C:9DA7, EndingSequenceSpritemaps_SamusLanding.</summary>
    SamusLanding,
    /// <summary>$8C:9DEA, EndingSequenceSpritemaps_SamusLanded.</summary>
    SamusLanded,
    /// <summary>$8C:9E55, EndingSequenceSpritemaps_SamusShooting.</summary>
    SamusShooting,
    /// <summary>$8C:9EA2, EndingSequenceSpritemaps_SuitlessSamusStanding.</summary>
    SuitlessSamusStanding,
    /// <summary>$8C:9F30, EndingSequenceSpritemaps_SuitlessSamusPreparingToJump.</summary>
    SuitlessSamusPreparingToJump,
    /// <summary>$8C:9F96, EndingSequenceSpritemaps_SuitlessSamusJumping.</summary>
    SuitlessSamusJumping,
    /// <summary>$8C:9FF7, EndingSequenceSpritemaps_SuitlessSamusStandingArmsStraight.</summary>
    SuitlessSamusStandingArmsStraight,
    /// <summary>$8C:A085, EndingSequenceSpritemaps_SuitlessSamusOpeningHairFrame1.</summary>
    SuitlessSamusOpeningHairFrame1,
    /// <summary>$8C:A0B4, EndingSequenceSpritemaps_SuitlessSamusOpeningHairFrame2.</summary>
    SuitlessSamusOpeningHairFrame2,
    /// <summary>$8C:A0E8, EndingSequenceSpritemaps_SuitlessSamusOpeningHairFrame3.</summary>
    SuitlessSamusOpeningHairFrame3,
    /// <summary>$8C:A11C, EndingSequenceSpritemaps_SuitlessSamusOpeningHairFrame4.</summary>
    SuitlessSamusOpeningHairFrame4,
    /// <summary>$8C:A150, EndingSequenceSpritemaps_SuitlessSamusOpeningHairFrame5.</summary>
    SuitlessSamusOpeningHairFrame5,
    /// <summary>$8C:A17F, EndingSequenceSpritemaps_SuitlessSamusOpeningHairFrame6.</summary>
    SuitlessSamusOpeningHairFrame6,
    /// <summary>$8C:A1B3, EndingSequenceSpritemaps_SuitlessSamusOpeningHairFrame7.</summary>
    SuitlessSamusOpeningHairFrame7,
    /// <summary>$8C:A200, EndingSequenceSpritemaps_SuitlessSamusOpeningHairFrame8.</summary>
    SuitlessSamusOpeningHairFrame8,
    /// <summary>$8C:A243, EndingSequenceSpritemaps_SuitlessSamusLowerBody.</summary>
    SuitlessSamusLowerBody,
}
