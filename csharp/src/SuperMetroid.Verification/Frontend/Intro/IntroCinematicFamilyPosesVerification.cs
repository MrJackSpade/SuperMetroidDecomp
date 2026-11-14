using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Reproduces #1645, #1654 and #1655: each of the intro's cinematic sprite families is pictured in exactly the poses its own actors
/// draw. The family's actors are the cinematic sprite definitions the intro spawns and draws through that family, and each
/// actor draws only the spritemaps its own instruction lists show.
/// <para>
/// The spritemaps are read here from the cartridge alone: an actor's lists run from the list its definition names up to the
/// list the next definition names. The definitions are those of bank $8B from $8B:CE55: the intro Mother Brain ($CE55),
/// the Metroid egg ($CE5B) and the confused baby Metroid ($CE79), the baby Metroid being delivered ($CE61) and being
/// examined ($CE67), the intro text caret ($CE6D), the Japanese text's next-page arrow ($CE73, lists $8B:CC0F to
/// $8B:CC2B, #1655), the six egg particles ($CECD to $CEEB) and the slime drops ($CEF1), the Mother Brain explosions, big
/// ($CF15) and small ($CF1B), and the intro Rinka ($CF21).
/// </para>
/// <para>
/// The list after the confused baby's, $8B:CC3F, is the Ceres station's large asteroids' ($CE7F), which only the Ceres
/// destruction draws, over its own characters (#1645).
/// </para>
/// <para>
/// Every installed sprite sheet is then held to it: no sheet holds a family's frame whose spritemap the family's actors
/// never show, and the intro's sheet holds, for each family, a frame for every spritemap the family's actors show, and no
/// frame of any family the intro does not draw.
/// </para>
/// </summary>
internal static class IntroCinematicFamilyPosesVerification
{
    /// <summary>The bank of the cinematic sprite definitions and their instruction lists.</summary>
    private const int ListBank = 0x8B;

    /// <summary>The bytes of a cinematic sprite definition: its initialization, its pre-instruction and its instruction list.</summary>
    private const int DefinitionBytes = 6;

    /// <summary>Where a cinematic sprite definition holds its instruction list, after its initialization and pre-instruction.</summary>
    private const int DefinitionListOffset = 4;

    /// <summary>The bytes of a frame record: its duration word, then its spritemap word.</summary>
    private const int FrameRecordBytes = 4;

    /// <summary>The bytes of an instruction that takes no operand.</summary>
    private const int BareInstructionBytes = 2;

    /// <summary>The bytes of an instruction that takes one operand word.</summary>
    private const int OperandInstructionBytes = 4;

    /// <summary>The spritemap word of a frame record that shows nothing.</summary>
    private const int NoSpritemap = 0;

    /// <summary>Each intro cinematic sprite family and the actors the intro draws through it.</summary>
    private static readonly FamilyActors[] Families =
    [
        new(CinematicSpriteFamily.IntroMotherBrain, [new("the intro Mother Brain", 0xCE55)]),
        new(CinematicSpriteFamily.IntroDiscoveryActor,
            [new("the Metroid egg", 0xCE5B), new("the confused baby Metroid", 0xCE79)]),
        new(CinematicSpriteFamily.IntroScientist,
            [new("the baby Metroid being delivered", 0xCE61), new("the baby Metroid being examined", 0xCE67)]),
        new(CinematicSpriteFamily.IntroCaret, [new("the intro text caret", 0xCE6D)]),
        new(CinematicSpriteFamily.IntroNextPageArrow, [new("the Japanese text's next-page arrow", 0xCE73)]),
        new(CinematicSpriteFamily.IntroEggEffect,
        [
            new("the Metroid egg's particle 1", 0xCECD),
            new("the Metroid egg's particle 2", 0xCED3),
            new("the Metroid egg's particle 3", 0xCED9),
            new("the Metroid egg's particle 4", 0xCEDF),
            new("the Metroid egg's particle 5", 0xCEE5),
            new("the Metroid egg's particle 6", 0xCEEB),
            new("the Metroid egg's slime drops", 0xCEF1),
        ]),
        new(CinematicSpriteFamily.IntroMotherBrainExplosion,
            [new("the big Mother Brain explosion", 0xCF15), new("the small Mother Brain explosion", 0xCF1B)]),
        new(CinematicSpriteFamily.IntroRinka, [new("the intro Rinka", 0xCF21)]),
    ];

    /// <summary>The bytes each instruction the actors' lists hold takes, its own word included.</summary>
    private static readonly Dictionary<ushort, int> InstructionBytes = InstructionByteCounts();

    /// <summary>
    /// Fails, naming every installed intro family frame whose spritemap the family's actors never show, every spritemap they
    /// show that the intro's sheet holds no frame of the family for, and every intro sheet frame of no family the intro draws.
    /// </summary>
    internal static void VerifyIntroCinematicFamilyPoses(IImportCartridgeSource cartridge)
    {
        Ensure.NotNull(cartridge);
        var shownByFamily = new Dictionary<CinematicSpriteFamily, HashSet<int>>();
        var ranges = new List<string>();
        foreach (FamilyActors family in Families)
        {
            var shown = new HashSet<int>();
            foreach (FamilyActor actor in family.Actors)
            {
                int first = ListOf(cartridge, actor.Definition);
                int end = ListOf(cartridge, actor.Definition + DefinitionBytes);
                shown.UnionWith(ShownSpritemaps(cartridge, actor, first, end));
                ranges.Add($"{actor.Name}'s from $8B:{first:X4} to $8B:{end:X4}");
            }
            AssertTrue(shown.Count > 0, $"the {family.Family} family's actors show spritemaps");
            shownByFamily.Add(family.Family, shown);
        }

        Dictionary<string, FamilyFrame> familyFrames = FamilyFrameSpritemaps();
        var failures = new List<string>();
        foreach (SpriteSheetId id in SpriteSheetDefinitions.Sheets)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
            foreach (string name in sheet.frames.Keys.Concat(sheet.blanks).Order(StringComparer.Ordinal))
            {
                if (familyFrames.TryGetValue(name, out FamilyFrame frame) && !shownByFamily[frame.Family].Contains(frame.Spritemap))
                    failures.Add($"  {SpriteSheetDefinitions.FileNames[id]} holds {name}, which shows spritemap $8C:{frame.Spritemap:X4}; no actor of the {frame.Family} family shows it");
            }
        }

        SpriteSheet intro = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.IntroSprites);
        string introFile = SpriteSheetDefinitions.FileNames[SpriteSheetId.IntroSprites];
        var introNames = new HashSet<string>(intro.frames.Keys.Concat(intro.blanks), StringComparer.Ordinal);
        foreach (string name in introNames.Order(StringComparer.Ordinal))
        {
            if (!familyFrames.ContainsKey(name))
                failures.Add($"  {introFile} holds {name}, a frame of no intro family's pose that shows a spritemap");
        }
        foreach ((CinematicSpriteFamily family, HashSet<int> shown) in shownByFamily)
        {
            foreach (int spritemap in shown.Order())
            {
                bool held = familyFrames.Any(frame => frame.Value.Family == family && frame.Value.Spritemap == spritemap &&
                    introNames.Contains(frame.Key));
                if (!held)
                    failures.Add($"  {introFile} holds no {family} frame for spritemap $8C:{spritemap:X4}, which the family's actors show");
            }
        }
        AssertTrue(failures.Count == 0,
            $"{failures.Count} installed intro family frames disagree with the poses the families' actors show ({string.Join("; ", ranges)}):{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Intro cinematic family poses: the installed sheets hold, for each of the {Families.Length} intro families, a frame for each spritemap its actors show, and for no other.");
    }

    /// <summary>The instruction list the cinematic sprite definition at <paramref name="definition"/> starts its actor on.</summary>
    private static int ListOf(IImportCartridgeSource cartridge, int definition) =>
        Word(cartridge, definition + DefinitionListOffset);

    /// <summary>
    /// The spritemaps the frame records of <paramref name="actor"/>'s lists show, from <paramref name="first"/> up to
    /// <paramref name="end"/>, read record by record so that each instruction's operand is stepped over, never read as a frame.
    /// </summary>
    /// <exception cref="InvalidDataException">The lists hold an instruction whose length is unknown, or run past their end.</exception>
    private static HashSet<int> ShownSpritemaps(IImportCartridgeSource cartridge, FamilyActor actor, int first, int end)
    {
        AssertTrue(first < end, $"{actor.Name}'s lists at $8B:{first:X4} start before the next definition's at $8B:{end:X4}");
        var shown = new HashSet<int>();
        int cursor = first;
        while (cursor < end)
        {
            ushort word = Word(cartridge, cursor);
            if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
            {
                ushort spritemap = Word(cartridge, cursor + sizeof(ushort));
                if (spritemap != NoSpritemap)
                    shown.Add(spritemap);
                cursor += FrameRecordBytes;
                continue;
            }
            cursor += InstructionBytes.TryGetValue(word, out int bytes) ? bytes
                : throw new InvalidDataException($"{actor.Name}'s list holds instruction ${word:X4} at $8B:{cursor:X4}, whose length is unknown.");
        }
        AssertEqual(end, cursor, $"{actor.Name}'s lists end exactly where the next definition's list starts");
        return shown;
    }

    /// <summary>
    /// The family and spritemap of each intro family frame name: every intro family's frame of each pose that shows a
    /// cinematic spritemap, as the sheets name it.
    /// </summary>
    private static Dictionary<string, FamilyFrame> FamilyFrameSpritemaps()
    {
        var frames = new Dictionary<string, FamilyFrame>(StringComparer.Ordinal);
        foreach (FamilyActors family in Families)
        {
            foreach (CinematicSpritemapSource source in CinematicSpritemapSourceDefinitions.Spritemaps)
            {
                if (source.Spritemap is SnesAddress spritemap)
                    frames.Add(CinematicSpriteSheet.FrameName(family.Family, source.Pose), new(family.Family, spritemap.Offset));
            }
        }
        return frames;
    }

    /// <summary>
    /// The length of each instruction a cinematic sprite list can hold: the owner instructions and the deletion and sleep
    /// take no operand, and the pre-instruction install, the goto, the counted goto and the timer load take one.
    /// </summary>
    private static Dictionary<ushort, int> InstructionByteCounts()
    {
        var counts = CinematicSpriteNativeListAddresses.OwnerInstructions.Keys.ToDictionary(word => word, _ => BareInstructionBytes);
        counts.Add((ushort)CinematicSpriteInstruction.Delete, BareInstructionBytes);
        counts.Add((ushort)CinematicSpriteInstruction.Sleep, BareInstructionBytes);
        counts.Add((ushort)CinematicSpriteInstruction.SetFrameBehavior, OperandInstructionBytes);
        counts.Add((ushort)CinematicSpriteInstruction.Goto, OperandInstructionBytes);
        counts.Add((ushort)CinematicSpriteInstruction.DecrementTimerAndGoto, OperandInstructionBytes);
        counts.Add((ushort)CinematicSpriteInstruction.SetTimer, OperandInstructionBytes);
        return counts;
    }

    /// <summary>The cartridge word at <paramref name="offset"/> in the lists' bank.</summary>
    private static ushort Word(IImportCartridgeSource cartridge, int offset) =>
        cartridge.ReadCartridgeWordFixedBank((int)SnesAddress.InBank(ListBank, offset));

    /// <summary>Runs the installed intro family frames against the poses each family's actors show (#1645, #1654, #1655).</summary>
    [VerificationCommand("--intro-cinematic-family-poses")]
    internal static int IntroCinematicFamilyPosesCommand()
    {
        Suite(nameof(VerifyIntroCinematicFamilyPoses), () => VerifyIntroCinematicFamilyPoses(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>An intro cinematic sprite family and the actors the intro draws through it.</summary>
    /// <param name="Family">The family.</param>
    /// <param name="Actors">The actors whose lists show the family's poses.</param>
    private sealed record FamilyActors(CinematicSpriteFamily Family, FamilyActor[] Actors);

    /// <summary>An actor the intro draws through a family.</summary>
    /// <param name="Name">The actor, as a failure names it.</param>
    /// <param name="Definition">Where in the lists' bank the actor's cinematic sprite definition is; the next definition's list ends its own.</param>
    private readonly record struct FamilyActor(string Name, int Definition);

    /// <summary>A family's frame of a pose and the spritemap that pose shows.</summary>
    /// <param name="Family">The family the frame is named for.</param>
    /// <param name="Spritemap">The bank-$8C spritemap the frame's pose shows.</param>
    private readonly record struct FamilyFrame(CinematicSpriteFamily Family, int Spritemap);
}
