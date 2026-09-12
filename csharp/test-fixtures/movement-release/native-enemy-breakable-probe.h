#include "native-bounded-cpu.h"

// Original CPU: actual Shaktool-room terrain and shared spike reaction / PLM handler.
int DiagnosticEnemyBreakable(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "bts,full,frame,carry,word,header,instruction,timer\n");
  for (int bts = 0; bts < 32; bts++) for (int full = 0; full < 2; full++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_ptr = 0xd8c5;
    ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
    int block = room_width_in_blocks * 5 + 16;
    level_data[block] = 0xa110; BTS[block] = (bts & 15) | (bts >= 16 ? 0x80 : 0);
    if (full) for (int p = 0; p < 40; p++) plm_header_ptr[p] = 0xd094;
    cur_block_index = block;
    ProbeRunBounded(0xa0c2c0);
    int carry = g_snes->cpu->c;
    for (int frame = -1; frame < ((bts & 15) == 15 && !full ? 16 : 0); frame++) {
      if (frame >= 0) {
        ProbeRunBounded(0x8483ad);
        vram_write_queue_tail = 0;
        ProbeRunBounded(0x8485b4);
      }
      fprintf(f, "%d,%d,%d,%d,%u,%u,%u,%u\n", BTS[block],full,frame,carry,
        level_data[block],plm_header_ptr[39],plm_instr_list_ptrs[39],plm_instruction_timer[39]);
    }
  }
  fclose(f); return 0;
}
