#ifndef PKT_UTILS_H
#define PKT_UTILS_H

#include "utils.h"

#define IDEMP_HIMASK 0b1111111111111000
#define IDEMP_LOMASK 0b0000000000000111
#define GET_ACKDIR(key_lo) key_lo & IDEMP_LOMASK

// Writes the normal packet header; it's ID and Length
inline void write_pkthead_24(uint8_t id, uint16_t len) {
    uint8_t buf[3] = { id, (len >> 8) & 255, len & 255 };
    Serial.write(buf, 3);
}

// Writes the ACK packet's header; The idempotency key and direction bitmasked together
// the MSW of HI and the 13 high bits of lo make the key, the 3 low bits of lo make the direction 
inline void write_ackhead_32(uint16_t hi, uint16_t lo, uint8_t dir) {
    write32(hi, (lo & IDEMP_HIMASK) | (dir & IDEMP_LOMASK));
}

#endif