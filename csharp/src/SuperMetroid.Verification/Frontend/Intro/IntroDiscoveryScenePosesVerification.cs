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
/// Reproduces #1645: the intro's discovery actors are pictured only in the poses the discovery scene draws them in. The
/// scene's setup spawns two actors, the Metroid egg (definition $8B:CE5B, spawned at $8B:AFD0) and the confused baby
/// Metroid (definition $8B:CE79, spawned at $8B:AFD6), and each draws only the spritemaps its own instruction lists show.
/// <para>
/// The spritemaps are read here from the cartridge alone: an actor's lists run from the list its definition names up to the
/// list the next definition names, the egg's from $8B:CB33 to the delivered baby's at $8B:CB9F and the confused baby's from
/// $8B:CC2B to $8B:CC3F. The list at $8B:CC3F is not the baby's: it is the list of definition $8B:CE7F, the Ceres station's
/// large asteroids, which only the Ceres destruction's setup spawns ($8B:C27C), and it shows spritemap $8C:909D over the
/// Ceres scene's characters. Composed over the intro's characters, that spritemap is a spread of unrelated fragments.
/// </para>
/// <para>
/// Every installed sprite sheet is then held to it: no sheet holds a discovery actor frame whose spritemap neither actor's
/// lists show, and the intro's sheet holds a discovery actor frame for every spritemap they do show.
/// </para>
/// </summary>
internal static class IntroDiscoveryScenePosesVerification
{
    /// <summary>The bank of the cinematic sprite definitions and their instruction lists.</summary>
    private const int ListBank = 0x8B;

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

    /// <summary>The actors the discovery scene's setup spawns, each with the definition after it, whose list ends its own.</summary>
    private static readonly DiscoveryActor[] Actors =
    [
        new("the Metroid egg", 0xCE5B, 0xCE61),
        new("the confused baby Metroid", 0xCE79, 0xCE7F),
    ];

    /// <summary>The bytes each instruction the actors' lists hold takes, its own word included.</summary>
    private static readonly Dictionary<ushort, int> InstructionBytes = InstructionByteCounts();

    /// <summary>
    /// Fails, naming every installed discovery actor frame whose spritemap the discovery scene's actors never show, and every
    /// spritemap they show that the intro's sheet holds no discovery actor frame for.
    /// </summary>
    internal static void VerifyIntroDiscoveryScenePoses(IImportCartridgeSource cartridge)
    {
        Ensure.NotNull(cartridge);
        var shown = new HashSet<int>();
        var ranges = new List<string>();
        foreach (DiscoveryActor actor in Actors)
        {
            int first = ListOf(cartridge, actor.Definition);
            int end = ListOf(cartridge, actor.NextDefinition);
            shown.UnionWith(ShownSpritemaps(cartridge, actor, first, end));
            ranges.Add($"{actor.Name}'s from $8B:{first:X4} to $8B:{end:X4}");
        }
        AssertTrue(shown.Count > 0, "the discovery scene's actors show spritemaps");

        Dictionary<string, int> discoveryFrames = DiscoveryFrameSpritemaps();
        var failures = new List<string>();
        foreach (SpriteSheetId id in SpriteSheetDefinitions.Sheets)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
            foreach (string name in sheet.frames.Keys.Concat(sheet.blanks).Order(StringComparer.Ordinal))
            {
                if (discoveryFrames.TryGetValue(name, out int spritemap) && !shown.Contains(spritemap))
                    failures.Add($"  {SpriteSheetDefinitions.FileNames[id]} holds {name}, which shows spritemap $8C:{spritemap:X4}; neither discovery actor's lists show it");
            }
        }
        SpriteSheet intro = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.IntroSprites);
        foreach (int spritemap in shown.Order())
        {
            bool held = discoveryFrames.Any(frame => frame.Value == spritemap &&
                (intro.frames.ContainsKey(frame.Key) || intro.blanks.Contains(frame.Key)));
            if (!held)
                failures.Add($"  {SpriteSheetDefinitions.FileNames[SpriteSheetId.IntroSprites]} holds no discovery actor frame for spritemap $8C:{spritemap:X4}, which the discovery scene shows");
        }
        AssertTrue(failures.Count == 0,
            $"{failures.Count} installed discovery actor frames disagree with the poses the discovery scene's actors show ({string.Join("; ", ranges)}):{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Intro discovery scene poses: the installed sheets hold a discovery actor frame for each of the {shown.Count} spritemaps the egg and the confused baby show, and for no other.");
    }

    /// <summary>The instruction list the cinematic sprite definition at <paramref name="definition"/> starts its actor on.</summary>
    private static int ListOf(IImportCartridgeSource cartridge, int definition) =>
        Word(cartridge, definition + DefinitionListOffset);

    /// <summary>
    /// The spritemaps the frame records of <paramref name="actor"/>'s lists show, from <paramref name="first"/> up to
    /// <paramref name="end"/>, read record by record so that each instruction's operand is stepped over, never read as a frame.
    /// </summary>
    /// <exception cref="InvalidDataException">The lists hold an instruction whose length is unknown, or run past their end.</exception>
    private static HashSet<int> ShownSpritemaps(IImportCartridgeSource cartridge, DiscoveryActor actor, int first, int end)
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
    /// The spritemap each discovery actor frame name shows: the frame of each pose that shows a cinematic spritemap, as the
    /// sheets name it for the discovery actors' family.
    /// </summary>
    private static Dictionary<string, int> DiscoveryFrameSpritemaps()
    {
        var frames = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int index = 0; index < CinematicSpritemapSourceDefinitions.Spritemaps.Length; index++)
        {
            if (CinematicSpritemapSourceDefinitions.Spritemaps[index] is SnesAddress spritemap)
                frames.Add(CinematicSpriteSheet.FrameName(CinematicSpriteFamily.IntroDiscoveryActor, new CinematicSpritePose(index)), spritemap.Offset);
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

    /// <summary>Runs the installed discovery actor frames against the poses the discovery scene's actors show (#1645).</summary>
    [VerificationCommand("--intro-discovery-scene-poses")]
    internal static int IntroDiscoveryScenePosesCommand()
    {
        Suite(nameof(VerifyIntroDiscoveryScenePoses), () => VerifyIntroDiscoveryScenePoses(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>An actor the discovery scene's setup spawns.</summary>
    /// <param name="Name">The actor, as a failure names it.</param>
    /// <param name="Definition">Where in the lists' bank the actor's cinematic sprite definition is.</param>
    /// <param name="NextDefinition">The definition after it, whose instruction list ends the actor's own.</param>
    private readonly record struct DiscoveryActor(string Name, int Definition, int NextDefinition);
}
