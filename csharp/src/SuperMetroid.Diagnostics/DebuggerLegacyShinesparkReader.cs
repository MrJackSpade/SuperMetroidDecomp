using System.Reflection;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Desktop;

/// <summary>Restores the explicit pre-alias nineteen-field spark layout, without dropping live crash state.</summary>
internal static class DebuggerLegacyShinesparkReader
{
    internal static void Restore(SamusShinesparkState instance, FieldInfo[] fields,
        BinaryReader reader, Func<string, Type> resolveType, Func<object?> readValue)
    {
        const string travelName = "<CrashAngularTravel>k__BackingField";
        const string deltaName = "_crashAngularDelta";
        var names = fields.Select(field => field.Name).ToList();
        int travelIndex = names.IndexOf("<SecondCrashEchoAngle>k__BackingField") + 1;
        if (fields.Length != 17 || travelIndex == 0)
            throw new InvalidDataException("Unsupported current shinespark layout for legacy alias restoration.");
        names.Insert(travelIndex, travelName);
        names.Add(deltaName);
        ushort travel = 0;
        sbyte delta = 0;
        foreach (string expectedName in names)
        {
            Type declaringType = resolveType(reader.ReadString());
            string actualName = reader.ReadString();
            if (declaringType != typeof(SamusShinesparkState) || actualName != expectedName)
                throw new InvalidDataException($"Legacy shinespark field {actualName} does not match {expectedName}.");
            object? value = readValue();
            if (expectedName == travelName)
                travel = value is ushort word ? word : throw new InvalidDataException("Legacy crash travel is not a word.");
            else if (expectedName == deltaName)
                delta = value is sbyte signed ? signed : throw new InvalidDataException("Legacy crash angular delta is not a signed byte.");
            else
                fields.Single(field => field.Name == expectedName).SetValue(instance, value);
        }

        // b944f1b5 kept these two words separate from the released echo coordinates.
        // The current crash handlers alias them. During a live crash preserve the
        // saved crash consumer; otherwise preserve the saved echo coordinates.
        // These contradictory old copies cannot both occupy the same native word.
        if (instance.Phase is ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle or ShinesparkPhase.CrashFinish)
        {
            typeof(SamusShinesparkState).GetProperty(nameof(SamusShinesparkState.CrashAngularTravel))!
                .SetValue(instance, travel);
            typeof(SamusShinesparkState).GetProperty("CrashAngularDelta", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(instance, (short)delta);
        }
        Console.Error.WriteLine("WARNING: Legacy shinespark stored separate crash/echo aliases; restored the active phase's owner. Conflicting inactive copies cannot be retained.");
    }
}
