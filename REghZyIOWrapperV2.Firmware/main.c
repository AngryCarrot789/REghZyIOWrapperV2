char read_buffer[256];
int read_index;

void clear_buffer() {
	int i = read_index;
	while ((read_buffer[--i] = 0) != 0) {

	}
}

void setup() {
	for (int i = 0; i < 256; i++) {
		read_buffer[i] = 0;
	}
}

void loop() {
	int read = Serial.read();
	if (read == -1) {

	}
}