/* #434: actual Flash admission followed by post-message suit setup/HDMA. */
enum { FlashVariaSetup = 0x91d4e4, FlashGravitySetup = 0x91d5ba,
       FlashVariaHdma = 0x88e026, FlashGravityHdma = 0x88e05c };
static void suit_entry_matrix(void) {
  printf("right,gravity,step,counter,suitsubstate,lightbeam,shine,palette\n");
  for (int right = 0; right < 2; right++)
  for (int gravity = 0; gravity < 2; gravity++) {
    memset(g_ram, 0, sizeof(g_ram));
    memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
    samus_pose = samus_prev_pose = right ? 1 : 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = right ? 8 : 4;
    samus_x_pos = 256; samus_y_pos = 400;
    samus_health = 49; samus_max_health = 99;
    samus_missiles = samus_super_missiles = samus_power_bombs = 10;
    button_config_shoot_x = 0x40; joypad1_lastkeys = 0x470; game_state = 8;
    run(RefreshRadius); run(FlashEntry);
    if (samus_movement_handler != FlashRaising || substate != 10)
      Die("Suit fixture must first admit an ordinary Flash");
    equipped_items = collected_items = gravity ? 0x20 : 1;
    run(gravity ? FlashGravitySetup : FlashVariaSetup);
    for (int step = 0; step < 161; step++) {
      if (step) run(gravity ? FlashGravityHdma : FlashVariaHdma);
      printf("%d,%d,%d,%04X,%04X,%04X,%04X,%04X\n",right,gravity,step,
        substate,substate,suit_pickup_light_beam_pos,samus_shine_timer,timer_for_shine_timer);
    }
  }
}
