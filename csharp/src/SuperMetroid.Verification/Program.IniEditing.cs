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
        Console.WriteLine("INI edits: preservation, existing/missing keys, section insertion, and invalid input checks pass.");
    }
}
