#include "Arduino.h"
#include "handlers.h"

char* write_buffer;

void buf_app16(int index, uint16_t v) {
	write_buffer[index] = ((v >> 8) & 255);
	write_buffer[index + 1] = (v & 255);
}

void buf_app8(int index, uint8_t v) {
	write_buffer[index] = v;
}

void buf_write(int index, int len) {
	for (int e = index + len; index < e; index++) {
		Serial.write(write_buffer[index]);
	}
}

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
	write_buffer = new char[128];
}

uint8_t read8_blocking() {
	while(Serial.available() < 1) { }
	return Serial.read();
}

uint16_t read16_blocking() {
	while (Serial.available() < 2) {}
	uint8_t a = Serial.read();
	uint8_t b = Serial.read();
	return (a << 8) | b;
}

inline void write8(uint8_t v) {
	Serial.write(v);
}

inline void write16(uint16_t v) {
	Serial.write((v >> 8) & 255);
	Serial.write(v & 255);
}

// Writes the given 32 bit integer, using the given HIWORD and LOWORD
// [HIWORD] [LOWORD]
// 0000...0 0000...0
// To write the value "3", the HIWORD would be 0, and the LOWORD would be 3
// To write the value "258", the HIWORD would be 1, and the LOWORD would be 2
void write32(uint16_t hi, uint16_t lo) {
	Serial.write((hi >> 8) & 255);
	Serial.write(hi & 255);
	Serial.write((lo >> 8) & 255);
	Serial.write(lo & 255);
}

void writeCSTR(const char* string, uint16_t len) {
	for(int i = 0; i < len; i++) {
		Serial.write(string[i]);
	}
}

void write_str_wl(const char* string, uint16_t len) {
	write16(len);
	Serial.write(string);
}

void write_wstr_wl(const char* string, uint16_t len) {
	write16(len);
	uint8_t buf[2] = { 0, 0 };
	for(int i = 0; i < len; i++) {
		buf[1] = string[i];
		Serial.write(buf, 2);
	}
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

void handle_p3hi() {
	uint16_t dataA = read16_blocking(); // [0] [1]
	uint16_t dataB = read16_blocking(); // [2] [3]
	uint16_t key = (dataB >> 3);// + ((dataA & 7) << 13);
	uint8_t dir = (dataB & 7);
	uint8_t code = read8_blocking();
	if (dir == 1) {
		write8(3); 						  // packet header - ID
		write16(19); 					  // packet header - Length (of the data)
		write32(dataA, (key << 3) | 3); // ACK header    - idempotency and direction
		write8(code);					  // P3HI - Info code
		char cccc = 'T';
		write_wstr_wl("The Arduino!", 12);	  // P3HI - Info string
	}
}