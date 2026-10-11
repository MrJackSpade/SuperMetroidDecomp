using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>
    /// Confirms the #628 Samus spatial values: the external displacement's native 16.16 arithmetic
    /// and half-word operations, the fixed position's axis/half-word operations, and the debugger
    /// migration that rebuilds them from the retired per-component words.
    /// </summary>
    private static void VerifySamusSpatialValues()
    {
        VerifyExtraDisplacementArithmetic();
        VerifyFixedPositionOperations();
        VerifyKinematicsLegacyGraph();
        Console.WriteLine("  Samus spatial values: displacement words, carry/wrap, half-word updates and legacy restoration agree.");
    }

    private static void VerifyExtraDisplacementArithmetic()
    {
        FixedVector<RoomPixels> zero = FixedVector<RoomPixels>.Zero;
        AssertEqual(0, zero.X.RawValue, "zero X");
        AssertEqual(0, zero.Y.RawValue, "zero Y");
        AssertEqual(default(FixedVector<RoomPixels>), zero, "zero is the default value");

        // Exact construction from the four native words ($0B58, $0B56, $0B5C, $0B5A).
        var words = FixedVector<RoomPixels>.FromWords(0xfffe, 0x8000, 0x0003, 0x4000);
        AssertEqual((short)-2, words.X.WholePixels, "signed X whole word");
        AssertEqual((ushort)0x8000, words.X.Fraction, "X fraction word");
        AssertEqual(unchecked((int)0xfffe8000), words.X.RawValue, "X assembles as signed 16.16");
        AssertEqual((short)3, words.Y.WholePixels, "Y whole word");
        AssertEqual((ushort)0x4000, words.Y.Fraction, "Y fraction word");

        // Axis and half-word replacement leave every other word untouched.
        var wholeX = words.WithWholeX(7);
        AssertEqual(FixedVector<RoomPixels>.FromWords(7, 0x8000, 3, 0x4000), wholeX, "whole X replacement keeps X fraction and Y");
        var fractionY = words.WithFractionY(0x0001);
        AssertEqual(FixedVector<RoomPixels>.FromWords(0xfffe, 0x8000, 3, 0x0001), fractionY, "fraction Y replacement keeps Y whole and X");
        var axisY = words.WithY(SnesSignedSixteenSixteen.Zero);
        AssertEqual(FixedVector<RoomPixels>.FromWords(0xfffe, 0x8000, 0, 0), axisY, "axis replacement keeps the other axis");

        // Carry from the fraction, borrow into it, and 32-bit wrap on each axis independently.
        var carry = FixedVector<RoomPixels>.FromWords(0, 0xc000, 0, 0) + FixedVector<RoomPixels>.FromWords(0, 0x8000, 0, 0);
        AssertEqual(FixedVector<RoomPixels>.FromWords(1, 0x4000, 0, 0), carry, "fraction carry reaches the whole word");
        var borrow = FixedVector<RoomPixels>.FromWords(0, 0, 1, 0) - FixedVector<RoomPixels>.FromWords(0, 0, 0, 1);
        AssertEqual(FixedVector<RoomPixels>.FromWords(0, 0, 0, 0xffff), borrow, "subtraction borrows from the whole word");
        var wrap = FixedVector<RoomPixels>.FromWords(0x7fff, 0xffff, 0xffff, 0xffff) + FixedVector<RoomPixels>.FromWords(0, 1, 0, 1);
        AssertEqual(FixedVector<RoomPixels>.FromWords(0x8000, 0, 0, 0), wrap, "each axis wraps at 32 bits");

        // Producers that add whole pixels (platforms, turtles, shutters) never touch the fraction.
        var pushed = words + new Offset16<RoomPixels>(0, -4);
        AssertEqual(FixedVector<RoomPixels>.FromWords(0xfffe, 0x8000, 0xffff, 0x4000), pushed, "whole-pixel push keeps fractions");
        var wrappedWhole = FixedVector<RoomPixels>.FromWords(0x7fff, 0x1234, 0, 0) + new Offset16<RoomPixels>(1, 0);
        AssertEqual(FixedVector<RoomPixels>.FromWords(0x8000, 0x1234, 0, 0), wrappedWhole, "whole-pixel push wraps the whole word only");

        // Producer accumulation within one EnemyMain: the frame clear, a riding platform's
        // whole Y delta, then an assigning producer (Oum) replacing only the whole Y word.
        var body = new SamusKinematicsState { ExtraDisplacement = FixedVector<RoomPixels>.FromWords(5, 0x1111, 6, 0x2222) };
        body.ExtraDisplacement = FixedVector<RoomPixels>.Zero;
        body.ExtraDisplacement += new Offset16<RoomPixels>(0, 3);
        body.ExtraDisplacement = body.ExtraDisplacement.WithFractionY(0x8000).WithWholeY(-1);
        AssertEqual(FixedVector<RoomPixels>.FromWords(0, 0, 0xffff, 0x8000), body.ExtraDisplacement,
            "clear, add, then whole-word assignment keeps the producer's fraction");
    }

    private static void VerifyFixedPositionOperations()
    {
        var position = new FixedPosition<RoomPixels>(new SnesFixedPosition(0x0100, 0xffff), new SnesFixedPosition(0x0200, 0x0001));
        AssertEqual(new Position16<RoomPixels>(0x0100, 0x0200), position.Whole, "whole position drops subpixels");
        AssertEqual(0x0100ffffu, position.X.RawValue, "X assembles as unsigned 16.16");
        var advanced = position.AdvanceX(SnesSignedSixteenSixteen.FromRaw(1));
        AssertEqual(new SnesFixedPosition(0x0101, 0), advanced.X, "advance carries into the whole word");
        AssertEqual(position.Y, advanced.Y, "advancing X leaves Y");
        var translated = position - new Offset16<RoomPixels>(0, 0x0201);
        AssertEqual(new SnesFixedPosition(0xffff, 0x0001), translated.Y, "whole translation wraps and keeps the subpixel");
        AssertEqual(position.X, translated.X, "translating Y leaves X");
        var halves = position.WithFractionX(0).WithWholeY(7);
        AssertEqual(new FixedPosition<RoomPixels>(new SnesFixedPosition(0x0100, 0), new SnesFixedPosition(7, 0x0001)), halves,
            "half-word replacement touches only the named word");
    }

    /// <summary>
    /// Rebuilds a capture in the pre-#628 layout (ten component auto-properties) and confirms
    /// restoration reconstructs the three spatial values exactly, including the word order.
    /// </summary>
    private static void VerifyKinematicsLegacyGraph()
    {
        var body = new SamusKinematicsState
        {
            Position = new(new(0x1234, 0x5678), new(0x9abc, 0xdef0)),
            ExtraDisplacement = FixedVector<RoomPixels>.FromWords(0xfffc, 0x8001, 0x0002, 0x4003),
            Radius = new(5, 0x13),
        };
        // An ownerless body's unsampled pose ($FF) is not a serializable pose; sample one.
        typeof(SamusKinematicsState).GetProperty(nameof(SamusKinematicsState.CollisionPose))!
            .SetValue(body, SamusPoseId.FacingRightNormalPose);
        using var current = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(current, body);
        byte[] bytes = current.ToArray();
        bytes = ReplaceSerializedField(bytes, typeof(SamusKinematicsState), "Position", body.Position,
            [("XPosition", (ushort)0x1234), ("XSubposition", (ushort)0x5678), ("YPosition", (ushort)0x9abc), ("YSubposition", (ushort)0xdef0)]);
        bytes = ReplaceSerializedField(bytes, typeof(SamusKinematicsState), "ExtraDisplacement", body.ExtraDisplacement,
            [("ExtraXDisplacement", (ushort)0xfffc), ("ExtraXSubdisplacement", (ushort)0x8001),
             ("ExtraYDisplacement", (ushort)0x0002), ("ExtraYSubdisplacement", (ushort)0x4003)]);
        bytes = ReplaceSerializedField(bytes, typeof(SamusKinematicsState), "Radius", body.Radius,
            [("XRadius", (ushort)5), ("YRadius", (ushort)0x13)]);

        using var legacy = new MemoryStream(bytes);
        var restored = DebuggerObjectGraphSerializer.Deserialize<SamusKinematicsState>(legacy);
        AssertEqual(body.Position, restored.Position, "legacy position words rebuild the 16.16 position");
        AssertEqual(body.ExtraDisplacement, restored.ExtraDisplacement, "legacy four displacement words rebuild the vector");
        AssertEqual(body.Radius, restored.Radius, "legacy radius words rebuild the extent");

        using var roundTrip = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(roundTrip, restored);
        roundTrip.Position = 0;
        var again = DebuggerObjectGraphSerializer.Deserialize<SamusKinematicsState>(roundTrip);
        AssertEqual(body.ExtraDisplacement, again.ExtraDisplacement, "current format round-trips the displacement");
        AssertEqual(body.Position, again.Position, "current format round-trips the position");

        byte[] partial = ReplaceSerializedField(current.ToArray(), typeof(SamusKinematicsState), "Radius", body.Radius,
            [("XRadius", (ushort)5)]);
        bool rejected = false;
        try { DebuggerObjectGraphSerializer.Deserialize<SamusKinematicsState>(new MemoryStream(partial)); }
        catch (InvalidDataException) { rejected = true; }
        AssertTrue(rejected, "a legacy capture missing one component word is rejected");
    }

    /// <summary>
    /// Replaces one serialized root field with the given retired primitive fields and fixes the
    /// root field count. Primitive values use reference ID zero, so graph identities stay exact.
    /// </summary>
    private static byte[] ReplaceSerializedField(
        byte[] bytes, Type owner, string property, object currentValue, (string Name, object Value)[] legacy)
    {
        byte[] current = EncodeField(owner, $"<{property}>k__BackingField", currentValue);
        int start = bytes.AsSpan().IndexOf(current);
        AssertTrue(start >= 0, $"current {property} field exists in the serialized graph");
        using var replacement = new MemoryStream();
        foreach ((string name, object value) in legacy)
            replacement.Write(EncodeField(owner, $"<{name}>k__BackingField", value));
        byte[] result = [.. bytes.AsSpan(0, start), .. replacement.ToArray(), .. bytes.AsSpan(start + current.Length)];

        using var header = new MemoryStream(result);
        using var reader = new BinaryReader(header);
        reader.ReadByte(); reader.ReadInt32(); reader.ReadString(); reader.ReadByte();
        int countOffset = checked((int)header.Position);
        int count = reader.ReadInt32();
        BitConverter.TryWriteBytes(result.AsSpan(countOffset, 4), count - 1 + legacy.Length);
        return result;

        static byte[] EncodeField(Type owner, string name, object value)
        {
            using var encoded = new MemoryStream();
            using (var writer = new BinaryWriter(encoded, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(owner.AssemblyQualifiedName!);
                writer.Write(name);
            }
            DebuggerObjectGraphSerializer.Serialize(encoded, value);
            return encoded.ToArray();
        }
    }
}
