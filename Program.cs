class Program
{
    static void Main()
    {
        var start = new List<float>() { 123.3f, 1.2f, 3f, 2, 612.1221f, 2 };
        List<decimal> encoded = encode(start);
        printDecimalBits(encoded.ElementAt(0));
        List<float> decoded = decode(encoded);

        Console.WriteLine("encoded: " + encoded);
        for(int i =  0; i < start.Count; i++)
        {
            Console.WriteLine($"{start[i]} -> {decoded[i]}");
        }
    }

    private static List<decimal> encode(List<float> list)
    {
        if(list.Count % 6 != 0)
        {
            throw new Exception("");
        }
        decimal[] res = new decimal[list.Count / 6];

        for(int i = 0; i < res.Length; i++)
        {
            IEnumerable<float> batch = list.Skip(i*6).Take(6);
            res[i] = encodeShortArrToDecimal(floats32ToFloats16(batch.ToList()).ToArray());
        };

        return res.ToList();
    }

    private static List<float> decode(List<decimal> start)
    {
        return start.SelectMany(x =>
        {
            return floats16ToFloats32(decodeDecimalToShortArr(x).ToList());
        }).ToList();
    }

    public static List<short> floats32ToFloats16(List<float> given)
    {
        return given.Select(x => float32Tofloat16(x)).ToList();
    }
    public static List<float> floats16ToFloats32(List<short> given)
    {
        return given.Select(x => float16ToFloat32(x)).ToList();
    }
    public static short float32Tofloat16(float value)
    {
        int fbits = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        int sign = (fbits >> 16) & 0x8000;          // Sign bit
        int val = ((fbits >> 23) & 0xFF) - 127 + 15; // Exponent - adjusted and clamped
        int frac = (fbits & 0x007FFFFF) >> 13;      // Fractional part
        int hbits = sign | (val << 10) | frac;

        return (short)hbits;
    }
    public static float float16ToFloat32(short hbits)
    {
        int sign = (hbits >> 15) & 0x00000001;
        int exp = (hbits >> 10) & 0x0000001F;
        int mant = hbits & 0x000003FF;

        int fbits = (sign << 31) | ((exp - 15 + 127) << 23) | (mant << 13);

        return BitConverter.ToSingle(BitConverter.GetBytes(fbits), 0);
    }

    private static short[] decodeDecimalToShortArr(decimal given)
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
    public static decimal encodeShortArrToDecimal(short[] arr)
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