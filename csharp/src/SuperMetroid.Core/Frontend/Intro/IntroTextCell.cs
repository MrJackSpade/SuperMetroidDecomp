namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One cell of the opening's narration text (#1467): the letter typed there, if any, and its colour variant.</summary>
/// <param name="Letter">The narration character shown in the cell; null where the cell is blank.</param>
/// <param name="ColourVariant">The colour variant the cell's glow last gave it; zero until a glow recolours it.</param>
internal readonly record struct IntroTextCell(char? Letter, int ColourVariant);
