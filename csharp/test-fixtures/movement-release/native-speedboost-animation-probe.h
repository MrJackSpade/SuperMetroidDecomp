// #472: queue-dependent final-stage animation transition on the untouched CPU.
// Include after native-release-probe.h; dispatch before SDL starts.
int DiagnosticSpeedBoostAnimation(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "occupancy,suppression,counter,frame,timer,writeIndex,slotValue\n");
  for (int occupancy = 0; occupancy < 16; occupancy++) {
    for (int suppression = 0; suppression < 4; suppression++) {
      cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
      g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
      sfx_writepos[2] = occupancy;
      debug_disable_sounds = suppression == 1;
      game_state = suppression == 2 ? 0x28 : 8;
      power_bomb_explosion_status = suppression == 3 ? 0x8000 : 0;
      equipped_items = 0x2000; samus_has_momentum_flag = 1;
      samus_movement_type = 1;
      joypad1_lastkeys = button_config_run_b = 0x8000;
      speed_boost_counter = 0x0301;
      samus_anim_frame = 10; samus_anim_frame_timer = 1;
      RunAsmCode(0x90852c, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%04X,%04X,%04X,%d,%d\n", occupancy, suppression,
        speed_boost_counter, samus_anim_frame, samus_anim_frame_timer,
        sfx_writepos[2], sfx3_queue[occupancy]);
    }
  }
  fclose(f);
  return 0;
}
