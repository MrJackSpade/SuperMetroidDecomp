import importlib.util
import json
import os
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import zipfile

spec = importlib.util.spec_from_file_location("release", Path(__file__).resolve().parents[1] / "tools/release.py")
release = importlib.util.module_from_spec(spec)
spec.loader.exec_module(release)


class ReleaseTests(unittest.TestCase):
    def test_windows_package_ships_only_canonical_defaults(self):
        with tempfile.TemporaryDirectory() as root:
            source = Path(root) / "publish"
            source.mkdir()
            (source / "SuperMetroid.Game.exe").write_bytes(b"synthetic executable")
            with self.assertRaisesRegex(ValueError, "missing.*default INI"):
                release.package("classic-windows", source, Path(root) / "out")
            template = source / release.DEFAULT_INI_NAME
            template.write_bytes(release.DEFAULT_INI.read_bytes())
            release.package("classic-windows", source, Path(root) / "out")
            with zipfile.ZipFile(Path(root) / "out/SuperMetroid-Classic-windows-x64.zip") as archive:
                self.assertIn(release.DEFAULT_INI_NAME, archive.namelist())
                release.check_default_ini(archive.read(release.DEFAULT_INI_NAME))
            template.write_text(release.DEFAULT_INI.read_text().replace("Invincibility=false", "Invincibility=true"))
            with self.assertRaisesRegex(ValueError, "differs"):
                release.package("classic-windows", source, Path(root) / "out")

    def test_rebirth_windows_package_needs_its_files_and_carries_no_assets(self):
        with tempfile.TemporaryDirectory() as root:
            source = Path(root) / "export"
            source.mkdir()
            (source / "SuperMetroidRebirth.exe").write_bytes(b"synthetic executable")
            with self.assertRaisesRegex(ValueError, "missing SuperMetroidRebirthAssets.exe"):
                release.package("rebirth-windows", source, Path(root) / "out")
            (source / "SuperMetroidRebirthAssets.exe").write_bytes(b"synthetic tool")
            (source / "REBIRTH-SETUP.md").write_text("setup")
            release.package("rebirth-windows", source, Path(root) / "out")
            with zipfile.ZipFile(Path(root) / "out/SuperMetroid-Rebirth-windows-x64.zip") as archive:
                self.assertEqual(sorted(archive.namelist()), sorted(release.REBIRTH_WINDOWS_FILES))
            (source / "assets").mkdir()
            (source / "assets/room.json").write_text("{}")
            with self.assertRaisesRegex(ValueError, "never ship"):
                release.package("rebirth-windows", source, Path(root) / "out")
            with self.assertRaisesRegex(ValueError, "Unknown artifact"):
                release.package("windows", source, Path(root) / "out")

    def test_android_version_is_stamped_once_and_only_from_a_release_version(self):
        with tempfile.TemporaryDirectory() as root:
            presets = Path(root) / "export_presets.cfg"
            presets.write_text('[preset.1.options]\nversion/code=1\nversion/name="0.0.0"\npackage/signed=false\n')
            release.stamp_android(presets, "1.2.3-rc.1", "45")
            self.assertEqual(presets.read_text(), '[preset.1.options]\nversion/code=45\nversion/name="1.2.3-rc.1"\npackage/signed=false\n')
            for version, code in [("1.2", "45"), ("1.2.3", "0"), ("1.2.3", "4;x"), ('1.2.3"\nx', "4")]:
                with self.assertRaises(ValueError):
                    release.stamp_android(presets, version, code)
            presets.write_text('version/code=1\nversion/code=2\nversion/name="0.0.0"\n')
            with self.assertRaisesRegex(ValueError, "2 times"):
                release.stamp_android(presets, "1.2.3", "45")

    def test_archive_rejects_personal_ini_or_modified_defaults(self):
        for name in ["SuperMetroid.ini", "SuperMetroid.defaults.ini", "nested/SuperMetroid.defaults.ini"]:
            with tempfile.TemporaryDirectory() as root:
                path = Path(root) / "release.zip"
                with zipfile.ZipFile(path, "w") as archive:
                    archive.writestr(name, "[Game]\nInvincibility=true\n")
                with self.assertRaises(ValueError):
                    release.check_archive(path)

    def test_tags_cannot_inject_commands_or_paths(self):
        self.assertEqual(release.version("v1.2.3-rc.1"), "1.2.3-rc.1")
        for tag in ["main", "v1.2", "v01.2.3", "v1.2.3\nattack", "v1.2.3/../../x", "v1.2.3;echo secret"]:
            with self.assertRaises(ValueError):
                release.version(tag)

    def test_private_payloads_are_rejected_in_both_archives(self):
        for name in ["assets/game/SuperMetroid.smc", "assets/audio/samples/a.wav", "debug.keystore",
                     "nested/audio-manifest.json", "x/../a", "SuperMetroid.save.json", "state.smstate",
                     "logs/SuperMetroid-session.zip", "logs/SuperMetroid-session.log"]:
            with tempfile.TemporaryDirectory() as root:
                path = Path(root) / "app.apk"
                with zipfile.ZipFile(path, "w") as archive:
                    archive.writestr(name, b"test")
                with self.assertRaises(ValueError):
                    release.check_archive(path)
        release.check_names(["SuperMetroid.Game.exe", "lib/arm64-v8a/libaot-System.dll.so", "ROM-SETUP.md"])

    def test_receipt_is_replaced_without_losing_release_notes(self):
        body = "Player-facing release notes.\n"
        body = release.receipt_body(body, {"status": "pending"})
        body = release.receipt_body(body, {"status": "sent", "message_id": "42"})
        self.assertEqual(len(release.RECEIPT.findall(body)), 1)
        self.assertIn("Player-facing release notes.", body)
        self.assertEqual(json.loads(release.RECEIPT.search(body).group(1))["message_id"], "42")

    def test_mentions_disabled(self):
        self.assertEqual(release.notification_payload("v1.0.0", "https://github.com/a/b/releases/tag/v1.0.0")["allowed_mentions"], {"parse": []})

    def test_notification_records_intent_before_post_and_edits_on_rerun(self):
        info = {"id": "123", "guild_id": release.GUILD_ID, "channel_id": "456"}
        published = {"databaseId": 1, "body": "Notes", "isDraft": False, "url": "https://github.com/a/b/releases/tag/v1.0.0"}
        calls = []

        def request(url, method="GET", payload=None):
            calls.append(method)
            if method == "GET":
                return info
            if method == "POST":
                self.assertIn('"status":"pending"', published["body"])
            return {"id": "789", "channel_id": "456"}

        def body(target, value):
            target["body"] = value

        env = {"DISCORD_RELEASES_WEBHOOK_URL": "https://discord.com/api/webhooks/123/test-token",
               "DISCORD_RELEASES_CHANNEL_ID": "456", "RELEASE_TAG": "v1.0.0"}
        with patch.dict(os.environ, env), patch.object(release, "webhook_request", side_effect=request), \
                patch.object(release, "release_info", return_value=published), patch.object(release, "set_body", side_effect=body):
            release.notify()
            release.notify()
            self.assertEqual(calls, ["GET", "POST", "GET", "PATCH"])
            published["body"] = release.receipt_body("Notes", {"status": "pending"})
            with self.assertRaisesRegex(ValueError, "uncertain"):
                release.notify()
            self.assertEqual(calls[-1], "GET")

    def test_wrong_channel_cannot_receive_announcement(self):
        env = {"DISCORD_RELEASES_WEBHOOK_URL": "https://discord.com/api/webhooks/123/test-token",
               "DISCORD_RELEASES_CHANNEL_ID": "456"}
        with patch.dict(os.environ, env), patch.object(release, "webhook_request", return_value={"guild_id": release.GUILD_ID, "channel_id": "999"}) as request:
            with self.assertRaisesRegex(ValueError, "destination"):
                release.notify()
            self.assertEqual(request.call_count, 1)

    def test_receipt_update_uses_rest_database_id(self):
        published = {"id": "RE_graphql_node", "databaseId": 123, "tagName": "v1.0.0", "body": "Before"}
        with patch.dict(os.environ, {"GH_REPO": "owner/repo"}), patch.object(release, "gh") as command:
            release.set_body(published, "After")
            self.assertIn("repos/owner/repo/releases/123", command.call_args.args)
            self.assertEqual(json.loads(command.call_args.kwargs["data"]), {"tag_name": "v1.0.0", "body": "After"})


if __name__ == "__main__":
    unittest.main()
