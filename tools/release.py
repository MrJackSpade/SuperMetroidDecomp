"""ROM-free packaging, GitHub release publication, and receipted Discord announcements."""
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import urllib.error
import urllib.parse
import urllib.request
import zipfile

TAG = re.compile(r"v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-([0-9A-Za-z]+(?:[.-][0-9A-Za-z]+)*))?\Z")
RECEIPT = re.compile(r"\n?<!-- discord-release: (\{[^\n]*\}) -->")
GUILD_ID = "1547270817312809112"


def version(tag):
    if not TAG.fullmatch(tag):
        raise ValueError("Release tags must be vMAJOR.MINOR.PATCH, optionally followed by -prerelease.")
    return tag[1:]


def check_names(names):
    for name in names:
        normalized = name.replace("\\", "/").lower()
        parts = normalized.split("/")
        if (any(p in {"..", "standalone-assets", "debug-states", "input-recordings"} for p in parts)
                or normalized.startswith("/")
                or normalized.endswith((".smc", ".sfc", ".spcu", ".wav", ".smstate", ".smrec", ".srm", ".keystore", ".jks"))
                or parts[-1] in {"audio-manifest.json", "installation.json", "supermetroid.save.json", "supermetroid.ini"}):
            raise ValueError(f"Release contains private game data or signing material: {name}")


def check_archive(path):
    with zipfile.ZipFile(path) as archive:
        check_names(archive.namelist())
        if archive.testzip() is not None:
            raise ValueError(f"Corrupt release archive: {path.name}")


def package(kind, source, output):
    source, output = Path(source), Path(output)
    output.mkdir(parents=True, exist_ok=True)
    if kind == "windows":
        files = [p for p in source.rglob("*") if p.is_file()]
        check_names(str(p.relative_to(source)) for p in files)
        if not (source / "SuperMetroid.Game.exe").is_file():
            raise ValueError("Desktop publish is missing SuperMetroid.Game.exe.")
        target = output / "SuperMetroid-windows-x64.zip"
        with zipfile.ZipFile(target, "w", zipfile.ZIP_DEFLATED) as archive:
            for path in sorted(files):
                archive.write(path, path.relative_to(source))
    else:
        check_archive(source)
        target = output / "SuperMetroid-android-arm64.apk"
        shutil.copyfile(source, target)
    check_archive(target)
    print(f"Verified game-data-free artifact: {target.name}")


def gh(*args, data=None, optional=False):
    result = subprocess.run(["gh", *args], input=data, text=True, capture_output=True)
    if result.returncode:
        if optional and "not found" in result.stderr.lower():
            return None
        raise RuntimeError(f"GitHub command failed ({args[0]}): {result.stderr.strip()}")
    return result.stdout


def release_info(tag):
    result = gh("release", "view", tag, "--json", "databaseId,tagName,body,url,isDraft,assets", optional=True)
    return json.loads(result) if result else None


def set_body(release, body):
    repo = os.environ["GH_REPO"]
    gh("api", "--method", "PATCH", f"repos/{repo}/releases/{release['databaseId']}", "--input", "-",
       data=json.dumps({"tag_name": release["tagName"], "body": body}))
    release["body"] = body


def publish(directory):
    tag = os.environ["RELEASE_TAG"]
    version(tag)
    root = Path(directory)
    assets = [root / "SuperMetroid-windows-x64.zip", root / "SuperMetroid-android-arm64.apk"]
    for asset in assets:
        check_archive(asset)
    checksums = root / "SHA256SUMS.txt"
    checksums.write_text("".join(f"{hashlib.sha256(p.read_bytes()).hexdigest()}  {p.name}\n" for p in assets), encoding="utf-8")
    assets.append(checksums)
    release = release_info(tag)
    if release and not release["isDraft"]:
        # Published versions are immutable. A rerun can retry the announcement without replacing binaries.
        required = {p.name for p in assets}
        if not required.issubset({a["name"] for a in release["assets"]}):
            raise ValueError("Published release is missing required assets; inspect it before retrying.")
        print(f"Release already published: {release['url']}")
        return
    if not release:
        gh("release", "create", tag, "--verify-tag", "--draft", "--title", tag, "--generate-notes")
        release = release_info(tag)
        set_body(release, (release["body"] or "") + "\n\nDownloads: Windows x64 (self-contained ZIP) and Android ARM64 APK. "
                 "Supply your own Super Metroid Japan/USA NTSC v1.0 ROM on first launch. No game assets are bundled.\n")
    gh("release", "upload", tag, *(str(p) for p in assets), "--clobber")
    args = ["release", "edit", tag, "--tag", tag, "--draft=false"]
    args.append("--prerelease" if "-" in tag else "--latest")
    gh(*args)
    print(f"Published {tag} with both verified applications and SHA-256 checksums.")


def webhook_request(url, method="GET", payload=None):
    request = urllib.request.Request(url, method=method, headers={"Content-Type": "application/json", "User-Agent": "SuperMetroid-Release/1.0"},
                                     data=json.dumps(payload).encode() if payload is not None else None)
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            return json.load(response)
    except urllib.error.HTTPError as error:
        # URLs contain the credential. Never print the exception, request URL, or response body.
        raise RuntimeError(f"Discord request failed with HTTP {error.code}; inspect the release receipt before retrying.") from None
    except (urllib.error.URLError, TimeoutError):
        raise RuntimeError("Discord delivery could not be confirmed; inspect the release receipt before retrying.") from None


def receipt_body(body, receipt):
    return RECEIPT.sub("", body).rstrip() + "\n\n<!-- discord-release: " + json.dumps(receipt, separators=(",", ":")) + " -->\n"


def notification_payload(tag, url):
    version(tag)
    return {"content": f"**Super Metroid {tag} is available**\nWindows x64 and Android ARM64 downloads:\n{url}\n"
            "Supply your own ROM on first launch.", "allowed_mentions": {"parse": []}, "flags": 4}


def notify():
    url = os.environ.get("DISCORD_RELEASES_WEBHOOK_URL", "").strip()
    channel = os.environ.get("DISCORD_RELEASES_CHANNEL_ID", "").strip()
    if not url or not channel:
        raise ValueError("Configure DISCORD_RELEASES_WEBHOOK_URL secret and DISCORD_RELEASES_CHANNEL_ID variable. The GitHub release is already published.")
    parsed = urllib.parse.urlsplit(url)
    if (parsed.scheme != "https" or parsed.netloc != "discord.com" or parsed.query or parsed.fragment
            or not re.fullmatch(r"/api/webhooks/\d+/[A-Za-z0-9_-]+", parsed.path)):
        raise ValueError("Expected a standard discord.com webhook URL.")
    info = webhook_request(url)
    if str(info.get("guild_id")) != GUILD_ID or str(info.get("channel_id")) != channel:
        raise ValueError("Release webhook destination does not match the configured server/channel.")
    tag = os.environ["RELEASE_TAG"]
    release = release_info(tag)
    if not release or release["isDraft"]:
        raise ValueError("Only a published release can be announced.")
    match = RECEIPT.search(release["body"] or "")
    receipt = json.loads(match.group(1)) if match else None
    payload = notification_payload(tag, release["url"])
    if receipt:
        if receipt.get("status") != "sent":
            raise ValueError("An earlier Discord attempt has uncertain delivery. Reconcile its release-body receipt before retrying; see docs/releases.md.")
        message_id = str(receipt.get("message_id", ""))
        if not message_id.isdigit() or receipt.get("webhook_id") != str(info["id"]):
            raise ValueError("Discord receipt does not match this webhook.")
        message = webhook_request(url + "/messages/" + message_id, "PATCH", payload)
    else:
        receipt = {"status": "pending", "webhook_id": str(info["id"]), "run_id": os.environ.get("GITHUB_RUN_ID", "local")}
        set_body(release, receipt_body(release["body"] or "", receipt))
        message = webhook_request(url + "?wait=true", "POST", payload)
    if str(message.get("channel_id")) != channel or not str(message.get("id", "")).isdigit():
        raise ValueError("Discord response did not confirm delivery in the releases channel.")
    receipt.update(status="sent", message_id=str(message["id"]))
    set_body(release, receipt_body(release["body"] or "", receipt))
    print(f"Discord release announcement confirmed: message {message['id']}.")


def main():
    command = sys.argv[1]
    if command == "version":
        run = int(os.environ["RELEASE_RUN"])
        tag = os.environ["RELEASE_REF"].removeprefix("refs/tags/") if os.environ["RELEASE_EVENT"] == "push" else f"v0.0.0-ci.{run}"
        values = f"tag={tag}\nversion={version(tag)}\ncode={run + 1}\n"
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as output:
            output.write(values)
    elif command.startswith("package-"):
        package(command.removeprefix("package-"), *sys.argv[2:])
    elif command == "publish":
        publish(sys.argv[2])
    elif command == "notify":
        notify()
    else:
        raise ValueError("Unknown release command.")


if __name__ == "__main__":
    try:
        main()
    except Exception as error:
        print(f"Release error: {error}", file=sys.stderr)
        sys.exit(1)
