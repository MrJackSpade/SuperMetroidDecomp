using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Diagnostics.Configuration;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Harness;

/// <summary>Checks INI editing preserves surrounding settings and supports every Android menu value.</summary>
internal static class IniEditingVerification
{
    /// <summary>Confirms existing and missing keys or sections round-trip, CRLF and comments survive, and invalid or multiline values fail before write.</summary>
    internal static void VerifyIniEditing()
    {
        string original = "; preserved comment\r\n[Audio]\r\nMasterVolumePercent=37\r\n[Game]\r\nInvincibility=false\r\n";
        string edited = HostOptionsIni.WithValue(original, "Game", "Invincibility", "true");
        AssertTrue(HostOptionsIni.Parse(edited).Game.Invincibility, "edited boolean is effective after parsing");
        AssertEqual(original.Replace("Invincibility=false", "Invincibility=true"), edited, "unrelated lines and CRLF preserved exactly");
        edited = HostOptionsIni.WithValue(edited, "game", "InfiniteAmmo", "true");
        AssertTrue(HostOptionsIni.Parse(edited).Game.InfiniteAmmo, "missing key inserted into case-insensitive section");
        AssertEqual(37, HostOptionsIni.Parse(edited).MasterVolumePercent, "unrelated audio setting retained");
        string added = HostOptionsIni.WithValue("", "Game", "MapReveal", "Secret");
        AssertEqual("Secret", HostOptionsIni.Parse(added).Game.MapReveal.ToString(), "missing section added");
        AssertThrows<System.IO.InvalidDataException>(() => HostOptionsIni.WithValue(original, "Audio", "MasterVolumePercent", "101"), "invalid value rejected before write");
        AssertThrows<ArgumentException>(() => HostOptionsIni.WithValue(original, "Game", "Invincibility", "true\nInfiniteAmmo=true"), "multiline injection rejected");
        foreach (var definition in SuperMetroid.Diagnostics.Android.AndroidSettingDefinitions.All)
        {
            foreach (string value in definition.Values)
            {
                string changed = HostOptionsIni.WithValue(original,
                    definition.Section, definition.Key, value);
                var parsed = HostOptionsIni.Parse(changed);
                AssertEqual(value, definition.Read(parsed), $"Android menu {definition.Key}={value} round trip");
                AssertTrue(changed.Contains("; preserved comment\r\n"), "Android menu retains comments");
                if (definition.Key != "MasterVolumePercent")
                    AssertEqual(37, parsed.MasterVolumePercent, "Android menu retains unrelated audio value");
            }
        }
        Console.WriteLine("INI edits: preservation, existing/missing keys, section insertion, and invalid input checks pass.");
    }

    /// <summary>Runs the focused host and handheld INI editing check.</summary>
    [VerificationCommand("--ini-edit")]
    internal static int IniEditCommand()
    {
        Suite(nameof(VerifyIniEditing), () => VerifyIniEditing());
        return 0;
    }
}
