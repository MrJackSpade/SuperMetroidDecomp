using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks cinematic text glow timing, slot reuse, and debugger restoration on the narration's shown text (#1467).</summary>
internal static class CinematicTextGlowVerification
{
    /// <summary>
    /// Tracks colour-variant steps at five-frame intervals over a rectangle of typed letters, which keep their letters and
    /// leave every other cell's variant alone, then the eight-slot limit.
    /// </summary>
    internal static void VerifyCinematicTextGlow()
    {
        var glow = new CinematicTextGlowSystem();
        IntroNarrationText text = LetteredText();
        glow.Spawn(new(1, 4), new(2, 2));
        CinematicTextGlowSystem? restored = null;
        IntroNarrationText? restoredText = null;
        for (int age = 0; age <= 20; age++)
        {
            glow.Step(text);
            text.Publish();
            if (restored is not null && restoredText is not null)
            {
                restored.Step(restoredText);
                restoredText.Publish();
            }
            for (int row = IntroNarrationText.FirstTextRow; row < IntroNarrationText.FirstTextRow + IntroNarrationText.TextRows; row++)
            {
                for (int column = 0; column < IntroNarrationText.RowCells; column++)
                {
                    bool rectangle = row is 4 or 5 && column is 1 or 2;
                    var expected = new IntroTextCell('A', rectangle ? Math.Min(age / 5, 3) : 0);
                    AssertEqual(expected, text.ShownAt(column, row), $"glow age {age} cell ({column}, {row}) keeps its letter and recolours only the rectangle");
                    if (restoredText is not null)
                        AssertEqual(expected, restoredText.ShownAt(column, row), "mid-glow debugger restore retains all timers and rectangle state");
                }
            }
            if (age == 7)
            {
                restored = RoundTrip(glow);
                restoredText = RoundTrip(text);
            }
        }
        // Eight simultaneous allocations fit; the ninth has no slot. After the final variant releases the slots, that same
        // rectangle can allocate.
        glow = new CinematicTextGlowSystem();
        text = LetteredText();
        for (int i = 0; i < 9; i++) glow.Spawn(new(checked((ushort)i), 4), new(1, 1));
        for (int frame = 0; frame < 16; frame++) glow.Step(text);
        text.Publish();
        for (int i = 0; i < 8; i++) AssertEqual(3, text.ShownAt(i, 4).ColourVariant, "eight native glow slots mature");
        AssertEqual(0, text.ShownAt(8, 4).ColourVariant, "full pool declines ninth glow");
        glow.Spawn(new(8, 4), new(1, 1));
        for (int frame = 0; frame < 16; frame++) glow.Step(text);
        text.Publish();
        AssertEqual(3, text.ShownAt(8, 4).ColourVariant, "finished glow frees a native slot");
        Console.WriteLine("  Cinematic text glow: exact 0/5/10/15-frame colour variants, rectangle preservation, pool exhaustion/reuse, and debugger continuation pass.");
    }

    /// <summary>Narration text showing the pages with the letter A typed into every text cell and shown.</summary>
    private static IntroNarrationText LetteredText()
    {
        var text = new IntroNarrationText();
        text.ShowPages();
        for (int row = IntroNarrationText.FirstTextRow; row < IntroNarrationText.FirstTextRow + IntroNarrationText.TextRows; row++)
        {
            for (int column = 0; column < IntroNarrationText.RowCells; column++)
                text.Type(new IntroNarrationCharacter(column, row, 'A'));
        }
        text.Publish();
        return text;
    }

    /// <summary><paramref name="value"/> saved and restored through the debugger's object-graph serializer.</summary>
    private static T RoundTrip<T>(T value) where T : class
    {
        using var stream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stream, value);
        stream.Position = 0;
        return DebuggerObjectGraphSerializer.Deserialize<T>(stream);
    }
}
