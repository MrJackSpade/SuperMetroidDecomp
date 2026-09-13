using System.Globalization;
using System.Text.Json;

/// <summary>Strict numeric trace comparison for #441; no cartridge-derived golden data is embedded.</summary>
internal static class MetroidFarmingComparison
{
    public static int Run(string managedPath, string nativePath)
    {
        using var native = File.ReadLines(nativePath).GetEnumerator();
        if (!native.MoveNext()) throw new InvalidDataException("Missing native CSV header.");
        var columns = native.Current.Split(',').Select((name, index) => (name, index))
            .ToDictionary(p => p.name, p => p.index, StringComparer.Ordinal);
        var counts = new Dictionary<string, int>();
        int frame = 0, failures = 0;
        foreach (string line in File.ReadLines(managedPath))
        {
            if (!native.MoveNext()) throw new InvalidDataException("Native trace ended early.");
            string[] row = native.Current.Split(',');
            if (row.Length != columns.Count) throw new InvalidDataException("Malformed numeric CSV row.");
            using var json = JsonDocument.Parse(line);
            JsonElement m = json.RootElement;
            if (m.GetProperty("Frame").GetInt32() != frame || Read("frame") != frame)
                throw new InvalidDataException("Non-contiguous frame order.");
            Compare(m, "Input", "input"); Compare(m, "XFixed", "x"); Compare(m, "YFixed", "y");
            Compare(m, "Pose", "pose"); Compare(m, "Health", "health"); Compare(m, "PowerBombs", "ammo");
            Compare(m, "EnemyHealth", "ehp"); Compare(m, "EnemyDefinitionPointer", "header");
            Compare(m, "XPosition", "ex"); Compare(m, "YPosition", "ey");
            Compare(m, "InvincibilityTimer", "invincible"); Compare(m, "EnemiesKilled", "killed");
            Compare(m, "RandomNumber", "rng");
            Compare(m, "SamusRadiusX", "srx"); Compare(m, "SamusRadiusY", "sry");
            JsonElement p = m.GetProperty("PowerBomb");
            Compare(p, "Flag", "pbflag"); Compare(p, "Status", "pbstatus");
            Compare(p, "XPosition", "pbx"); Compare(p, "YPosition", "pby");
            Compare(p, "PreExplosionRadius", "preRadius"); Compare(p, "ExplosionRadius", "radius");
            Compare(p, "RadiusSpeed", "speed");
            JsonElement drops = m.GetProperty("Drops");
            if (drops.GetArrayLength() != 18) throw new InvalidDataException("Expected all eighteen projectile slots.");
            for (int slot = 0; slot < 18; slot++)
            {
                JsonElement d = drops[slot];
                Compare(d, "Kind", $"kind{slot}");
                // C# empty slots are cleared objects; native deletion clears only ID.
                // Compare every slot's lifetime, but do not call stale inactive WRAM
                // a live pickup position/list. This is not a raw WRAM equivalence test.
                if (d.GetProperty("Kind").GetInt32() == 0 && Read($"kind{slot}") == 0) continue;
                Compare(d, "XPosition", $"px{slot}");
                Compare(d, "YPosition", $"py{slot}"); Compare(d, "InstructionPointer", $"list{slot}");
                Compare(d, "InstructionTimer", $"timer{slot}"); Compare(d, "PreInstruction", $"pre{slot}");
                Compare(d, "Variable0", $"value{slot}"); Compare(d, "EnemyHeaderPointer", $"header{slot}");
                if (slot == 17) { Compare(d, "XRadius", "prx"); Compare(d, "YRadius", "pry"); }
            }
            frame++;

            long Read(string name) => long.Parse(row[columns[name]], CultureInfo.InvariantCulture);
            void Compare(JsonElement owner, string property, string column)
            {
                long actual = owner.GetProperty(property).GetInt64(), expected = Read(column);
                if (actual == expected) return;
                if (failures++ < 25) Console.WriteLine($"Frame {frame} {column}: managed={actual} native={expected}");
                counts[column] = counts.GetValueOrDefault(column) + 1;
            }
        }
        if (frame != 1050 || native.MoveNext()) throw new InvalidDataException("Expected exactly 1050 frames in both traces.");
        if (failures != 0)
        {
            foreach (var pair in counts) Console.WriteLine($"{pair.Key}: {pair.Value} mismatching frames");
            throw new InvalidDataException($"{failures} mismatches; parity not established.");
        }
        Console.WriteLine("All 1050 frames match movement, radii, resources, Power Bomb state, Metroid damage/death, RNG, all projectile lifetimes and active pickup fields.");
        return 0;
    }
}
