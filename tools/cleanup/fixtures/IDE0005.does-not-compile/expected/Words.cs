using System.Text;

namespace Fixture;

/// <summary>Joins words, in a project that does not compile: its unnecessary using goes all the same.</summary>
public static class Words
{
    /// <summary>The words joined by spaces.</summary>
    public static string Joined(string[] words)
    {
        var text = new StringBuilder();
        foreach (string word in words)
            text.Append(word).Append(' ');
        return text.ToString().TrimEnd();
    }
}
