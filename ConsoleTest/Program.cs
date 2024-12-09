using Things;
using static System.Console;

var buffer = new CharRingBuffer(26);

buffer.Write("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

WriteLine($"CanWrite should be false: {buffer.CanWrite}");
WriteLine($"CanRead should be true: {buffer.CanRead}");
WriteLine($"WriteSpaceAvailable should be 0: {buffer.WriteSpaceAvailable}");
