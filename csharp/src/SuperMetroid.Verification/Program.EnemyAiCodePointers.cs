using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// Audits the production enemy callback catalog rather than maintaining a second hand-written
    /// list in the verifier. The root fields are complete 24-bit initialization/main entry points;
    /// the nested bank containers hold the 16-bit interaction callbacks stored in enemy headers.
    /// </summary>
    static void VerifyEnemyAiCodePointerCatalog()
    {
        FieldInfo[] longEntryPoints = typeof(EnemyAiCodePointers)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(int))
            .ToArray();
        AssertTrue(longEntryPoints.Length != 0,
            "enemy AI catalog contains 24-bit initialization/main entry points");

        foreach (FieldInfo field in longEntryPoints)
        {
            int address = (int)field.GetRawConstantValue()!;
            AssertTrue(address is >= 0x808000 and <= 0xffffff,
                $"enemy AI entry {field.Name} is a mapped 24-bit cartridge address");
        }

        Type[] bankCatalogs = typeof(EnemyAiCodePointers)
            .GetNestedTypes(BindingFlags.Public)
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ToArray();
        AssertTrue(bankCatalogs.Length != 0,
            "enemy AI catalog contains bank-local interaction callback containers");

        var interactionPointers = new HashSet<ushort>();
        int interactionEntryCount = 0;
        foreach (Type bankCatalog in bankCatalogs)
        {
            AssertTrue(bankCatalog.Name.StartsWith("Bank", StringComparison.Ordinal),
                $"enemy interaction catalog {bankCatalog.Name} identifies its native bank");

            FieldInfo[] fields = bankCatalog
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && field.FieldType == typeof(ushort))
                .ToArray();
            AssertTrue(fields.Length != 0,
                $"enemy interaction catalog {bankCatalog.Name} is not empty");

            foreach (FieldInfo field in fields)
            {
                ushort pointer = (ushort)field.GetRawConstantValue()!;
                AssertTrue(pointer >= 0x8000,
                    $"enemy interaction callback {bankCatalog.Name}.{field.Name} is in mapped ROM");
                interactionPointers.Add(pointer);
                interactionEntryCount++;
            }
        }

        // This reflection scan covers the constants used by the production RoomEnemySystem
        // partials. Any newly translated raw bank-local AI value fails until it is added to one
        // of the named bank containers above. Small AI-handler property masks are intentionally
        // excluded: they are data flags, not executable callbacks.
        FieldInfo[] translatedCallbacks = typeof(RoomEnemySystem)
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(field =>
                field.IsLiteral &&
                field.FieldType == typeof(ushort) &&
                field.Name.EndsWith("Ai", StringComparison.Ordinal) &&
                (ushort)field.GetRawConstantValue()! >= 0x8000)
            .ToArray();
        foreach (FieldInfo callback in translatedCallbacks)
        {
            ushort pointer = (ushort)callback.GetRawConstantValue()!;
            AssertTrue(interactionPointers.Contains(pointer),
                $"translated enemy callback {callback.Name} (${pointer:X4}) is centrally named");
        }

        Console.WriteLine(
            $"  Enemy AI catalog: {longEntryPoints.Length} long entry points, " +
            $"{interactionEntryCount} bank-local callbacks, and " +
            $"{translatedCallbacks.Length} production callback aliases verified.");
    }
}
