using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Development-only transcription of walking-Pirate extended hitbox records.
    /// The checked-in result is immutable simulation metadata, not editable art.
    /// </summary>
    private static void GenerateWalkingPirateCollisionDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var frames = new List<(ushort Pointer,
            (short X, short Y, ushort Hitboxes)[] Components)>();
        var hitboxes = new SortedDictionary<ushort,
            (short Left, short Top, short Right, short Bottom,
                ushort TouchAi, ushort ShotAi)[]>();
        int componentTotal = 0;
        EnemyExtendedFrameDefinition[] sourceFrames =
        [
            new(EnemyExtendedFrameDefinitions.Bank,
                EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap,
                "walking_pirate_empty_collision"),
            .. EnemyExtendedFrameDefinitions.Frames.ToArray(),
        ];
        foreach (EnemyExtendedFrameDefinition frame in sourceFrames
                     .OrderBy(frame => frame.Pointer))
        {
            int count = rom.ReadByte((frame.Bank << 16) | frame.Pointer);
            if (count is < 1 or > EnemyExtendedFrameDefinitions.MaximumComponents)
                throw new InvalidDataException(
                    $"Walking Pirate frame $B2:{frame.Pointer:X4} has {count} components.");
            var components = new (short X, short Y, ushort Hitboxes)[count];
            for (int index = 0; index < count; index++)
            {
                ushort record = unchecked((ushort)(frame.Pointer + 2 + index * 8));
                short x = unchecked((short)ReadWord(rom, frame.Bank, record));
                short y = unchecked((short)ReadWord(rom, frame.Bank,
                    unchecked((ushort)(record + 2))));
                ushort hitboxPointer = ReadWord(rom, frame.Bank,
                    unchecked((ushort)(record + 6)));
                components[index] = (x, y, hitboxPointer);
                if (hitboxes.ContainsKey(hitboxPointer))
                    continue;
                int rectangleCount = ReadWord(rom, frame.Bank, hitboxPointer);
                if (rectangleCount is < 1 or > 2)
                    throw new InvalidDataException(
                        $"Walking Pirate hitbox $B2:{hitboxPointer:X4} has {rectangleCount} rectangles.");
                var rectangles = new (short Left, short Top, short Right,
                    short Bottom, ushort TouchAi, ushort ShotAi)[rectangleCount];
                for (int rectangle = 0; rectangle < rectangleCount; rectangle++)
                {
                    ushort address = unchecked((ushort)(hitboxPointer + 2 + rectangle * 12));
                    rectangles[rectangle] = (
                        unchecked((short)ReadWord(rom, frame.Bank, address)),
                        unchecked((short)ReadWord(rom, frame.Bank,
                            unchecked((ushort)(address + 2)))),
                        unchecked((short)ReadWord(rom, frame.Bank,
                            unchecked((ushort)(address + 4)))),
                        unchecked((short)ReadWord(rom, frame.Bank,
                            unchecked((ushort)(address + 6)))),
                        ReadWord(rom, frame.Bank,
                            unchecked((ushort)(address + 8))),
                        ReadWord(rom, frame.Bank,
                            unchecked((ushort)(address + 10))));
                }
                hitboxes.Add(hitboxPointer, rectangles);
            }
            componentTotal += count;
            frames.Add((frame.Pointer, components));
        }
        if (frames.Count != 38 || componentTotal != 76 || hitboxes.Count != 46 ||
            hitboxes.Values.Sum(rectangles => rectangles.Length) != 48)
            throw new InvalidDataException(
                "Walking Pirate collision inventory changed from the pinned cartridge.");

        var source = new StringBuilder(12000);
        source.AppendLine("// Generated from the pinned cartridge by --generate-walking-pirate-collision.");
        source.AppendLine("namespace SuperMetroid.Core.Game;");
        source.AppendLine();
        source.AppendLine("/// <summary>Engine-owned offset and hitbox identity in one extended frame component.</summary>");
        source.AppendLine("internal readonly record struct WalkingPirateCollisionComponent(short X, short Y, ushort HitboxPointer);");
        source.AppendLine("/// <summary>Engine-owned signed collision bounds and touch/shot callbacks.</summary>");
        source.AppendLine("internal readonly record struct WalkingPirateCollisionHitbox(short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);");
        source.AppendLine("internal readonly record struct WalkingPirateCollisionFrame(ushort Pointer, WalkingPirateCollisionComponent[] Components);");
        source.AppendLine("internal readonly record struct WalkingPirateCollisionList(ushort Pointer, WalkingPirateCollisionHitbox[] Rectangles);");
        source.AppendLine();
        source.AppendLine("/// <summary>Fixed bank-$B2 walking-Pirate collision data, separate from editable OAM art.</summary>");
        source.AppendLine("internal static class WalkingPirateCollisionDefinitions");
        source.AppendLine("{");
        source.AppendLine("    private static readonly WalkingPirateCollisionFrame[] Frames =");
        source.AppendLine("    [");
        foreach ((ushort pointer, (short X, short Y, ushort Hitboxes)[] components)
                 in frames)
        {
            source.Append($"        new(0x{pointer:X4}, [");
            foreach ((short x, short y, ushort hitbox) in components)
                source.Append($"new({x}, {y}, 0x{hitbox:X4}), ");
            source.AppendLine("]),");
        }
        source.AppendLine("    ];");
        source.AppendLine("    private static readonly WalkingPirateCollisionList[] Lists =");
        source.AppendLine("    [");
        foreach ((ushort pointer, (short Left, short Top, short Right,
                     short Bottom, ushort TouchAi, ushort ShotAi)[] rectangles)
                 in hitboxes)
        {
            source.Append($"        new(0x{pointer:X4}, [");
            foreach ((short left, short top, short right, short bottom,
                         ushort touch, ushort shot) in rectangles)
                source.Append($"new({left}, {top}, {right}, {bottom}, 0x{touch:X4}, 0x{shot:X4}), ");
            source.AppendLine("]),");
        }
        source.AppendLine("    ];");
        source.AppendLine("    internal static int FrameCount => Frames.Length;");
        source.AppendLine("    internal static int ListCount => Lists.Length;");
        source.AppendLine("    internal static WalkingPirateCollisionFrame Frame(int index) => Frames[index];");
        source.AppendLine("    internal static WalkingPirateCollisionList List(int index) => Lists[index];");
        source.AppendLine();
        source.AppendLine("    internal static ReadOnlySpan<WalkingPirateCollisionComponent> ComponentsAt(ushort pointer)");
        source.AppendLine("    {");
        source.AppendLine("        int low = 0, high = Frames.Length - 1;");
        source.AppendLine("        while (low <= high)");
        source.AppendLine("        {");
        source.AppendLine("            int middle = low + ((high - low) >> 1);");
        source.AppendLine("            ushort candidate = Frames[middle].Pointer;");
        source.AppendLine("            if (candidate == pointer) return Frames[middle].Components;");
        source.AppendLine("            if (candidate < pointer) low = middle + 1;");
        source.AppendLine("            else high = middle - 1;");
        source.AppendLine("        }");
        source.AppendLine("        throw new InvalidDataException($\"Walking Pirate collision frame $B2:{pointer:X4} is not compiled.\");");
        source.AppendLine("    }");
        source.AppendLine("    internal static ReadOnlySpan<WalkingPirateCollisionHitbox> HitboxesAt(ushort pointer)");
        source.AppendLine("    {");
        source.AppendLine("        int low = 0, high = Lists.Length - 1;");
        source.AppendLine("        while (low <= high)");
        source.AppendLine("        {");
        source.AppendLine("            int middle = low + ((high - low) >> 1);");
        source.AppendLine("            ushort candidate = Lists[middle].Pointer;");
        source.AppendLine("            if (candidate == pointer) return Lists[middle].Rectangles;");
        source.AppendLine("            if (candidate < pointer) low = middle + 1;");
        source.AppendLine("            else high = middle - 1;");
        source.AppendLine("        }");
        source.AppendLine("        throw new InvalidDataException($\"Walking Pirate hitbox list $B2:{pointer:X4} is not compiled.\");");
        source.AppendLine("    }");
        source.AppendLine("}");
        const string path =
            "csharp/src/SuperMetroid.Core/Game/WalkingPirateCollisionDefinitions.cs";
        File.WriteAllText(path, source.ToString(), new UTF8Encoding(false));
        Console.WriteLine(
            $"Generated {frames.Count} walking-Pirate collision frames, " +
            $"{componentTotal} components, {hitboxes.Count} lists and 48 rectangles at {path}.");

        static ushort ReadWord(ISnesAddressSpace bus, byte bank, ushort address) =>
            (ushort)(bus.ReadByte((bank << 16) | address) |
                bus.ReadByte((bank << 16) | unchecked((ushort)(address + 1))) << 8);
    }
}
