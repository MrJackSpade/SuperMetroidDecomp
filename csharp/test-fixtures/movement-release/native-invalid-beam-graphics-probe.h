// #396: isolated retail AC8D loader with Chainsaw bits (Wave+Spazer+Plasma).
// Include after native-release-probe.h; dispatch before SDL initialization.
// IMPORTANT: disable main.c's explicit SDL error boxes for this diagnostic. The
// current native harness rejects unmapped palette reads rather than modeling open bus.
int DiagnosticInvalidBeamGraphics(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  for (int beam = 13; beam < 14; beam++) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0;
    g_snes->cpu->dp = 0;
    equipped_beams = beam;
    RunAsmCode(0x90ac8d, 0, 0, 0, 0);
    printf("BEAM %X queue-tail=%u bytes=", beam, vram_write_queue_tail);
    for (int i = 0; i < 7; i++) printf("%02X", g_ram[0xd0 + i]);
    printf(" palette=");
    for (int i = 0; i < 16; i++) printf("%04X%s", palette_buffer[224 + i], i == 15 ? "\n" : ",");
  }
  return 0;
}
