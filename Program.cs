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
        int b1 = bits[0];
        int b2 = bits[1];
        int b3 = bits[2];
        int b4 = bits[3];

        // Convert the 3 integers to binary strings
        string s1 = Convert.ToString(b1, 2).PadLeft(32, '0');
        string s2 = Convert.ToString(b2, 2).PadLeft(32, '0');
        string s3 = Convert.ToString(b3, 2).PadLeft(32, '0');
        string s4 = Convert.ToString(b4, 2).PadLeft(32, '0');

        // Combine the binary strings to get the final binary representation
        string binaryString = $"{s1} {s2} {s3} {s4}";

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
        if (given.Count != 6)
        {
            throw new Exception("Input list must contain exactly 6 shorts.");
        }

        // Concatenate the 6 shorts into a 96-bit binary representation
        long combinedValue = 0;
        for (int i = 0; i < 6; i++)
        {
            combinedValue = (combinedValue << 16) | (ushort)given[i];
        }

        // Convert the 96-bit binary representation to a decimal value
        decimal result = new decimal(new int[] { (int)combinedValue, (int)(combinedValue >> 32), 0, 0 });

        return result;
    }
    public static List<short> decimalToShort(decimal given)
    {
        // Get the underlying 96-bit binary representation of the decimal value
        int[] bits = decimal.GetBits(given);

        // Check if the decimal has exactly 96 bits (4 elements in the bits array)
        if (bits.Length != 4)
        {
            throw new Exception("Input decimal must have exactly 96 bits.");
        }

        // Convert the 96-bit binary representation into 6 shorts
        List<short> result = new List<short>();
        for (int i = 0; i < 4; i++)
        {
            // Get the lower 16 bits from each 32-bit element
            short lowerShort = (short)(bits[i] & 0xFFFF);
            result.Add(lowerShort);

            // Shift right by 16 bits to get the next 16 bits from the same 32-bit element
            short upperShort = (short)((bits[i] >> 16) & 0xFFFF);
            result.Add(upperShort);
        }

        return result;
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
    public static decimal SetBit(decimal input, int position, bool value)
    {
        if (position < 0 || position > 127)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 0 and 127.");
        }

        int[] bits = Decimal.GetBits(input);

        int intIndex = position / 32;
        int bitIndex = position % 32;

        if (value)
        {
            // Set the bit to 1
            bits[intIndex] |= (1 << bitIndex);
        }
        else
        {
            // Set the bit to 0
            bits[intIndex] &= ~(1 << bitIndex);
        }

        return new decimal(bits);
    }
    static void Main()
    {
        test32To16bitFloats();
    }
    static void testDecimalBitSetting()
    {
        for (int i = 0; i < 96; i++)
        {
            decimal x = SetBit(i, 0, true);
            Console.WriteLine(x);
            ByteJuggler.printDecimal(x);
        }
    }
    static void test32To16bitFloats()
    {
        List<float> start = new List<float>() {-20, 10, 32.2f, 1.0f, 8.0f, 13.2f };
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
        List<float> start = new List<float>() { -20, 10, 32.2f, 1.0f, 8.0f, 13.2f };

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
        for (int i = 0; i < collection.Count; i++)
        {
            Console.Write(collection.ElementAt(i) + ", ");
        }
        Console.WriteLine();
    }
}
/*
using System;
using System.Diagnostics.Tracing;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static void Main()
    {
        var start = new short[] { 123, 1, 3, 2, 612, 2 };
        decimal encoded = encode(start);
        short[] decoded = decode(encoded);

        Console.WriteLine("encoded: " + encoded);
        for(int i =  0; i < decoded.Length; i++)
        {
            Console.WriteLine($"{start[i]} -> {decoded[i]}");
        }
    }

    private static short[] decode(decimal given)
    {
        short[] decoded = new short[6];
        int[] bits = decimal.GetBits(given);
        for(int i = 0; i < 3; i++)
        {
            int firstIdx = i;
            int secondIdx = i + 3;
            short firstValue = (short)(((int)bits[i]) >> 16);
            short secondValue = (short)(((int)bits[i]) & 0xFFFF);
            decoded[firstIdx] = firstValue;
            decoded[secondIdx] = secondValue;
        }

        return decoded;
    }

    public static void printDecimalBits(decimal myDecimal)
    {
        int[] bits = decimal.GetBits(myDecimal);

        Console.Write(myDecimal.ToString() + ": ");

        // Output individual components in binary format
        for (int i = 0; i < bits.Length; i++)
        {
            Console.Write(Convert.ToString(bits[i], 2).PadLeft(32, '0'));
            Console.Write(" ");
        }
        Console.WriteLine();
    }
    public static void printIntBits(int number)
    {
        int totalBits = sizeof(int) * 8;

        // Loop through each bit and print it
        for (int i = totalBits - 1; i >= 0; i--)
        {
            int bitValue = (number >> i) & 1;
            Console.Write(bitValue);
        }

        Console.WriteLine();
    } 
    public static void printShortBits(int number)
    {
        int totalBits = sizeof(short) * 8;

        // Loop through each bit and print it
        for (int i = totalBits - 1; i >= 0; i--)
        {
            int bitValue = (number >> i) & 1;
            Console.Write(bitValue);
        }

        Console.WriteLine();
    }
    public static decimal encode(short[] arr)
    {
        if(arr.Length != 6)
        {
            return -1;
        }
        int[] ints = new int[3];
        for (int i = 0; i < 3; i++)
        {
            int firstIdx = i;
            int secondIdx = i + 3; 
            ints[i] = (arr[firstIdx] << 16) | ((int)arr[secondIdx]);

            Console.WriteLine(firstIdx);
            printIntBits(arr[firstIdx]);
            printShortBits(arr[firstIdx]);
            Console.WriteLine(secondIdx);
            printIntBits(arr[secondIdx]);
            printShortBits(arr[secondIdx]);
            Console.WriteLine("------");
            printIntBits(ints[i]);
            Console.WriteLine("------");
        }

        // Combine the components to create the decimal
        int[] bits = new int[4];
        ints.CopyTo(bits, 0);
        decimal result = new decimal(bits);
        int[] gottenBits = decimal.GetBits(result);

        for(int i = 0; i < gottenBits.Length; i++)
        {
            Console.WriteLine($"{bits[i]} -> {gottenBits[i]}");
        }

        return result;
    }
}
*/