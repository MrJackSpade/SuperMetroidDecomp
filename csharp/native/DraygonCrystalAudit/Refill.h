/* Inventory/admission seams: real placement, pickup collision and PB cleanup.
   The intervening explosion duration and boss flight are not simulated here. */
enum { PlacePowerBomb = 0x90bf9d, PickupCollision = 0x86efe0, PowerBombCleanup = 0x888b4e };
static void refill_pickup(int distant) {
  eproj_E[0] = 6; eproj_F[0] = 400; eproj_radius[0] = 0x0505;
  eproj_x_pos[0] = samus_x_pos + (distant ? 100 : 0); eproj_y_pos[0] = samus_y_pos;
  run(PickupCollision);
}
static void refill_row(int right, int mode, int stage) {
  printf("%d,%d,%d,%04X,%04X,%04X,%02X,%d\n",right,mode,stage,samus_power_bombs,
    samus_missiles,samus_super_missiles,samus_pose,samus_movement_handler==FlashRaising);
}
static void refill_matrix(void) {
  printf("right,mode,stage,pbs,missiles,supers,pose,flash\n");
  for (int right = 0; right < 2; right++)
  for (int mode = 0; mode < 8; mode++) {
    memset(g_ram, 0, sizeof(g_ram));
    memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
    samus_pose = samus_prev_pose = right ? 0x1d : 0x41;
    samus_pose_x_dir = samus_prev_pose_x_dir = right ? 8 : 4;
    samus_x_pos = 256; samus_y_pos = 400;
    samus_health = 49; samus_max_health = 99;
    samus_power_bombs = samus_max_power_bombs = mode == 4 ? 9 : mode == 5 ? 11 : 10;
    samus_missiles = samus_super_missiles = samus_max_missiles = samus_max_super_missiles = 10;
    button_config_shoot_x = 0x40; game_state = 8; hud_item_index = 3;
    grapple_beam_function = 0xc4f0; equipped_items = 4;
    run(RefreshRadius); refill_row(right,mode,0);
    if (mode == 1) refill_pickup(0);
    refill_row(right,mode,1);
    joypad1_lastkeys = joypad1_newkeys = 0x40;
    run(PlacePowerBomb); refill_row(right,mode,2);
    if (mode == 2 || mode == 3 || mode == 4 || mode == 6 || mode == 7) refill_pickup(mode == 3);
    refill_row(right,mode,3);
    power_bomb_explosion_x_pos = projectile_x_pos[5];
    power_bomb_explosion_y_pos = projectile_y_pos[5];
    grab(right);
    if (mode == 7) samus_x_pos++;
    joypad1_lastkeys = mode == 6 ? 0x4f0 : 0x470;
    run(PowerBombCleanup); refill_row(right,mode,4);
    if ((samus_movement_handler == FlashRaising) != (mode == 2 || mode == 5))
      Die("Refill-before-cleanup admission differs from expected successful controls");
  }
}
