using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Preserves the eye-door editable pointer/visual-ID mapping, its ordinal reverse lookup rules, rejected identities, and the mirrored-source alias across the pointer domain.</summary>
    private static void VerifyEyeDoorVisualIds()
    {
        // Original 23-entry public override identity contract, before the naming conversion.
        (ushort Pointer, string Id)[] expected =
        [
            (0x9c03,"left-eye-frame-0"),(0x9c0b,"left-eye-frame-1"),(0x9c13,"left-eye-frame-2"),
            (0x9c1b,"left-eye-frame-3"),(0x9c23,"left-eye-frame-4"),
            (0x9c2b,"left-middle-frame-0"),(0x9c31,"left-middle-frame-1"),(0x9c37,"left-middle-frame-2"),
            (0x9c3d,"left-bottom-frame-0"),(0x9c43,"left-bottom-frame-1"),(0x9c49,"left-bottom-frame-2"),
            (0x9c4f,"left-eye-clear"),
            (0x9c5b,"right-eye-frame-0"),(0x9c63,"right-eye-frame-1"),(0x9c6b,"right-eye-frame-2"),
            (0x9c73,"right-eye-frame-3"),(0x9c7b,"right-eye-frame-4"),
            (0x9c83,"right-middle-frame-0"),(0x9c89,"right-middle-frame-1"),(0x9c8f,"right-middle-frame-2"),
            (0x9c95,"right-bottom-frame-0"),(0x9c9b,"right-bottom-frame-1"),(0x9ca1,"right-bottom-frame-2"),
        ];
        var exported = EyeDoorPlmDrawDefinitions.Editable.ToArray();
        AssertEqual(expected.Length, exported.Length, "Eye door original editable identity count");
        for (int index = 0; index < expected.Length; index++)
        {
            var entry = expected[index];
            AssertEqual(entry.Pointer, exported[index].Pointer, "Eye door original editable order");
            AssertEqual(entry.Id, EyeDoorPlmDrawDefinitions.VisualId(entry.Pointer), "Eye door original artwork ID");
            AssertTrue(EyeDoorPlmDrawDefinitions.TryGetByVisualId(entry.Id,out var named), "Eye door reverse ID");
            AssertEqual(entry.Pointer,named.Pointer,"Eye door reverse draw selection");
            foreach (string bad in new[] {entry.Id.ToUpperInvariant()," " + entry.Id,entry.Id + " "})
            {
                AssertTrue(!EyeDoorPlmDrawDefinitions.TryGetByVisualId(bad,out var missing), "Eye door ordinal identity");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList),missing,"Eye door rejected ID output");
            }
        }
        foreach (string? bad in new string?[] {null,"","left-eye-frame-5","right-middle-frame-3","left-bottom-frame-01","right-eye-clear"})
            AssertTrue(!EyeDoorPlmDrawDefinitions.TryGetByVisualId(bad!,out _),"Eye door unsupported identity");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            if (!expected.Any(entry => entry.Pointer == raw))
                AssertThrows<InvalidDataException>(() => EyeDoorPlmDrawDefinitions.VisualId((ushort)raw),"Eye door full rejected pointer domain");
            AssertEqual((ushort)(raw == 0x9bf7 ? 0x9c4f : raw),
                EyeDoorPlmDrawDefinitions.VisualSource((ushort)raw),"Eye door original mirrored alias");
        }
    }
}
