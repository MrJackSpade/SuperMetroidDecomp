// Original $90:EAAB wrapper/EA7F checker and bank-$80 queue. Each row starts
// with constructed WRAM; no C implementation supplies the expected decisions.
static int DiagnosticHealthWarning(const char *output) {
  char path[1024];
  if (snprintf(path, sizeof(path), "%s.warning.csv", output) >= sizeof(path)) return 5;
  FILE *f = fopen(path, "wx"); if (!f) return 4;
  const int health_values[] = {0, 1, 30, 31, 32, 99, 32767, 32768, 32798, 32799, 65535};
  const int occupancy_values[] = {0, 5, 6};
  fprintf(f, "health,initialLatch,suppression,occupancy,latch,added,command,retryAdded\n");
  for (int h = 0; h < 11; h++) for (int latch = 0; latch < 2; latch++)
  for (int suppression = 0; suppression < 3; suppression++) for (int q = 0; q < 3; q++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    samus_health = health_values[h]; samus_health_warning = latch;
    power_bomb_explosion_status = suppression == 1 ? 0x8000 : 0;
    debug_disable_sounds = suppression == 2;
    sfx_writepos[2] = occupancy_values[q];
    ProbeRunBoundedRegisters(0x90eaab, 0, 0, 0);
    int added = sfx_writepos[2] - occupancy_values[q];
    int command = added ? sfx3_queue[occupancy_values[q]] : 0;
    int result_latch = samus_health_warning;
    int before_retry = sfx_writepos[2];
    sfx_readpos[2] = sfx_writepos[2];
    power_bomb_explosion_status = debug_disable_sounds = 0;
    ProbeRunBoundedRegisters(0x90eaab, 0, 0, 0);
    fprintf(f, "%d,%d,%d,%d,%d,%d,%d,%d\n", health_values[h], latch,
      suppression, occupancy_values[q], result_latch, added, command,
      sfx_writepos[2] - before_retry);
  }
  fclose(f);
  if (snprintf(path, sizeof(path), "%s.warning-handlers.csv", output) >= sizeof(path)) return 5;
  f = fopen(path, "wx"); if (!f) return 4;
  const uint32 handlers[] = {0x90e725, 0x90e8dc, 0x90e8d6, 0x90e8d9, 0x90e902, 0x90e8ec};
  fprintf(f, "handler,reachesCheck\n");
  for (int i = 0; i < 6; i++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    game_state = 8;
    ProbeRunBoundedRegisters(0x91e00d, 0, 0, 0);
    room_width_in_blocks = 16; room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int b = 256; b < 272; b++) level_data[b] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    samus_x_pos = 128; samus_y_pos = 235; samus_x_radius = 5; samus_y_radius = 21;
    samus_pose = samus_prev_pose = 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_movement_handler = 0xa337;
    samus_x_speed_table_pointer = 0x9f55;
    samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
    samus_anim_frame_timer = 5; samus_health = 30; samus_max_health = 99;
    bool reached = ProbeRunBoundedUntil(handlers[i], 0, 0, 0, 0x90ea7f);
    fprintf(f, "%06X,%d\n", handlers[i], reached ? 1 : 0);
  }
  fclose(f); return 0;
}
