using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static partial class Program
{
    /// <summary>Finite source-inventoried palette formats; no gameplay or ROM-read discovery.</summary>
    private static void VerifyPaletteJsonContracts()
    {
        var accepted = new List<string>();
        int rejected = 0;
        foreach (PaletteJsonContract contract in PaletteJsonContracts())
        {
            JsonObject root = contract.Document;
            LoadPaletteContract(contract, root.ToJsonString());
            // Only the two historically case-insensitive formats accept this spelling.
            string historical = root.ToJsonString().Replace("\"version\"", "\"VERSION\"", StringComparison.Ordinal);
            if (contract.CaseInsensitive) LoadPaletteContract(contract, historical);
            else Reject(historical, "case-sensitive schema rejects an unknown spelling");

            Reject("{\"version\":999," + root.ToJsonString()[1..], "duplicate version");
            Reject("{\"VERSION\":999," + root.ToJsonString()[1..], "aliased version");
            Reject(Mutate(root, copy => copy["physics"] = 7), "unknown root field");
            Reject(Mutate(root, copy => copy.Remove("version")), "missing version");
            Reject(Mutate(root, copy => copy["version"] = 999), "unsupported version");
            Reject("null", "null document");
            Reject("{", "malformed JSON");

            // Test every named family, and a representative row/color within every
            // family. Array shape and RGB bounds belong to the production compiler.
            foreach (var field in root.Where(pair => pair.Key != "version"))
            {
                string name = field.Key;
                Reject(Mutate(root, copy => copy.Remove(name)), $"missing {name}");
                Reject(Mutate(root, copy => copy[name] = null), $"null {name}");
            }
            foreach (string[] path in PaletteContractArrayPaths(root))
            {
                Reject(Mutate(root, copy => SetPaletteContractNode(copy, path, new JsonArray())),
                    $"empty {string.Join('.', path)}");
                Reject(Mutate(root, copy => SetPaletteContractNode(copy, path, null)),
                    $"null {string.Join('.', path)}");
            }
            foreach (string[] path in PaletteContractColorPaths(root))
            {
                foreach (string channel in new[] { "red", "green", "blue" })
                {
                    foreach (int invalid in new[] { -1, 32 })
                        Reject(Mutate(root, copy => ((JsonObject)PaletteContractNode(copy, path)!)[channel] = invalid),
                            $"RGB5 {string.Join('.', path)} {channel}={invalid}");
                    Reject(Mutate(root, copy => ((JsonObject)PaletteContractNode(copy, path)!).Remove(channel)),
                        $"missing {string.Join('.', path)} {channel}");
                }
                Reject(Mutate(root, copy => ((JsonObject)PaletteContractNode(copy, path)!)["alpha"] = 255),
                    $"unknown color field {string.Join('.', path)}");
            }
            string json = root.ToJsonString();
            Reject(json.Replace("\"red\":", "\"red\":32,\"red\":", StringComparison.Ordinal), "duplicate color component");
            Reject(json.Replace("\"red\":", "\"RED\":32,\"red\":", StringComparison.Ordinal), "aliased color component");
            if (root["world"] is JsonObject world)
                foreach (var entry in world)
                    Reject(json.Replace($"\"{entry.Key}\":", $"\"{entry.Key}\":null,\"{entry.Key}\":", StringComparison.Ordinal),
                        $"duplicate world selection {entry.Key}");
            if (contract.Name == "map cycle")
            {
                Reject(Mutate(root, copy => ((JsonObject)copy["frames"]![0]!).Remove("durationTicks")), "missing frame duration");
                foreach (int duration in new[] { 0, 255 })
                    Reject(Mutate(root, copy => copy["frames"]![0]!["durationTicks"] = duration), "invalid frame duration");
                Reject(json.Replace("\"durationTicks\":", "\"durationTicks\":0,\"durationTicks\":", StringComparison.Ordinal),
                    "duplicate frame duration");
                Reject(Mutate(root, copy => copy["frames"]![0]!["attack"] = 7), "unknown frame field");
            }

            void Reject(string json, string description)
            {
                try { LoadPaletteContract(contract, json); }
                catch (InvalidDataException) { rejected++; return; }
                accepted.Add($"{contract.Name}: {description}");
            }
        }
        AssertEqual(0, accepted.Count, "palette compilers must reject ambiguous/corrupt data:\n" + string.Join('\n', accepted));
        VerifyPaletteContractLegacyOverrides();
        VerifyPaletteContractVisibleIsolation();
        Console.WriteLine($"PASS palette JSON contracts: 12 formats / 18 selections, {rejected} invalid documents, " +
            "legacy overrides and exact visible palette/animation isolation.");
    }

    private sealed record PaletteJsonContract(string Name, JsonObject Document, Action<Stream> Load,
        bool CaseInsensitive = false);

    private static JsonObject PaletteContractDocument(object document) => (JsonObject)JsonSerializer.SerializeToNode(
        document, document.GetType(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;

    private static void LoadPaletteContract(PaletteJsonContract contract, string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json), writable: false);
        contract.Load(stream);
    }

    private static string Mutate(JsonObject source, Action<JsonObject> mutation)
    {
        var copy = (JsonObject)source.DeepClone();
        mutation(copy);
        return copy.ToJsonString();
    }

    private static JsonNode? PaletteContractNode(JsonNode root, string[] path)
    {
        JsonNode? node = root;
        foreach (string part in path) node = node is JsonArray ? node[int.Parse(part)] : node![part];
        return node;
    }

    private static void SetPaletteContractNode(JsonNode root, string[] path, JsonNode? value)
    {
        JsonNode parent = PaletteContractNode(root, path[..^1])!;
        if (parent is JsonArray array) array[int.Parse(path[^1])] = value;
        else parent[path[^1]] = value;
    }

    private static IEnumerable<string[]> PaletteContractArrayPaths(JsonNode? node, string[]? path = null)
    {
        path ??= [];
        if (node is JsonArray array)
        {
            yield return path;
            foreach (string[] child in PaletteContractArrayPaths(array[0], [.. path, "0"])) yield return child;
        }
        else if (node is JsonObject obj)
            foreach (var pair in obj)
                foreach (string[] child in PaletteContractArrayPaths(pair.Value, [.. path, pair.Key])) yield return child;
    }

    private static IEnumerable<string[]> PaletteContractColorPaths(JsonNode? node, string[]? path = null)
    {
        path ??= [];
        if (node is JsonObject obj)
        {
            if (obj.ContainsKey("red")) yield return path;
            else foreach (var pair in obj)
                foreach (string[] child in PaletteContractColorPaths(pair.Value, [.. path, pair.Key])) yield return child;
        }
        else if (node is JsonArray array)
            foreach (string[] child in PaletteContractColorPaths(array[0], [.. path, "0"])) yield return child;
    }
}
