using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    private static void VerifyIniEditing()
    {
        string original = "; preserved comment\r\n[Audio]\r\nMasterVolumePercent=37\r\n[Game]\r\nInvincibility=false\r\n";
        string edited = SuperMetroidGameOptionsIni.WithValue(original, "Game", "Invincibility", "true");
        AssertTrue(SuperMetroidGameOptionsIni.Parse(edited).Invincibility, "edited boolean is effective after parsing");
        AssertEqual(original.Replace("Invincibility=false", "Invincibility=true"), edited, "unrelated lines and CRLF preserved exactly");
        edited = SuperMetroidGameOptionsIni.WithValue(edited, "game", "InfiniteAmmo", "true");
        AssertTrue(SuperMetroidGameOptionsIni.Parse(edited).InfiniteAmmo, "missing key inserted into case-insensitive section");
        AssertEqual(37, SuperMetroidGameOptionsIni.Parse(edited).MasterVolumePercent, "unrelated audio setting retained");
        string added = SuperMetroidGameOptionsIni.WithValue("", "Game", "MapReveal", "Secret");
        AssertEqual("Secret", SuperMetroidGameOptionsIni.Parse(added).MapReveal.ToString(), "missing section added");
        AssertThrows<System.IO.InvalidDataException>(() => SuperMetroidGameOptionsIni.WithValue(original, "Audio", "MasterVolumePercent", "101"), "invalid value rejected before write");
        AssertThrows<ArgumentException>(() => SuperMetroidGameOptionsIni.WithValue(original, "Game", "Invincibility", "true\nInfiniteAmmo=true"), "multiline injection rejected");
        // Exercise the actual handheld menu catalog, not a parallel list of settings.
        // Every selectable value must round-trip through the same parser the host loads.
        foreach (var definition in SuperMetroid.Android.AndroidSettingDefinitions.All)
        {
            foreach (string value in definition.Values)
            {
                string changed = SuperMetroidGameOptionsIni.WithValue(original,
                    definition.Section, definition.Key, value);
                var parsed = SuperMetroidGameOptionsIni.Parse(changed);
                AssertEqual(value, definition.Read(parsed), $"Android menu {definition.Key}={value} round trip");
                AssertTrue(changed.Contains("; preserved comment\r\n"), "Android menu retains comments");
                if (definition.Key != "MasterVolumePercent")
                    AssertEqual(37, parsed.MasterVolumePercent, "Android menu retains unrelated audio value");
            }
        }
        Console.WriteLine("INI edits: preservation, existing/missing keys, section insertion, and invalid input checks pass.");
    }
}
