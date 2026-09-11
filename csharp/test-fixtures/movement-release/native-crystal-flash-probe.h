// #408: execute the original bank-$88 cleanup and its bank-$90 activation call.
// Include after native-release-probe.h. No player save, GUI or translated routine.
int DiagnosticCrystalFlash(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "case,left,pose,flag,immunity,knockback,health,missiles,supers,pbs\n");
  for (int test = 0; test < 44; test++) for (int left = 0; left < 2; left++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    game_state = 8; button_config_shoot_x = 0x40; joypad1_lastkeys = 0x470;
    samus_pose = samus_prev_pose = left ? 0x41 : 0x1d;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = 4;
    samus_x_pos = power_bomb_explosion_x_pos = 128;
    samus_y_pos = power_bomb_explosion_y_pos = 128;
    samus_health = 49; samus_max_health = 99;
    samus_missiles = samus_super_missiles = samus_power_bombs = 10;
    samus_invincibility_timer = 96; samus_knockback_timer = 5;
    power_bomb_flag = 0xffff; samus_input_handler = 0xe913;
    if (test == 1) samus_health = 50;
    if (test == 2) samus_health = 51;
    if (test == 3) samus_missiles = 9;
    if (test == 4) samus_super_missiles = 9;
    if (test == 5) samus_power_bombs = 9;
    if (test == 6) samus_reserve_health = 1;
    if (test == 7) samus_y_speed = 1;
    if (test == 8) samus_y_subspeed = 1;
    if (test == 9) samus_x_pos++;
    if (test == 10) samus_y_pos++;
    if (test == 11) samus_x_subpos = 0xffff;
    if (test == 12) samus_y_subpos = 0xffff;
    if (test == 13) joypad1_lastkeys ^= 0x400;
    if (test == 14) joypad1_lastkeys |= 0x80;
    if (test == 15) samus_max_reserve_health = 100;
    if (test == 16) samus_max_power_bombs = 10;
    if (test == 17) samus_health = 0;
    // Exhaust the held-input chord independently of edge/new-input history.
    if (test >= 18 && test < 34) {
      int chord = test - 18;
      joypad1_lastkeys = ((chord & 1) ? 0x400 : 0) | ((chord & 2) ? 0x20 : 0) |
                        ((chord & 4) ? 0x10 : 0) | ((chord & 8) ? 0x40 : 0);
    }
    if (test == 34 || test == 35) {
      button_config_shoot_x = 0x8000;
      joypad1_lastkeys = test == 34 ? 0x8430 : 0x470;
    }
    if (test == 36) samus_x_pos--;
    if (test == 37) samus_y_pos--;
    if (test == 38) samus_missiles = 11;
    if (test == 39) samus_super_missiles = 11;
    if (test == 40) samus_power_bombs = 11;
    if (test == 41) samus_missiles = 0;
    if (test == 42) samus_super_missiles = 0;
    if (test == 43) samus_power_bombs = 0;
    RunAsmCode(0x888b4e, 0, 0, 0, 0);
    fprintf(f, "%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
      test,left,samus_pose,power_bomb_flag,samus_invincibility_timer,samus_knockback_timer,
      samus_health,samus_missiles,samus_super_missiles,samus_power_bombs);
  }
  fclose(f); return 0;
}

static int ProbeCrystalFlashLifetime(const char *rom, const char *output, bool contact) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  if (contact) fprintf(f, "suit,");
  fprintf(f, "left,offset,frame,phase,pose,anim,timer,y,health,missiles,supers,pbs,immunity,knockback\n");
  for (int suit = 0; suit < (contact ? 3 : 1); suit++)
  for (int left = 0; left < 2; left++) for (int offset = 0; offset < 8; offset++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    game_state = 8; button_config_shoot_x = 0x40; joypad1_lastkeys = 0x470;
    samus_pose = samus_prev_pose = left ? 0x41 : 0x1d;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = 4;
    samus_x_pos = power_bomb_explosion_x_pos = 128;
    samus_y_pos = power_bomb_explosion_y_pos = 128;
    samus_health = 49; samus_max_health = 1499;
    equipped_items = suit == 1 ? 1 : suit == 2 ? 0x20 : 0;
    samus_missiles = samus_super_missiles = samus_power_bombs = 10;
    power_bomb_flag = 0xffff; samus_input_handler = 0xe913;
    room_width_in_blocks = room_height_in_blocks = 16;
    interactive_enemy_indexes[0] = 0xffff;
    samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    RunAsmCode(0x888b4e, 0, 0, 0, 0);
    int done = 0;
    for (int frame = 0; frame < 400; frame++) {
      nmi_frame_counter_word = frame + offset;
      if (!contact) { samus_invincibility_timer = 77; samus_knockback_timer = 5; }
      if (contact && frame == 30) {
        // A deliberately admitted ordinary touch before beta; compare its damage
        // and the subsequent native movement/animation handlers, not broadphase.
        cur_enemy_index = 0;
        gEnemyData(0)->enemy_ptr = 0xd47f; // Retail Ripper header, damage five.
        gEnemyData(0)->x_pos = samus_x_pos;
        RunAsmCode(0xa0a4a1, 0, 0, 0, 0);
      }
      RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0);
      int phase = samus_movement_handler == 0xd678 ? 1 : samus_movement_handler == 0xd6ce ? 2 : samus_movement_handler == 0xd75b ? 3 : 0;
      if (contact) fprintf(f, "%d,", suit);
      fprintf(f, "%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        left,offset,frame,phase,samus_pose,samus_anim_frame,samus_anim_frame_timer,
        samus_y_pos,samus_health,samus_missiles,samus_super_missiles,samus_power_bombs,samus_invincibility_timer,samus_knockback_timer);
      if (!phase) { done = 1; break; }
    }
    if (!done) { fclose(f); return 5; }
  }
  fclose(f); return 0;
}

int DiagnosticCrystalFlashLifetime(const char *rom, const char *output) {
  return ProbeCrystalFlashLifetime(rom, output, false);
}
int DiagnosticCrystalFlashContact(const char *rom, const char *output) {
  return ProbeCrystalFlashLifetime(rom, output, true);
}
