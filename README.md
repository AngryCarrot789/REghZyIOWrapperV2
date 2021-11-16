# REghZyIOWrapperV2
A more efficient of another thing i made, that uses raw data in the packets, rather than formatted strings. Also containing ACK packets 

This repo also contains 2 classes that i made: DataInputStream, and DataOutputStream :)

These allow you to read and write primitive data types (e.g WriteByte(), WriteLong(), etc)

To solve the problem with writing strings, i created a PacketUtils class, which contains a "WriteStringWL()" method (write string with label), 
which writes the length of the string (as 2 bytes), and then writes the chars. Similar thing for reading; read the 2 byte length, then read that many chars

The idempotency key store works using nodes (it's basically a sort of node-based number range collection). And it also doesn't allow the key 0; must be 1 or above.
It does mean that adding random numbers will be slower than just using a list (and it will use more numbers), but if the order of numbers is somewhat linear,
it should heavily reduce the memory usage (especially if there's 100,000s of entries)

if you add 1 to 50000, and skip a number every 100 or so times, there should be around 500 nodes (1-99, 101-199, etc). 
Searching 500 nodes is probably faster than searching a list of 50,000 elements. The class could also be optimised to store an inner range, specifying
the "missing" range between 2 nodes, e.g rather than have 2 nodes; 1-20, 23-40, you just have 1-40, and that node contains a "missing range" from 21-22
