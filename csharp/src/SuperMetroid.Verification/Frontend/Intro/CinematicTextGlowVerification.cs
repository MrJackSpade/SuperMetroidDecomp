using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks cinematic text glow timing, slot reuse, and debugger restoration on the narration's text (#1467, #1555).</summary>
internal static class CinematicTextGlowVerification
{
    /// <summary>
    /// Tracks colour-variant steps at five-frame intervals over a square of four glowing typed letters, which keep their
    /// letters and leave every other letter's variant alone, then the eight-slot limit.
    /// </summary>
    internal static void VerifyCinematicTextGlow()
    {
        var glow = new CinematicTextGlowSystem();
        IntroNarrationText text = LetteredText();
        glow.Spawn(1, 4);
        glow.Spawn(2, 4);
        glow.Spawn(1, 5);
        glow.Spawn(2, 5);
        CinematicTextGlowSystem? restored = null;
        IntroNarrationText? restoredText = null;
        for (int age = 0; age <= 20; age++)
        {
            glow.Step(text);
            if (restored is not null && restoredText is not null)
                restored.Step(restoredText);
            for (int row = IntroNarrationText.FirstTextRow; row < IntroNarrationText.FirstTextRow + IntroNarrationText.TextRows; row++)
            {
                for (int column = 0; column < IntroNarrationText.RowCells; column++)
                {
                    bool rectangle = row is 4 or 5 && column is 1 or 2;
                    var expected = new IntroTextCell('A', rectangle ? Math.Min(age / 5, 3) : 0);
                    AssertEqual(expected, text.ShownAt(column, row), $"glow age {age} letter ({column}, {row}) keeps its letter and recolours only the glowing square");
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
        // Every glow started runs, however many are running at once (#1625): a whole row of simultaneous glows all mature,
        // and a letter glows again after its earlier glow has ended.
        glow = new CinematicTextGlowSystem();
        text = LetteredText();
        for (int i = 0; i < IntroNarrationText.RowCells; i++) glow.Spawn(i, 4);
        for (int frame = 0; frame < 16; frame++) glow.Step(text);
        for (int i = 0; i < IntroNarrationText.RowCells; i++)
            AssertEqual(3, text.ShownAt(i, 4).ColourVariant, $"simultaneous glow {i} of {IntroNarrationText.RowCells} matures");
        text.Recolour(8, 4, 0);
        glow.Spawn(8, 4);
        for (int frame = 0; frame < 16; frame++) glow.Step(text);
        AssertEqual(3, text.ShownAt(8, 4).ColourVariant, "a letter glows again after its earlier glow ended");
        Console.WriteLine("  Cinematic text glow: exact 0/5/10/15-frame colour variants, letter preservation, unbounded simultaneous glows, and debugger continuation pass.");
    }

    /// <summary>Narration text with the letter A typed at every place of the text rows.</summary>
    private static IntroNarrationText LetteredText()
    {
        var text = new IntroNarrationText();
        for (int row = IntroNarrationText.FirstTextRow; row < IntroNarrationText.FirstTextRow + IntroNarrationText.TextRows; row++)
        {
            for (int column = 0; column < IntroNarrationText.RowCells; column++)
                text.Type(new IntroNarrationCharacter(column, row, 'A'));
        }
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
