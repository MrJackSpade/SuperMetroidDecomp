// #472: observe the real normal-touch callback before the later hurt interruption.
// Include after native-release-probe.h and dispatch before SDL initialization.
int DiagnosticEnemyContact(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  for (int left = 0; left < 2; left++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_handler = 0xa337;
    samus_health = 99; samus_x_pos = 128; samus_y_pos = 160;
    EnemyData *enemy = gEnemyData(0);
    enemy->enemy_ptr = 0xd47f; // Retail Ripper header, not fabricated damage data.
    enemy->x_pos = 128; enemy->y_pos = 160;
    RunAsmCode(0xa0a4a1, 0, 0, 0, 0);
    printf("CONTACT left=%d health=%u pose=%02X timer=%u invincibility=%u direction=%u handler=%04X\n",
      left, samus_health, samus_pose, samus_knockback_timer,
      samus_invincibility_timer, knockback_dir, samus_movement_handler);
  }
  for (int suit = 0; suit < 3; suit++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    equipped_items = suit == 0 ? 0 : suit == 1 ? 1 : 0x20;
    samus_pose = samus_prev_pose = 1; samus_pose_x_dir = 8;
    samus_movement_handler = 0xa337; samus_health = 99;
    samus_x_pos = samus_y_pos = 120;
    eproj_id[0] = 0x9642; eproj_properties[0] = 20;
    eproj_x_pos[0] = eproj_y_pos[0] = 120;
    RunAsmCode(0xa09923, 0, 0, 0, 0);
    printf("PROJECTILE suit=%04X health=%u pose=%02X timer=%u direction=%u handler=%04X id=%04X\n",
      equipped_items, samus_health, samus_pose, samus_knockback_timer,
      knockback_dir, samus_movement_handler, eproj_id[0]);
  }
  return 0;
}
