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

    /// <summary>Physical piece represented by one eye-door draw record.</summary>
    internal enum Component
    {
        /// <summary>Animated eye blocks, including closed, vulnerable, hit-flash, and solid-open frames.</summary>
        Eye,
        /// <summary>Middle body block that changes tile across its three animation frames.</summary>
        Middle,
        /// <summary>Bottom body block that changes tile across its three animation frames.</summary>
        Bottom,
        /// <summary>Four-block clearing draw used while the eye door opens.</summary>
        Clear
    }

    /// <summary>Decoded bank-$84 draw pointer and the component/frame/orientation needed to rebuild its block words.</summary>
    /// <param name="Pointer">Native instruction-list draw pointer represented by this record.</param>
    /// <param name="Part">Eye, body, or clearing component encoded at that pointer.</param>
    /// <param name="Frame">Zero-based animation frame within the selected component.</param>
    /// <param name="Left">Whether the draw belongs to the left-facing eye door.</param>
    internal readonly record struct Draw(ushort Pointer, Component Part, int Frame, bool Left)
    {
        /// <summary>Whether the component's block run is arranged vertically rather than horizontally.</summary>
        internal bool Vertical => Part is Component.Eye or Component.Clear;

        /// <summary>Number of packed block words emitted for this draw; zero marks an absent pointer.</summary>
        internal int WordCount => Pointer == 0 ? 0 : Part == Component.Clear ? 4 : Part == Component.Eye ? 2 : 1;

        /// <summary>Builds the packed tile/collision word for one block in this component frame.</summary>
        /// <param name="cell">Zero-based block position in the component's emitted run.</param>
        /// <returns>Block word combining collision, facing/bottom flags, and tile index.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="cell"/> is outside the emitted run.</exception>
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

    /// <summary>Enumerates all left- and right-facing eye, body, and clear draw lists in native pointer order.</summary>
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

    /// <summary>Maps the mirrored opening-clear pointer to its canonical left-eye visual source.</summary>
    /// <param name="pointer">Native eye-door draw pointer.</param>
    /// <returns>The source pointer used to retain the shared visual identity.</returns>
    internal static ushort VisualSource(ushort pointer) =>
        pointer == MirroredOpeningClear ? LeftEyeClear : pointer;

    /// <summary>Decodes a recognized eye-door draw address into its component, frame, and facing.</summary>
    /// <param name="pointer">Bank-$84 draw pointer to classify.</param>
    /// <param name="draw">Receives the decoded descriptor on success; otherwise the default value.</param>
    /// <returns><see langword="true"/> when the pointer identifies a compiled eye-door draw.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        if (pointer is MirroredOpeningClear or LeftEyeClear)
        {
            draw = new(pointer, Component.Clear, 0, pointer == MirroredOpeningClear);
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

    /// <summary>Builds the packed PLM block list for a recognized eye-door draw pointer.</summary>
    /// <param name="pointer">Bank-$84 draw pointer to materialize.</param>
    /// <param name="list">Receives the generated draw list on success; otherwise the default value.</param>
    /// <returns><see langword="true"/> when the pointer has a compiled component/frame description.</returns>
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

    /// <summary>Produces the stable editable-asset identifier for a left- or right-facing eye-door frame.</summary>
    /// <param name="pointer">Bank-$84 draw pointer whose component and frame are named.</param>
    /// <returns>Identifier such as <c>right-eye-frame-2</c>.</returns>
    /// <exception cref="InvalidDataException">The pointer does not align with a compiled component frame.</exception>
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

    /// <summary>Finds an editable eye-door draw list by its stable visual identifier.</summary>
    /// <param name="id">Identifier returned by <see cref="VisualId"/> for an editable frame.</param>
    /// <param name="list">Receives the matching draw list on success; otherwise the default value.</param>
    /// <returns><see langword="true"/> when the identifier belongs to a non-aliased editable frame.</returns>
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
