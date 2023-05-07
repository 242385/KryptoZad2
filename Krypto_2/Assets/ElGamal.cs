using System;
using System.Collections;
using AnotherFileBrowser.Windows;
using System.Numerics;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class ElGamal : MonoBehaviour
{
    byte[] _fileBytes;
    byte[][] _blockArray;
    private byte[] _output;
    BigInteger gValue;
    BigInteger aValue;
    BigInteger hValue;
    BigInteger pValue;

    public TMP_Text path;
    public TMP_InputField G;
    public TMP_InputField H;
    public TMP_InputField A;
    public TMP_InputField P;

    void Start()
    {
        // We don't guarantee G number to be a generator of the multiplicative group
        pValue = GenerateRandomBigPrime(32);
        gValue = RandomBigIntInRange(2, pValue - 2);
        aValue = RandomBigIntInRange(2, pValue - 2);
        hValue = ModularPow(gValue, aValue, pValue);

        G.text = gValue.ToString();
        H.text = hValue.ToString();
        A.text = aValue.ToString();
        P.text = pValue.ToString();
        
        byte[] randomNumber = new byte[8];

        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomNumber);
        }

        SingleBlockEncrypting(randomNumber);
    }

    BigInteger RandomBigIntInRange(BigInteger a, BigInteger b)
    {
        // Inclusive
        int maxBytes = (b.ToByteArray().Length > 8) ? 8 : b.ToByteArray().Length;
        BigInteger result;
        do
        {
            result = GenerateRandomBigInt(maxBytes);
        } while (result <= a || result >= b);

        return result;
    }

    BigInteger GenerateRandomBigInt(int numBytes)
    {
        byte[] randomNumber = new byte[numBytes];

        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomNumber);
        }

        // Set the most significant bit to 0 so we're sure that it's always positive
        randomNumber[numBytes - 1] &= 0x7F;

        BigInteger bigInt = new BigInteger(randomNumber);

        return bigInt;
    }

    BigInteger GenerateRandomBigPrime(int numBytes, int certainty = 100)
    {
        BigInteger candidate;

        do
        {
            candidate = GenerateRandomBigInt(numBytes);
        } while (!IsPrime(candidate, certainty));

        return candidate;
    }

    BigInteger ModularPow(BigInteger a, BigInteger exp, BigInteger mod)
    {
        BigInteger result = 1;
        while (exp > 0)
        {
            if (exp % 2 == 1)
            {
                result = (result * a) % mod;
            }

            a = (a * a) % mod;
            exp /= 2;
        }

        return result;
    }

    bool IsPrime(BigInteger n, int certainty)
    {
        if (n <= 1)
        {
            return false;
        }

        if (n == 2 || n == 3)
        {
            return true;
        }

        if (n % 2 == 0)
        {
            return false;
        }

        BigInteger d = n - 1;
        int s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s++;
        }

        Random random = new Random();

        for (int i = 0; i < certainty; i++)
        {
            BigInteger a;
            do
            {
                a = new BigInteger(random.Next()) % (n - 3) + 2;
            } while (a >= n - 2);

            BigInteger x = ModularPow(a, d, n);

            if (x == 1 || x == n - 1)
            {
                continue;
            }

            for (int r = 1; r < s; r++)
            {
                x = (x * x) % n;
                if (x == 1)
                {
                    return false;
                }

                if (x == n - 1)
                {
                    break;
                }
            }

            if (x != n - 1)
            {
                return false;
            }
        }

        return true;
    }

    public void Encrypt()
    {
        /*if (key1Field.text.Length == 0)
            key1Field.text = exampleKey1;
        
        if (key2Field.text.Length == 0)
            key2Field.text = exampleKey2;
        
        if (key3Field.text.Length == 0)
            key3Field.text = exampleKey3;

        key1Field.text = key1Field.text.PadRight(64, '0');
        key2Field.text = key2Field.text.PadRight(64, '0');
        key3Field.text = key3Field.text.PadRight(64, '0');

        exampleKey1 = key1Field.text;
        exampleKey2 = key2Field.text;
        exampleKey3 = key3Field.text;*/

        for (int i = 0; i < _blockArray.Length; i++)
        {
            _blockArray[i] = SingleBlockEncrypting(_blockArray[i]);
        }
    }

    public void Decrypt()
    {
        /*if (key1Field.text.Length == 0)
            key1Field.text = exampleKey1;
        
        if (key2Field.text.Length == 0)
            key2Field.text = exampleKey2;
        
        if (key3Field.text.Length == 0)
            key3Field.text = exampleKey3;

        key1Field.text = key1Field.text.PadRight(64, '1');
        key2Field.text = key2Field.text.PadRight(64, '2');
        key3Field.text = key3Field.text.PadRight(64, '3');*/
    }

    byte[] SingleBlockEncrypting(byte[] block)
    {
        BigInteger m = ConvertBlockToNumber(block);
        BigInteger r = RandomBigIntInRange(1, pValue);
        BigInteger c1, c2;

        c1 = ModularPow(gValue, r, pValue);
        c2 = m * ModularPow(hValue, r, pValue);
        
        Debug.Log(c1);
        Debug.Log(c2);
        
        return Array.Empty<byte>();
    }

    BigInteger ConvertBlockToNumber(byte[] block)
    {
        byte[] extended = new byte[block.Length + 1];
        extended[^1] = 0;
        for (int i = 0; i < extended.Length - 1; i++)
        {
            extended[i] = block[i];
        }

        BigInteger result = new BigInteger(extended);
        result += 2;

        return result;
    }

    byte[] ConvertNumberToBlock(BigInteger number, int blockSize)
    {
        number -= 2;
        byte[] byteArray = number.ToByteArray();
        byte[] clipped = new byte[blockSize];

        int copyLength = Math.Min(byteArray.Length, clipped.Length);

        for (int i = 0; i < copyLength; i++)
        {
            clipped[i] = byteArray[i];
        }

        return clipped;
    }


    BitArray LeftShift(BitArray b, int count)
    {
        // Beware! Least significant bits are to the right, so our left shift is actually C#'s right shift XD
        // BitArray.Set() is used for wrapping bits so we don't lose them.
        bool msb;
        for (int i = 0; i < count; i++)
        {
            msb = b.Get(0);
            b = new BitArray(b.RightShift(1));
            b.Set(b.Length - 1, msb);
        }

        return b;
    }

    BitArray RightShift(BitArray b, int count)
    {
        // Beware! Least significant bits are to the right, so our left shift is actually C#'s right shift XD
        // BitArray.Set() is used for wrapping bits so we don't lose them.
        bool lsb;
        for (int i = 0; i < count; i++)
        {
            lsb = b.Get(b.Length - 1);
            b = new BitArray(b.LeftShift(1));
            b.Set(0, lsb);
        }

        return b;
    }

    BitArray ConcatenateBitArrays(BitArray bits1, BitArray bits2)
    {
        // Beware! Least significant bits are to the right, so our left shift is actually C#'s right shift XD
        // BitArray.Set() is used for wrapping bits so we don't lose them.
        BitArray concatenatedBits = new BitArray(bits1.Length + bits2.Length);
        for (int i = 0; i < bits1.Count; i++)
            concatenatedBits[i] = bits1[i];
        for (int i = 0; i < bits2.Count; i++)
            concatenatedBits[bits2.Count + i] = bits2[i];
        return concatenatedBits;
    }

    int BitArrayToInt(BitArray b)
    {
        int inInt = 0;
        for (int i = 0; i < b.Length; i++)
        {
            inInt <<= 1;
            if (b[i])
            {
                inInt |= 1;
            }
        }

        return inInt;
    }

    BitArray IntToBitArray(int value, int bitArrayLength)
    {
        string binaryString = Convert.ToString(value, 2);

        if (binaryString.Length < bitArrayLength)
        {
            binaryString = binaryString.PadLeft(bitArrayLength, '0');
        }

        BitArray bitArray = new BitArray(bitArrayLength);
        for (int i = 0; i < bitArrayLength; i++)
        {
            if (binaryString[i] == '1')
            {
                bitArray.Set(i, true);
            }
        }

        return bitArray;
    }

    BitArray XOR(BitArray A, BitArray B)
    {
        BitArray result = new BitArray(A.Length);
        for (int i = 0; i < A.Length; i++)
        {
            result.Set(i, A[i] ^ B[i]);
        }

        return result;
    }

    BitArray BytesToBitArray(byte[] bytes)
    {
        BitArray bitArray = new BitArray(bytes);
        for (int i = 0; i < bitArray.Length; i += 8)
        {
            for (int j = 0; j < 4; j++)
                (bitArray[i + j], bitArray[i + 7 - j]) = (bitArray[i + 7 - j], bitArray[i + j]);
        }

        return bitArray;
    }

    byte[] ConvertStringToBytes(string s)
    {
        return Encoding.ASCII.GetBytes(s);
    }

    public void OpenFile()
    {
        var bp = new BrowserProperties
        {
            filter = "All Files (*.*)|*.*",
            filterIndex = 0
        };

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            _fileBytes = File.ReadAllBytes(path);
            _blockArray = SplitTo64BitBlocks(_fileBytes);
            this.path.text = $"Wybrany plik: {path}";
        });
    }

    public void SaveFile()
    {
        var bp = new BrowserProperties
        {
            filter = "All Files (*.*)|*.*",
            filterIndex = 0
        };

        new FileBrowser().SaveFileBrowser(bp, "test", ".txt", path =>
        {
            File.WriteAllBytes(path, _fileBytes);
            this.path.text = $"Wybrany plik: {path}";
        });
    }

    byte[][] SplitTo64BitBlocks(byte[] data)
    {
        // Calculate the number of 64-bit blocks needed
        int numBlocks = (data.Length + 7) / 8;

        // Create a 2D array to hold the blocks
        byte[][] blocks = new byte[numBlocks][];
        for (int i = 0; i < numBlocks; i++)
        {
            blocks[i] = new byte[8];
        }

        // Copy the data into the blocks, padding with zeros if necessary
        for (int i = 0; i < data.Length; i++)
        {
            blocks[i / 8][i % 8] = data[i];
        }

        for (int i = data.Length; i < numBlocks * 8; i++)
        {
            blocks[i / 8][i % 8] = 0;
        }

        return blocks;
    }

    public byte[] ConcatenateByteArrays(byte[][] arrays)
    {
        int totalLength = 0;
        foreach (byte[] array in arrays)
        {
            totalLength += array.Length;
        }

        byte[] result = new byte[totalLength];
        int currentIndex = 0;
        foreach (byte[] array in arrays)
        {
            Array.Copy(array, 0, result, currentIndex, array.Length);
            currentIndex += array.Length;
        }

        return result;
    }


    void DebugByteShowBits(byte[] bytes)
    {
        string s = "";
        foreach (byte b in bytes)
        {
            for (int i = 7; i >= 0; i--)
            {
                s += (b & (1 << i)) != 0 ? "1" : "0";
            }

            s += " ";
        }

        Debug.Log(s);
    }

    void DebugBits(BitArray bits, int space)
    {
        string s = "";
        for (int i = 0; i < bits.Length; i++)
        {
            s += bits[i] ? "1" : "0";
            if ((i + 1) % space == 0)
                s += " ";
        }

        Debug.Log(s);
    }

    public void Quit()
    {
        Application.Quit();
    }
}