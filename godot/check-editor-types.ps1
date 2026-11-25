# Checks that nothing made in the editor holds something that is not. Run by build.ps1; fails the build.
#
# A class marked [Tool] is made in the editor. One of its exported properties whose type is another script of the
# project (a resource, a node) is given a real instance there only if that script is marked [Tool] too: otherwise the
# editor hands it a plain placeholder, the typed property cannot take it, and the editor reports an invalid cast and
# leaves the property unset (a scene saved from the editor in that state can lose it).
#
# This reads the sources as text. It lists every exported property of a [Tool] class whose type, or the type an
# array or dictionary of it holds, is a script class of the project that is not [Tool].
# Usage: pwsh -File godot/check-editor-types.ps1
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

try {
    $root = $PSScriptRoot
    $files = @(Get-ChildItem -Path (Join-Path $root 'engine'), (Join-Path $root 'game') -Recurse -Filter '*.cs' -File)

    # Every class of the project that builds on something, by name: whether it is made in the editor, and where it is.
    $classes = @{}
    $declaration = [regex]'(?ms)((?:^\s*\[[^\]\r\n]*\]\s*\r?\n)*)^\s*public\s+(?:abstract\s+|sealed\s+)?partial\s+class\s+(\w+)\s*:\s*([\w\.]+)'
    $sources = @{}
    foreach ($file in $files) {
        $text = [System.IO.File]::ReadAllText($file.FullName)
        $sources[$file.FullName] = $text
        foreach ($match in $declaration.Matches($text)) {
            $name = $match.Groups[2].Value
            $base = $match.Groups[3].Value
            # An interface is no base to build a script on.
            if ($base -cmatch '^I[A-Z]') { continue }
            $classes[$name] = [pscustomobject]@{
                Name = $name
                Tool = $match.Groups[1].Value -match '\[Tool\]'
                File = $file.FullName
                Start = $match.Index
            }
        }
    }

    $exported = [regex]'(?ms)\[Export[^\]]*\]\s*(?:\[[^\]]*\]\s*)*public\s+([\w\.\<\>\?, ]+?)\s+(\w+)\s*\{'
    $problems = [System.Collections.Generic.List[string]]::new()
    foreach ($class in $classes.Values) {
        if (-not $class.Tool) { continue }
        $text = $sources[$class.File]

        # The class's own text: from its declaration to the next class of the file, or the file's end.
        $later = @($classes.Values | Where-Object { $_.File -eq $class.File -and $_.Start -gt $class.Start } | Sort-Object Start)
        $end = if ($later.Count -gt 0) { $later[0].Start } else { $text.Length }
        $body = $text.Substring($class.Start, $end - $class.Start)
        foreach ($property in $exported.Matches($body)) {
            foreach ($held in [regex]::Matches($property.Groups[1].Value, '[A-Za-z_]\w*')) {
                $type = $held.Value
                if (-not $classes.ContainsKey($type)) { continue }
                if ($classes[$type].Tool) { continue }
                $where = $class.File.Substring($root.Length + 1).Replace('\', '/')
                $problems.Add("godot/${where}: $($class.Name) is made in the editor ([Tool]) and holds `"$($property.Groups[2].Value)`" of the type $type, which is not: mark $type [Tool] as well.")
            }
        }
    }

    if ($problems.Count -gt 0) {
        $problems | Sort-Object -Unique | ForEach-Object { [Console]::Error.WriteLine($_) }
        [Console]::Error.WriteLine("$($problems.Count) exported propert$(if ($problems.Count -eq 1) { 'y' } else { 'ies' }) of classes made in the editor hold a type that is not made there.")
        exit 1
    }

    Write-Output "Editor types: $(@($classes.Values | Where-Object Tool).Count) classes made in the editor hold nothing that is not."
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
