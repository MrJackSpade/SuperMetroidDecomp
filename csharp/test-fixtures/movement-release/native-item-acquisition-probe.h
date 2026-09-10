#include "native-bounded-cpu.h"

// #463: run the original PLM handler after a real remote item collision.
// Stop at $85:8080, before the message coroutine waits for player input.
int DiagnosticItemAcquisition(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f,"left,gap,blocker,timer,trigger,message,ammo,maxammo,bit\n");
  for (int left=0;left<2;left++)
  for (int gap=0;gap<17;gap++)
  for (int blocker=0;blocker<3;blocker++)
  for (int timer=1;timer<=4;timer++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=16;
    room_width_in_scrolls=1; room_height_in_scrolls=1; room_size_in_blocks=512;
    samus_x_pos=left ? 117+gap : 139-gap; samus_y_pos=136;
    samus_x_radius=5; samus_y_radius=12; samus_pose=left ? 0x1a : 0x19;
    game_state=8; plm_flag=0x8000;
    int column=left ? 6 : 9;
    level_data[8*16+column]=0xb000; BTS[8*16+column]=0x45;
    plm_header_ptr[39]=0xeedb; plm_block_indices[39]=2*(8*16+column);
    // Visible missile loop: remaining draw timer, next draw, triggered link.
    plm_instruction_timer[39]=timer; plm_instr_list_ptrs[39]=0xe0ce;
    plm_pre_instrs[39]=0xdf89; plm_instruction_list_link_reg[39]=0xe0d6;
    if (blocker) level_data[(blocker==1 ? 7 : 9)*16+column]=0x8000;
    *(uint16 *)(g_ram+0x12)=left ? -8 : 8;
    *(uint16 *)(g_ram+0x14)=0;
    ProbeRunBounded(0x94967f);
    uint16 triggered=plm_timers[39];
    bool message=ProbeRunBoundedUntil(0x8485b4,0,0,0,0x858080);
    fprintf(f,"%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X\n",left,gap,blocker,timer,
      triggered,message ? g_snes->cpu->a : 0,samus_missiles,samus_max_missiles,item_bit_array[0]);
  }
  fclose(f); return 0;
}
