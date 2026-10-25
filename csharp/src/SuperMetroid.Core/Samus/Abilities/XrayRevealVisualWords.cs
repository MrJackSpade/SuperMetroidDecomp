namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Replaceable metatile references for one cartridge-defined X-ray reveal.</summary>
/// <param name="TopLeft">First 16x16 visual metatile operand, used by every drawable reveal.</param>
/// <param name="TopRight">Adjacent horizontal metatile operand for wide and square copies.</param>
/// <param name="BottomLeft">Adjacent vertical metatile operand for tall and square copies.</param>
/// <param name="BottomRight">Diagonal metatile operand for square copies; other command shapes leave it unused.</param>
public readonly record struct XrayRevealVisualWords(
    int TopLeft,
    int TopRight,
    int BottomLeft,
    int BottomRight);
