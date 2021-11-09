#include <stdlib.h>
typedef unsigned char byte;

byte read_buffer[256] = {0};
int read_index;

typedef struct {
	char Data1;
	int Data2;
	unsigned short Data3;
	char Data4;

}__attribute__((packed, aligned(1))) sSampleStruct;

typedef struct {
	byte size;
	byte id;
	byte meta;
	byte* data;
} Packet01;

void clearBuffer() {
	while (read_index > 0) {
		read_buffer[--read_index] = 0;
	}
}

void setup() {
	read_buffer[0] = 7;
	read_buffer[1] = 21;
	read_buffer[2] = 69;
	read_buffer[3] = 'h';
	read_buffer[4] = 'e';
	read_buffer[5] = 'l';
	read_buffer[6] = 'l';
	read_buffer[7] = 'o';
}

byte read() {
	if (read_index > 255) {
		return 0;
	}

	return read_buffer[read_index++];
}

byte peek() {
	if (read_index > 255) {
		return 0;
	}

	return read_buffer[read_index];
}

int main() {
	setup();

	Packet01* pkt = reinterpret_cast<Packet01*>(read_buffer);
}