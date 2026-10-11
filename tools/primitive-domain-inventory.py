"""Regenerate the #627 primitive-domain inventory from an analyzer build log.

Usage:
    dotnet build csharp/SuperMetroid.Full.slnx -c Release --no-incremental > build.log
    python tools/primitive-domain-inventory.py build.log

Every SME6270-SME6279 diagnostic in the log becomes one open finding in
docs/primitive-domain-inventory.json, grouped by the domain the analyzer inferred.
Resolved domains are kept from the existing inventory: each records the type that now
owns the domain, the boundary that decodes it, and how invalid values are handled.
"""
import collections
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INVENTORY = ROOT / "docs" / "primitive-domain-inventory.json"
BACKSLASH = chr(92)
DIAGNOSTIC = re.compile(
    r"([A-Za-z]:" + re.escape(BACKSLASH) + r"[^(]+)\((\d+),(\d+)\): warning (SME627\d): (.*?)( \[.*\])?$")
RULES = {
    "SME6270": "closed domain switched as a primitive",
    "SME6271": "closed-domain switch ignores unexpected values",
    "SME6272": "masked primitive used as a selector",
    "SME6273": "enum interpolated with a numeric format specifier",
    "SME6274": "closed domain compared as a primitive",
    "SME6275": "primitive parameter only cast to a domain enum",
    "SME6276": "arithmetic produces a closed-domain value",
    "SME6277": "closed-domain value carried as a primitive",
    "SME6278": "closed domain stored as literals",
    "SME6279": "masked primitive compared as a selector",
}


def domain_of(rule, message):
    patterns = {
        "SME6275": r"unchecked cast to (\w+);",
        "SME6276": r"undefined (\w+);",
        "SME6277": r"holds a (\w+) narrowed",
        "SME6278": r"^'(\w+)'",
        "SME6279": r"^Local '(\w+)'",
    }
    if rule in patterns:
        match = re.search(patterns[rule], message)
        return match.group(1) if match else "unknown"
    if rule in ("SME6270", "SME6274"):
        match = re.search(r"named constants of (.*?); model", message)
        return match.group(1) if match else "unknown"
    if rule == "SME6271":
        match = re.match(r"Switch over (\S+)", message)
        return match.group(1) if match else "unknown"
    match = re.match(r"Local '(\w+)'", message)
    return "masked selector " + (match.group(1) if match else "unknown")


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
    domains = collections.defaultdict(list)
    for (path, line_number, column, rule), message in sorted(findings.items()):
        domains[domain_of(rule, message)].append({
            "file": path, "line": line_number, "column": column,
            "rule": rule, "message": message, "status": "open",
        })
    inventory = {
        "ticket": 627,
        "rules": RULES,
        "open": {name: entries for name, entries in sorted(domains.items())},
        "resolved": previous.get("resolved", {}),
        "summary": {
            "openFindings": len(findings),
            "openDomains": len(domains),
            "byRule": dict(collections.Counter(rule for (_, _, _, rule) in findings)),
        },
    }
    INVENTORY.write_text(json.dumps(inventory, indent=1) + "\n", encoding="utf-8")
    print(json.dumps(inventory["summary"]))


if __name__ == "__main__":
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    main(sys.argv[1])
