#include "native-bounded-cpu.h"

// Storage admission and palette-owned countdown; no movement/input route is synthesized.
int DiagnosticShineStorage(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const int counters[]={0,0x3ff,0x400,0x4ff,0x500,0x8300,0x8400,0xff00};
  fprintf(f,"counter,suit,frame,timer,kind,paletteframe,sound,palette\n");
  for(int c=0;c<8;c++) for(int suit=0;suit<3;suit++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    speed_boost_counter=counters[c]; samus_suit_palette_index=suit*2;
    ProbeRunBounded(0x91f7b0);
    for(int frame=0;frame<=182;frame++) {
      sfx_readpos[2]=sfx_writepos[2]=0; memset(sfx3_queue,0,16);
      if(frame && timer_for_shine_timer==1) ProbeRunBounded(0x91dac7);
      fprintf(f,"%04X,%d,%d,%04X,%04X,%04X,%04X,",counters[c],suit,frame,
        samus_shine_timer,timer_for_shine_timer,special_samus_palette_frame,
        sfx_writepos[2]?sfx3_queue[0]:0);
      // Expiry's normal-palette restoration belongs to the outer dispatcher,
      // not this handler. Compare palette words only while the handler is live.
      if(frame && timer_for_shine_timer==1)
        for(int i=0;i<16;i++) fprintf(f,"%04X",palette_buffer[192+i]);
      fprintf(f,"\n");
    }
  }
  fclose(f); return 0;
}
