using System;
using System.Security.Principal;
using static System.Runtime.InteropServices.JavaScript.JSType;

class ByteJuggler
{
    public static void printFloatBits(float given)
    {
        // Convert the float value to its binary representation as a byte array
        byte[] bytes = BitConverter.GetBytes(given);

        // Extract the individual components of the floating-point value
        int sign = (bytes[3] >> 7) & 1;
        int exponent = ((bytes[3] & 0x7F) << 1) | ((bytes[2] >> 7) & 1);
        int mantissa = ((bytes[2] & 0x7F) << 16) | (bytes[1] << 8) | bytes[0];

        // Print the components with spaces in between
        Console.WriteLine($"{sign} {mantissa} {exponent}");
    }
    public static void printDecimal(decimal given)
    {
        int[] bits = decimal.GetBits(given);

        // Each decimal number is represented using four 32-bit integers (96 bits in total).
        // We only need the first 3 integers to get the binary representation.
        int lastInt = bits[bits.Length - 1];
        int midInt = bits[bits.Length - 2];
        int firstInt = bits[bits.Length - 3];

        // Convert the 3 integers to binary strings
        string lastBits = Convert.ToString(lastInt, 2).PadLeft(32, '0');
        string midBits = Convert.ToString(midInt, 2).PadLeft(32, '0');
        string firstBits = Convert.ToString(firstInt, 2).PadLeft(32, '0');

        // Combine the binary strings to get the final binary representation
        string binaryString = $"{firstBits} {midBits} {lastBits}";

        Console.WriteLine(binaryString);
    }
}
class Helper
{
    public static short floatToHalf(float value)
    {
        int fbits = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        int sign = (fbits >> 16) & 0x8000;          // Sign bit
        int val = ((fbits >> 23) & 0xFF) - 127 + 15; // Exponent - adjusted and clamped
        int frac = (fbits & 0x007FFFFF) >> 13;      // Fractional part
        int hbits = sign | (val << 10) | frac;

        return (short)hbits;
    }
    public static float HalfToFloat(short hbits)
    {
        int sign = (hbits >> 15) & 0x00000001;
        int exp = (hbits >> 10) & 0x0000001F;
        int mant = hbits & 0x000003FF;

        int fbits = (sign << 31) | ((exp - 15 + 127) << 23) | (mant << 13);

        return BitConverter.ToSingle(BitConverter.GetBytes(fbits), 0);
    }
}
class CustomConverter
{
    public static List<short> floatToShort(List<float> given)
    {
        return given.Select(x => Helper.floatToHalf(x)).ToList();
    }
    public static decimal shortToDecimal(List<short> given)
    {
        return 1;
    }
    public static List<short> decimalToShort(decimal given)
    {
        return new List<short>();
    }
    public static List<float> shortToFloat(List<short> given)
    {
        return given.Select(x => Helper.HalfToFloat(x)).ToList();
    }
}
class Magician
{
    public static decimal floatsToDecimal(List<float> given)
    {
        return CustomConverter.shortToDecimal(CustomConverter.floatToShort(given));
    }
    public static List<float> decimalToFloats(decimal given)
    {
        return CustomConverter.shortToFloat(CustomConverter.decimalToShort(given));
    }
}
class Program
{
    static void Main()
    {
       test32To16bitFloats();
    }
    static void test32To16bitFloats()
    {
        List<float> start = new List<float>() { 15, -20, 10, 20, 32.2f, 1.0f, 8.0f, 13.2f };
        List<short> encoded = CustomConverter.floatToShort(start);
        List<float> decoded = CustomConverter.shortToFloat(encoded);

        Console.WriteLine("start");
        printCollection(start);
        Console.WriteLine();

        Console.WriteLine("encoded");
        printCollection(encoded);
        Console.WriteLine();

        Console.WriteLine("decoded:");
        printCollection(decoded);
        Console.WriteLine();
    }
    static void testCompleteEncoding()
    {
        List<float> start = new List<float>() { 15, -20, 10, 20, 32.2f, 1.0f, 8.0f, 13.2f };

        decimal encoded = Magician.floatsToDecimal(start);
        List<float> decoded = Magician.decimalToFloats(encoded);

        Console.WriteLine("starting with collection: ");
        printCollection(start);
        Console.WriteLine();

        Console.WriteLine($"encoded {encoded}");
        ByteJuggler.printDecimal(encoded);
        Console.WriteLine();

        Console.WriteLine("decoded flaots");
        printCollection(decoded);
    }

    static void printCollection<T>(List<T> collection)
    {
        for(int i = 0; i < collection.Count; i++)
        {
            Console.Write(collection.ElementAt(i) + ", ");
        }
        Console.WriteLine();
    }
}
