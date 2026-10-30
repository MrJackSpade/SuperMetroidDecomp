// A window (ScreenWindow, #1468), packed by D3D11WindowPacking into WindowShapes: the first row holds the shape count,
// the first line and the line after the last; each shape then takes two rows, its kind and parameters, signed.
//   Rectangle: left, top, right, bottom, both corners included.
//   Oval: its core's left end x and y, the core's width, then the horizontal and vertical radii.
//   Cone: its apex x and y, then the edges it sweeps clockwise from and to, in 256ths of a turn clockwise from up.
static const uint WindowRectangle = 1, WindowOval = 2, WindowCone = 3;
// A cone's edge keeps the pixels whose centres lie within half a pixel outside it, so a zero-width cone is a ray one
// pixel wide.
static const float ConeEdgeTolerance = 0.5;
static const float TurnRadians = 6.28318530718;
static const float AngleUnitsPerTurn = 256.0;

// The direction an edge angle points on the screen, x right and y down.
float2 ConeEdge(int angle)
{
    float turned = angle * (TurnRadians / AngleUnitsPerTurn);
    return float2(sin(turned), -cos(turned));
}

// How far a point lies clockwise of a direction: positive to its right on the screen.
float Clockwise(float2 direction, float2 offset) { return direction.x * offset.y - direction.y * offset.x; }

bool InWindowShape(int4 head, int4 tail, int2 pixel)
{
    uint kind = (uint)head.x;
    if (kind == WindowRectangle)
        return pixel.x >= head.y && pixel.y >= head.z && pixel.x <= head.w && pixel.y <= tail.x;
    if (kind == WindowOval)
    {
        float dx = (float)max(0, max(head.y - pixel.x, pixel.x - (head.y + head.w)));
        float dy = (float)(pixel.y - head.z);
        float rx = (float)tail.x, ry = (float)tail.y;
        return (dx * dx) / (rx * rx) + (dy * dy) / (ry * ry) <= 1.0;
    }
    if (kind == WindowCone)
    {
        float2 offset = float2(pixel - head.yz);
        float2 first = ConeEdge(head.w), second = ConeEdge(tail.x);
        return Clockwise(first, offset) >= -ConeEdgeTolerance && Clockwise(offset, second) >= -ConeEdgeTolerance
            && dot(offset, first + second) >= 0.0;
    }
    return false;
}

bool InWindow(uint2 screen)
{
    uint4 header = WindowShapes[0];
    if (screen.y < header.y || screen.y >= header.z) return false;
    [loop] for (uint shape = 0; shape < header.x; shape++)
        if (InWindowShape(asint(WindowShapes[1 + shape * 2]), asint(WindowShapes[2 + shape * 2]), int2(screen)))
            return true;
    return false;
}
