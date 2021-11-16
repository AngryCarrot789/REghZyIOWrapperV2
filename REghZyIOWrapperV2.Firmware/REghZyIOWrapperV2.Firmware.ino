#include "handlers.h"

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

uint64_t read8_blocking() {
	while(Serial.available() < 1) { }
	return Serial.read();
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
		case 1: handle_p1dw(read8_blocking()); break;
		case 3: handle_p3hi(0); break;
	}

	return;
}

void handle_p1dw(uint8_t data) {
	uint8_t pin = (data >> 1) & 127;
	uint8_t state = (data & 1);
	digitalWrite(pin, state);
}

void handle_p3hi(uint8_t data) {

}
