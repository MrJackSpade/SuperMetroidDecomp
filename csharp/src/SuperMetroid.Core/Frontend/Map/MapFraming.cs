namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// How one map screen frames the area map (#1975): where it centres the revealed rows and how far each scroll arrow lets
/// the revealed map come into the screen. File select and the pause screen frame the map by the same rules
/// (<see cref="MapScroll"/>); these are the places their screens differ (<see cref="MapFramingDefinitions"/>).
/// </summary>
/// <param name="CentreLine">The screen line, in pixels from the top, the framing centres the middle of the revealed rows on.</param>
/// <param name="LeftEdge">The left arrow scrolls while the revealed map's leftmost column lies left of this screen column.</param>
/// <param name="RightEdge">The right arrow scrolls while the revealed map's rightmost column lies at or right of this screen column.</param>
/// <param name="TopEdge">The up arrow scrolls while the revealed map's top row lies above this screen line.</param>
/// <param name="BottomEdge">The down arrow scrolls while the revealed map's bottom row lies at or below this screen line.</param>
/// <param name="ArrowDrop">Screen pixels the screen draws its scroll arrows below their installed places.</param>
public sealed record MapFraming(int CentreLine, int LeftEdge, int RightEdge, int TopEdge, int BottomEdge, int ArrowDrop);
