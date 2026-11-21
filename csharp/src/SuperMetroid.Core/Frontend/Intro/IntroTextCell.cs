namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One cell of the opening's narration text (#1467): the letter typed there, if any, and its glow step.</summary>
/// <param name="Letter">The narration character shown in the cell; null where the cell is blank.</param>
/// <param name="Glow">The glow step the cell last showed; the typed colours until a glow recolours it.</param>
internal readonly partial record struct IntroTextCell(char? Letter, IntroTextGlowStep Glow);
