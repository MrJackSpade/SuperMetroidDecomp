"""Regenerate the #628 spatial data-clump inventory from an analyzer build log.

Usage:
    dotnet build csharp/SuperMetroid.Full.slnx -c Release --no-incremental > build.log
    python tools/spatial-clump-inventory.py build.log

Every SME628x diagnostic in the log becomes one open finding in
docs/spatial-clump-inventory.json, grouped by the type that owns the clump (member clumps) or
the method whose signature carries it (parameter clumps). Resolved groups are kept from the
existing inventory: each records the spatial type that now owns the value, its coordinate
space, and the boundary that splits it into components, if any.
"""
import collections
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INVENTORY = ROOT / "docs" / "spatial-clump-inventory.json"
BACKSLASH = chr(92)
DIAGNOSTIC = re.compile(
    r"([A-Za-z]:" + re.escape(BACKSLASH) + r"[^(]+)\((\d+),(\d+)\): (?:warning|error) (SME628\d): (.*?)( \[.*\])?$")
RULES = {
    "SME6280": "spatial value stored as parallel components",
    "SME6281": "spatial value passed as parallel parameters",
    "SME6282": "spatial values stored as parallel collections",
    "SME6283": "spatial value rebuilt component by component",
}


def owner_of(rule, message, path):
    patterns = {
        "SME6280": r" of (\w+) are the ",
        "SME6281": r" of ([\w.]+) are the ",
        "SME6282": r" of (\w+) hold the ",
    }
    if rule in patterns:
        match = re.search(patterns[rule], message)
        return match.group(1) if match else "unknown"
    # SME6283 names no owner type: group component arithmetic by file.
    return path.rsplit("/", 1)[-1]


def main(log_path):
    findings = {}
    for line in Path(log_path).read_text(encoding="utf-8", errors="replace").splitlines():
        match = DIAGNOSTIC.search(line.strip())
        if not match:
            continue
        path, line_number, column, rule, message, _ = match.groups()
        relative = path.split("SuperMetroidDecomp" + BACKSLASH, 1)[-1].replace(BACKSLASH, "/")
        findings[(relative, int(line_number), int(column), rule)] = message

    previous = json.loads(INVENTORY.read_text(encoding="utf-8")) if INVENTORY.exists() else {}
    groups = collections.defaultdict(list)
    for (path, line_number, column, rule), message in sorted(findings.items()):
        groups[owner_of(rule, message, path)].append({
            "file": path, "line": line_number, "column": column,
            "rule": rule, "message": message, "status": "open",
        })
    inventory = {
        "ticket": 628,
        "rules": RULES,
        "taxonomy": "docs/spatial-types.md",
        "open": {name: entries for name, entries in sorted(groups.items())},
        "resolved": previous.get("resolved", {}),
        "summary": {
            "openFindings": len(findings),
            "openGroups": len(groups),
            "byRule": dict(collections.Counter(rule for (_, _, _, rule) in findings)),
        },
    }
    INVENTORY.write_text(json.dumps(inventory, indent=1) + "\n", encoding="utf-8")
    print(json.dumps(inventory["summary"]))


if __name__ == "__main__":
    if len(sys.argv) != 2:
        raise SystemExit("usage: spatial-clump-inventory.py BUILD_LOG")
    main(sys.argv[1])
