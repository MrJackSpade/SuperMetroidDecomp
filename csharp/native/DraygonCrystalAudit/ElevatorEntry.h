/* #434: actual elevator actor dispatch around same-boundary Flash admission. */
enum { FlashElevatorInit = 0xa394e6, FlashElevatorMain = 0xa3952a };
static void elevator_entry_matrix(void) {
  printf("up,right,contact,newdown,order,status,pose,x,y,flash,shine,palette\n");
  for (int up = 0; up < 2; up++)
  for (int right = 0; right < 2; right++)
  for (int contact = 0; contact < 2; contact++)
  for (int newdown = 0; newdown < 2; newdown++)
  for (int order = 0; order < 2; order++) {
    memset(g_ram, 0, sizeof(g_ram));
    memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
    samus_pose = samus_prev_pose = right ? 1 : 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = right ? 8 : 4;
    samus_x_pos = 136; samus_y_pos = 235;
    samus_health = 49; samus_max_health = 99;
    samus_missiles = samus_super_missiles = samus_power_bombs = 10;
    button_config_shoot_x = 0x40; game_state = 8;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    cur_enemy_index = 0;
    Enemy_Elevator *e = Get_Elevator(0);
    e->base.x_pos = 136; e->base.y_pos = 256; e->elevat_parameter_1 = up;
    run(FlashElevatorInit); run(RefreshRadius);
    elevator_flags = contact;
    joypad1_lastkeys = 0x470; joypad1_newkeys = newdown ? 0x470 : 0;
    if (!order) run(FlashEntry);
    run(FlashElevatorMain);
    if (order) run(FlashEntry);
    printf("%d,%d,%d,%d,%d,%04X,%02X,%04X,%04X,%d,%04X,%04X\n",up,right,contact,newdown,order,
      elevator_status,samus_pose,samus_x_pos,samus_y_pos,samus_movement_handler == FlashRaising,samus_shine_timer,timer_for_shine_timer);
  }
}
