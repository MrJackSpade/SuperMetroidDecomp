namespace SuperMetroid.Core.Frontend;

public static partial class SuperMetroidGameOptionsIni
{
    /// <summary>Updates one validated setting while preserving unrelated lines and comments.</summary>
    public static string WithValue(string contents, string section, string key, string value)
    {
        ArgumentNullException.ThrowIfNull(contents);
        foreach (string token in new[] { section, key, value })
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            if (token.IndexOfAny(['\r', '\n', '[', ']', '=']) >= 0)
                throw new ArgumentException("An INI edit must contain a single section, key and value.");
        }
        _ = Parse(contents);
        string newline = contents.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = contents.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n').ToList();
        bool inSection = false, replaced = false;
        int sectionLine = -1;
        for (int index = 0; index < lines.Count; index++)
        {
            string line = lines[index].Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                inSection = line[1..^1].Trim().Equals(section, StringComparison.OrdinalIgnoreCase);
                if (inSection) sectionLine = index;
            }
            if (!inSection || line.StartsWith(';') || line.StartsWith('#')) continue;
            int equals = line.IndexOf('=');
            if (equals <= 0 || !line[..equals].Trim().Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
            lines[index] = key + "=" + value;
            replaced = true;
            break;
        }
        if (!replaced)
        {
            if (sectionLine >= 0) lines.Insert(sectionLine + 1, key + "=" + value);
            else { lines.Add("[" + section + "]"); lines.Add(key + "=" + value); }
        }
        string result = string.Join(newline, lines);
        _ = Parse(result);
        return result;
    }
}
