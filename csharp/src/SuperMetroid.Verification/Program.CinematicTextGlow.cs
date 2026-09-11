using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyCinematicTextGlow()
    {
        var glow = new CinematicTextGlowSystem();
        var map = Enumerable.Repeat((ushort)0xe155, 1024).ToArray();
        glow.Spawn(1, 4, 2, 2);
        CinematicTextGlowSystem? restored = null;
        ushort[]? restoredMap = null;
        for (int age = 0; age <= 20; age++)
        {
            glow.Step(map);
            restored?.Step(restoredMap!);
            for (int index = 0; index < map.Length; index++)
            {
                bool rectangle = index / 32 is 4 or 5 && index % 32 is 1 or 2;
                ushort expected = (ushort)(0xe155 | (rectangle ? Math.Min(age / 5, 3) << 10 : 0));
                AssertEqual(expected, map[index], $"glyph age {age} tile {index} preserves non-palette bits and exact rectangle");
            }
            if (restored is not null)
                AssertTrue(map.AsSpan().SequenceEqual(restoredMap), "mid-glow debugger restore retains all timers and rectangle state");
            if (age == 7)
            {
                using var stream = new MemoryStream();
                DebuggerObjectGraphSerializer.Serialize(stream, glow);
                stream.Position = 0;
                restored = DebuggerObjectGraphSerializer.Deserialize<CinematicTextGlowSystem>(stream);
                restoredMap = map.ToArray();
            }
        }
        // Eight simultaneous allocations fit; the ninth has no slot. After final
        // palette application releases the slots, that same rectangle can allocate.
        glow = new CinematicTextGlowSystem();
        Array.Fill(map, (ushort)0);
        for (int i = 0; i < 9; i++) glow.Spawn(i, 0, 1, 1);
        for (int frame = 0; frame < 16; frame++) glow.Step(map);
        for (int i = 0; i < 8; i++) AssertEqual((ushort)0x0c00, map[i], "eight native glow slots mature");
        AssertEqual((ushort)0, map[8], "full pool declines ninth glow");
        glow.Spawn(8, 0, 1, 1);
        for (int frame = 0; frame < 16; frame++) glow.Step(map);
        AssertEqual((ushort)0x0c00, map[8], "finished glow frees a native slot");
        var fields = typeof(IntroCinematicObjectSystem).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .OrderBy(field => field.MetadataToken).ToArray();
        var legacy = DebuggerStateFieldMigrations.SelectSerializedFields(typeof(IntroCinematicObjectSystem), fields, fields.Length - 1);
        AssertTrue(legacy.SequenceEqual(fields.Where(field => field.Name != "textGlow")), "legacy intro retains every previous serialized field");
        Console.WriteLine("  Cinematic text glow: exact 0/5/10/15-frame palette changes, rectangle preservation, pool exhaustion/reuse, and debugger continuation pass.");
    }
}
