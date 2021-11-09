#ifndef __STRING_BUILDER
#define __STRING_BUILDER

#define BUFFER_RESIZE_SUCCESS 1
#define BUFFER_RESIZE_UNCHANCED 1
#define BUFFER_RESIZE_ERR 0

#include <stdlib.h>
#include <math.h>

// A buffer than autoresizes itself, and supports appending strings and integers
class StringBuilder {
public:


	// Ensures the internal buffer can fit the given number of extra characters (extraSize)
	// Returns true if the buffer didn't require a resize, or if it was successfully resized
	// Returns false if the buffer failed to resize (possibly due to memory fragmentation)
	int ensureBufferSize(const int extraSize) {
		if (requireResize(extraSize)) {
			if (resizeBuffer(bufferCapacity + extraSize)) {
				return BUFFER_RESIZE_SUCCESS;
			}

			return BUFFER_RESIZE_ERR;
		}

		return BUFFER_RESIZE_UNCHANCED;
	}

	// Deletes the internal buffer
	void deleteBuffer() {
		delete[](m_buffer);
	}

private:
	// Checks if the internal buffer needs to be resized to fit the given number of extra characters (extraSize)
	inline bool requireResize(const int extraSize) {
		return (extraSize + nextIndex) > bufferCapacity;
	}

	// Sets the last character in this StringBuilder as 0 (aka a null character, allowing strlen to be used)
	void setLastAsNull() {
		setNonCharToNull();
		m_buffer[bufferCapacity] = 0;
	}

	// Resizes the internal buffer to the given size, optionally copying the old buffer into the new one
	bool resizeBuffer(const int newSize) {
		// hopefully prevents heap fragmentation on 
		// things that dont have much ram... like arduinos
		void* newBuffer = realloc(m_buffer, newSize + 1);
		if (newBuffer == nullptr) {
			return false;
		}

		bufferCapacity = newSize;
		m_buffer = (char*)newBuffer;
		setLastAsNull();
		return true;
	}

	// Sets all of the chars including (and past) the next write index, to null
	// So that toString() wont return extra stuff
	void setNonCharToNull() {
		for (int i = nextIndex, end = (bufferCapacity + 1); i < end; i++) {
			m_buffer[i] = 0;
		}
	}

	// Creates the buffer with the capacity + 1
	// DOES NOT DELETE THE OLD BUFFER!
	// msg to me; delete it or rip memory
	void createCapacityBuffer() {
		m_buffer = new char[bufferCapacity + 1];
	}

private:
	// The capacity of the internal buffer
	int bufferCapacity;

	// The index where the next char is to be set (and also how many chars have been written)
	int nextIndex;

	// The pointer to the buffer
	char* m_buffer;
};

#endif // !__STRING_BUILDER