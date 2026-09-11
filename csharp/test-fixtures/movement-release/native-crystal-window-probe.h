// #408: original-CPU Crystal Flash window profile and fixed-color lifetime.
// Include after native-release-probe.h; invoke before SDL initialization.
int DiagnosticCrystalPalette(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "beam,offset,frame,type,bubbleFrame,bubbleTimer,bodyOffset,colors\n");
  const uint16 beams[] = { 0, 0x1000, 3, 8 };
  const uint16 offsets[] = { 0, 4, 36 };
  for (int beam = 0; beam < 4; beam++) for (int offset = 0; offset < 3; offset++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    game_state = 8; button_config_shoot_x = 0x40; joypad1_lastkeys = 0x470;
    samus_pose = samus_prev_pose = 0x1d; samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_health = 49; samus_max_health = 99;
    samus_missiles = samus_super_missiles = samus_power_bombs = 10;
    samus_input_handler = 0xe913; equipped_beams = beams[beam];
    special_samus_palette_timer = offsets[offset];
    RunAsmCode(0x90d5a2, 0, 0, 0, 0);
    for (int frame = 0; frame <= 180; frame++) {
      if (frame == 180) samus_shine_timer = 0xffff;
      RunAsmCode(0x91db93, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%04X,%04X,%04X,%04X,", beam, offset, frame,
        timer_for_shine_timer, special_samus_palette_frame, samus_shine_timer, special_samus_palette_timer);
      for (int color = 224; color < 240; color++) fprintf(f, "%04X", palette_buffer[color]);
      fputc('\n', f);
    }
  }
  fclose(f); return 0;
}

int DiagnosticCrystalWindow(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  power_bomb_explosion_status = 0x8000;
  power_bomb_explosion_x_pos = 128; power_bomb_explosion_y_pos = 112;
  RunAsmCode(0x88a2e4, 0, 0, 0, 0);
  RunAsmCode(0x88a309, 0, 0, 0, 0);
  fprintf(f, "frame,phase,radius,speed,r,g,b,left192,right192\n");
  int phase = 1;
  for (int frame = 1; frame <= 160; frame++) {
    hdma_object_instruction_timers[0] = 0;
    RunAsmCode(phase == 1 ? 0x88a552 : 0x88a35d, 0, 0, 0, 0);
    if (hdma_object_instruction_timers[0]) {
      if (phase == 1) phase = 2;
      else { RunAsmCode(0x88a317, 0, 0, 0, 0); phase = 0; }
    }
    fprintf(f, "%d,%d,%04X,%04X,%02X,%02X,%02X,", frame, phase,
      power_bomb_explosion_radius, power_bomb_pre_explosion_radius_speed,
      reg_COLDATA[0] & 31, reg_COLDATA[1] & 31, reg_COLDATA[2] & 31);
    for (int i = 0; i < 192; i++) fprintf(f, "%02X", power_bomb_explosion_left_hdma[i]);
    fputc(',', f);
    for (int i = 0; i < 192; i++) fprintf(f, "%02X", power_bomb_explosion_right_hdma[i]);
    fputc('\n', f);
    if (!phase) break;
  }
  if (phase) { fclose(f); return 5; }
  fprintf(f, "ordinary-frame,wake,r,g,b\n");
  power_bomb_explosion_status = 0x8000;
  hdma_object_timers[0] = 0; hdma_object_D[0] = 32;
  reg_COLDATA[0] = 0x3f; reg_COLDATA[1] = 0x5f; reg_COLDATA[2] = 0x9f;
  for (int frame = 1; frame <= 32; frame++) {
    hdma_object_instruction_timers[0] = 0;
    RunAsmCode(0x888b98, 0, 0, 0, 0);
    fprintf(f, "%d,%d,%02X,%02X,%02X\n", frame, hdma_object_instruction_timers[0],
      reg_COLDATA[0] & 31, reg_COLDATA[1] & 31, reg_COLDATA[2] & 31);
  }
  fclose(f); return 0;
}
