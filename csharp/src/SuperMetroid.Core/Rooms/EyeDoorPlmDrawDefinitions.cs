namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical bank-$84 draw payloads for both mirrored, three-component eye doors.
/// The eye-door PLM instruction streams still control timing, attacks, hits,
/// persistence, and the eventual conversion to an ordinary blue door.
/// </summary>
internal static class EyeDoorPlmDrawDefinitions
{
    /// <summary>Left-facing eye animation begins at $84:9C03.</summary>
    private const ushort LeftEyeFirst = 0x9c03;
    /// <summary>Left-facing middle component begins at $84:9C2B.</summary>
    private const ushort LeftMiddleFirst = 0x9c2b;
    /// <summary>Left-facing bottom component begins at $84:9C3D.</summary>
    private const ushort LeftBottomFirst = 0x9c3d;
    /// <summary>Left-facing eye's four-block clearing frame is $84:9C4F.</summary>
    private const ushort LeftEyeClear = 0x9c4f;
    /// <summary>$84:9BF7 is the horizontally mirrored four-block clear used by the left eye's opening list.</summary>
    internal const ushort MirroredOpeningClear = 0x9bf7;
    /// <summary>Right-facing eye animation begins at $84:9C5B.</summary>
    private const ushort RightEyeFirst = 0x9c5b;

    internal enum Component { Eye, Middle, Bottom, Clear }

    /// <summary>The two four-block clearing frames that share one visual identity.</summary>
    private enum ClearFrame : ushort
    {
        /// <summary>$84:9BF7, the mirrored clear drawn by the left eye's opening list.</summary>
        MirroredOpening = MirroredOpeningClear,
        /// <summary>$84:9C4F, the left-facing eye's clearing frame.</summary>
        LeftEye = LeftEyeClear,
    }

    private static bool IsLeftOpeningClear(ClearFrame frame) => frame switch
    {
        ClearFrame.MirroredOpening => true,
        ClearFrame.LeftEye => false,
        _ => throw new InvalidOperationException($"Undefined {nameof(ClearFrame)} {(int)frame}."),
    };

    internal readonly record struct Draw(ushort Pointer, Component Part, int Frame, bool Left)
    {
        internal bool Vertical => Part is Component.Eye or Component.Clear;
        internal int WordCount => Pointer == 0 ? 0 : Part == Component.Clear ? 4 : Part == Component.Eye ? 2 : 1;
        internal ushort WordAt(int cell)
        {
            if ((uint)cell >= (uint)WordCount) throw new IndexOutOfRangeException();
            int tile, collision;
            bool bottom;
            if (Part == Component.Clear)
            {
                tile = cell is 0 or 3 ? 0xaa : 0xcc;
                collision = 8;
                bottom = cell >= 2;
            }
            else if (Part == Component.Eye)
            {
                // Closed, opening, vulnerable, hit-flash and open-but-solid draws.
                tile = Frame < 3 ? 0xcc - Frame : Frame == 3 ? 0xcd : 0xca;
                collision = Frame == 2 ? 0xc + cell : 8;
                bottom = cell != 0;
            }
            else
            {
                tile = 0xaa + Frame;
                collision = 0xa;
                bottom = Part == Component.Bottom;
            }
            return (ushort)((collision << 12) | (Left ? 0x400 : 0) | (bottom ? 0x800 : 0) | tile);
        }
    }

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int side = 0; side < 2; side++)
            {
                int shift = side * (RightEyeFirst - LeftEyeFirst);
                TryGet((ushort)(MirroredOpeningClear + shift), out var clear);
                yield return clear;
                for (int frame = 0; frame < 5; frame++)
                {
                    TryGet((ushort)(LeftEyeFirst + shift + frame * 8), out var eye);
                    yield return eye;
                }
                for (int component = 0; component < 2; component++)
                for (int frame = 0; frame < 3; frame++)
                {
                    TryGet((ushort)(LeftMiddleFirst + shift + component * 18 + frame * 6), out var value);
                    yield return value;
                }
            }
        }
    }

    /// <summary>The mirrored clear shares one visual identity, preserving existing 23-frame overrides.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> Editable =>
        All.Where(draw => draw.Pointer != MirroredOpeningClear);

    internal static ushort VisualSource(ushort pointer) =>
        pointer == MirroredOpeningClear ? LeftEyeClear : pointer;

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        if (Enum.IsDefined((ClearFrame)pointer))
        {
            draw = new(pointer, Component.Clear, 0, IsLeftOpeningClear((ClearFrame)pointer));
            return true;
        }
        bool left = pointer < RightEyeFirst;
        int normalized = left ? pointer : pointer - (RightEyeFirst - LeftEyeFirst);
        Component part;
        int first, count, stride;
        if (normalized < LeftMiddleFirst)
        { part = Component.Eye; first = LeftEyeFirst; count = 5; stride = 8; }
        else if (normalized < LeftBottomFirst)
        { part = Component.Middle; first = LeftMiddleFirst; count = 3; stride = 6; }
        else
        { part = Component.Bottom; first = LeftBottomFirst; count = 3; stride = 6; }
        int offset = normalized - first;
        if (offset < 0 || offset >= count * stride || offset % stride != 0)
        { draw = default; return false; }
        draw = new(pointer, part, offset / stride, left);
        return true;
    }

    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (!TryDescribe(pointer, out var draw)) { list = default; return false; }
        var words = new ushort[draw.WordCount];
        for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(cell);
        list = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new((ushort)((draw.Vertical ? 0x8000 : 0) | words.Length), words, 0, 0),
        });
        return true;
    }

    internal static string VisualId(ushort pointer)
    {
        if (pointer == LeftEyeClear) return "left-eye-clear";
        bool right = pointer >= RightEyeFirst;
        int normalized = right ? pointer - (RightEyeFirst - LeftEyeFirst) : pointer;
        string component;
        int first, count, stride;
        if (normalized < LeftMiddleFirst)
        { component = "eye"; first = LeftEyeFirst; count = 5; stride = 8; }
        else if (normalized < LeftBottomFirst)
        { component = "middle"; first = LeftMiddleFirst; count = 3; stride = 6; }
        else
        { component = "bottom"; first = LeftBottomFirst; count = 3; stride = 6; }
        int offset = normalized - first;
        if (offset < 0 || offset >= count * stride || offset % stride != 0)
            throw new InvalidDataException($"Eye-door draw ${pointer:X4} has no visual ID.");
        return $"{(right ? "right" : "left")}-{component}-frame-{offset / stride}";
    }

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in Editable)
        {
            if (string.Equals(id, VisualId(candidate.Pointer), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }

}
