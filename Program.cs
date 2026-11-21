using System;
using System.Collections;

public class Utils
{
    public static int GetDecLength(uint n)
    {
        return n != 0 ? (int)Math.Log10(n) + 1 : 1;
    }
    public static int GetBinLength(uint n)
    {
        if(n == 0) return 1;
        int digits = 0;
        while (n > 0)
        {
            n >>= 1;
            digits++;
        }
        return digits;
    }
    public static bool[] ToBoolArray(uint n)
    {
        int binDigits = GetBinLength(n);
        bool[] result = new bool[binDigits];
        for(int i = 0; i < binDigits; i++)
        {
            result[i] = (n & (1u << (binDigits - i - 1))) != 0;
        }
        return result;
    }
    public static void PrintBits(BitArray bits)
    {
        foreach(bool bit in bits)
        {
            Console.Write(bit ? "1" : "0");
        }
        Console.WriteLine();
    }
    public static bool[] CombineBoolArr(bool[] arr1, bool[] arr2)
    {
        bool[] combined = new bool[arr1.Length + arr2.Length];
        Array.Copy(arr1, 0, combined, 0, arr1.Length);
        Array.Copy(arr2, 0, combined, arr1.Length, arr2.Length);
        return combined;
    }
    public static uint Pow2(int exponent)
    {
        uint result = 1u << exponent;
        return result;
    }
}
public class EliasGamma
{
    public static bool[] EncodeAsBoolArray(uint n)
    {
        if(n == 0) throw new ArgumentOutOfRangeException(nameof(n), "Invalid input : 0");

        int binLength = Utils.GetBinLength(n);
        int bitsLength = binLength*2 - 1;
        bool[] bitsBoolArr = new bool[bitsLength];
        for(int i = 0 ; i < binLength; i++)
        {
            // 後ろから
            bitsBoolArr[bitsLength - 1 - i] = (n & (1u << i)) != 0;
        }
        return bitsBoolArr;
    }
    public static BitArray Encode(uint n)
    {
        BitArray bits = new BitArray(EncodeAsBoolArray(n));
        return bits;
    }

    private static int GetBinaryPartLength(BitArray bits)
    {
        int digits = 1;
        while (true)
        {
            if(bits[digits] == true) break;

            digits++;
        }
        return digits;
    }
    public static uint DecodeOnce(BitArray bits)
    {
        int length = GetBinaryPartLength(bits);
        uint result = 0;
        for(int i = 0; i < length; i++)
        {
            //Console.WriteLine($"i {i} / condition {bits[bits.Length - 1 - i] is true}");
            // 後ろから
            if(bits[bits.Length - 1 - i] is true) result += Utils.Pow2(i);
        }
        return result;
    }

    
    public static uint[] Decode(BitArray bits, int offset = 0)
    {
        if (offset < 0 || offset > bits.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));

        int prefixZeroCount = 0;
        int remainingBits = 0;
        uint memo = 0;
        bool nowReadingPrefix = true;

        List<uint> decoded = new List<uint>();

        for (int i = offset; i < bits.Length; i++)
        {
            bool bit = bits[i];

            if (nowReadingPrefix)
            {
                if (!bit)
                {
                    prefixZeroCount++;
                    continue;
                }

                nowReadingPrefix = false;
                remainingBits = prefixZeroCount;
                memo = 1;

                if (remainingBits == 0)
                {
                    decoded.Add(memo);
                    memo = 0;
                    prefixZeroCount = 0;
                    nowReadingPrefix = true;
                }
            }
            else
            {
                memo = (memo << 1) | (bit ? 1u : 0u);
                remainingBits--;

                if (remainingBits == 0)
                {
                    decoded.Add(memo);
                    memo = 0;
                    prefixZeroCount = 0;
                    nowReadingPrefix = true;
                }
            }
        }

        if (!nowReadingPrefix || prefixZeroCount != 0)
            throw new FormatException("Incomplete Elias Gamma code.");

        return decoded.ToArray();
    }

    public static BitArray EncodeFromUintArray(uint[] array)
    {
        bool[] codedAsBool = EncodeAsBoolArray(array[0]);
        for(int i = 1; i < array.Length; i++)
        {
            codedAsBool = Utils.CombineBoolArr(codedAsBool, EncodeAsBoolArray(array[i]));
        }
        return new BitArray(codedAsBool);
    }
}

public class Program
{
    static void Main()
    {
        uint[] arr = [1,2,3,5,8,13,25,31,32];
        bool[] codedAsBool = EliasGamma.EncodeAsBoolArray(arr[0]);
        foreach(uint item in arr)
        {
            if(item == arr[0]) continue;
            codedAsBool = Utils.CombineBoolArr(codedAsBool, EliasGamma.EncodeAsBoolArray(item));
        }
        //codedAsBool = Utils.CombineBoolArr(codedAsBool, [false]);
        BitArray coded = new BitArray(codedAsBool);
        Utils.PrintBits(coded);
        Console.WriteLine(string.Join(", ", EliasGamma.Decode(coded)));
        //Utils.PrintBits(EliasGamma.EncodeFromUintArray(EliasGamma.Decode(coded)));

    }
}