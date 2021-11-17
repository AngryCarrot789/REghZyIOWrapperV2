#ifndef UTILS_H
#define UTILS_H

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
inline void write32(uint16_t hi, uint16_t lo) {
	uint8_t buf[4] = {
		(hi >> 8) & 255,
		hi & 255,
		(lo >> 8) & 255,
		lo & 255
	};

	Serial.write(buf, 4);
}

inline void write_utf8_wl(const char* string, uint16_t charCount) {
	write16(charCount);
	Serial.write(string);
}

// Writes 2 bytes for the length of the string, and writes 2 bytes for each character 
// (the 1st byte will always be 0, because arduinos probably can't support wchar)
void write_utf16_wl(const char* string, uint16_t charCount) {
	write16(charCount);
	uint8_t buf[2] = { 0, 0 };
	for(int i = 0; i < charCount; i++) {
		buf[1] = string[i];
		Serial.write(buf, 2);
	}
}

inline uint8_t read8_blocking() {
	while(Serial.available() < 1) { }
	return Serial.read();
}

inline uint16_t read16_blocking() {
	while (Serial.available() < 2) {}
	uint8_t a = Serial.read();
	uint8_t b = Serial.read();
	return (a << 8) | b;
}

#endif 