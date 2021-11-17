#include "Arduino.h"
#include "handlers.h"
#include "pktutils.h"
#include "utils.h"

void read_packet(uint8_t id, uint16_t len);

void setup(void) {
	Serial.begin(9600);
	pinMode(5, OUTPUT);
	pinMode(6, OUTPUT);
	pinMode(7, OUTPUT);
	pinMode(8, OUTPUT);
	pinMode(9, OUTPUT);
	pinMode(10, OUTPUT);
	pinMode(11, OUTPUT);
	pinMode(12, OUTPUT);
	pinMode(13, OUTPUT);
}

void loop(void) {
	int readable = Serial.available();
	if (readable < 3) {
		return;
	}

	uint8_t id = Serial.read();
	uint8_t len_1 = Serial.read();
	uint8_t len_2 = Serial.read();
	uint16_t len = (len_1 << 8) + (len_2); 
	read_packet(id, len);
}

void read_packet(uint8_t id, uint16_t len) {
	switch (id) {
		case 1: handle_p1dw(); break;
		case 2: handle_p2sh(); break;
		case 3: handle_p3hi(); break;
	}

	return;
}

void handle_p1dw() {
	uint8_t data = read8_blocking();
	uint8_t pin = (data >> 1) & 127;
	uint8_t state = (data & 1);
	digitalWrite(pin, state);
}

void handle_p2sh() {
	write_pkthead_24(2, 14);
	write_utf16_wl("Hello!", 6);
}

void handle_p3hi() {
	uint16_t key_hi = read16_blocking(); // [0] [1]
	uint16_t key_lo = read16_blocking(); // [2] [3]
	uint8_t dir = GET_ACKDIR(key_lo);
	uint8_t code = read8_blocking();
	if (dir == 1) {
		write_pkthead_24(3, 31);
		write_ackhead_32(key_hi, key_lo, 3);
		write8(code);					  	// P3HI - Info code
		write_utf16_wl("The Arduino!", 12);	// P3HI - Info string
	}
}