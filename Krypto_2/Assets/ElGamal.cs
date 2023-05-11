using System;
using System.Collections;
using AnotherFileBrowser.Windows;
using System.Numerics;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

public class ElGamal : MonoBehaviour
{
    byte[] _fileBytes;
    byte[][] _blockArray; // 8B
    byte[][] _encryptedBlockArray; // 64B
    uint _encryptedSize;
    byte[] _output;
    BigInteger gValue;
    BigInteger aValue;
    BigInteger hValue;
    BigInteger pValue;

    public TMP_Text path;
    public TMP_InputField G;
    public TMP_InputField H;
    public TMP_InputField A;
    public TMP_InputField P;

    void GenerateRandomData()
    {
        // We don't guarantee G number to be a generator of the multiplicative group
        pValue = GenerateRandomBigPrime(32);
        gValue = RandomBigIntInRange(2, pValue - 2);
        aValue = RandomBigIntInRange(2, pValue - 2);
        hValue = BigInteger.ModPow(gValue, aValue, pValue);

        G.text = gValue.ToString();
        H.text = hValue.ToString();
        A.text = aValue.ToString();
        P.text = pValue.ToString();
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

            BigInteger x = BigInteger.ModPow(a, d, n);

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

    BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger g = BigInteger.GreatestCommonDivisor(a, m);
        if (g != 1)
            throw new ArgumentException("Modular inverse does not exist");

        BigInteger x;
        BigInteger y;
        ExtendedEuclidean(a, m, out x, out y);

        return (x % m + m) % m;
    }

    void ExtendedEuclidean(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
    {
        if (b == 0)
        {
            x = 1;
            y = 0;
            return;
        }

        BigInteger x1;
        BigInteger y1;
        ExtendedEuclidean(b, a % b, out x1, out y1);

        x = y1;
        y = x1 - (a / b) * y1;
    }

    BigInteger InvModDecryption(BigInteger x, BigInteger a, BigInteger n, BigInteger b)
    {
        BigInteger a_to_n_mod_b = BigInteger.ModPow(a, n, b);
        BigInteger inverse = ModInverse(a_to_n_mod_b, b);
        BigInteger result = (x * inverse) % b;

        return result;
    }

    void SetKeyValuesFromUI()
    {
        gValue = BigInteger.Parse(G.text);
        aValue = BigInteger.Parse(A.text);
        pValue = BigInteger.Parse(P.text);
        hValue = BigInteger.ModPow(gValue, aValue, pValue);
    }

    public void Encrypt()
    {
        if (G.text.Length == 0 || A.text.Length == 0 || P.text.Length == 0)
            GenerateRandomData();
        else
            SetKeyValuesFromUI();

        _encryptedBlockArray = new byte[_blockArray.Length][];
        for (int i = 0; i < _blockArray.Length; i++)
        {
            _encryptedBlockArray[i] = SingleBlockEncrypting(_blockArray[i]);
        }

        _encryptedSize = Convert.ToUInt32(_blockArray.Length);
        _fileBytes = ConcatenateByteArrays(_encryptedBlockArray);
    }

    public void Decrypt()
    {
        if (G.text.Length == 0 || A.text.Length == 0 || P.text.Length == 0)
            GenerateRandomData();
        else
            SetKeyValuesFromUI();

        if (_encryptedSize == 0)
            _encryptedSize = Convert.ToUInt32(_blockArray.Length / 8);
        
        byte[] b = new byte[64];
        for (int i = 0; i < _encryptedSize; i++)
        {
            b = SingleBlockDecrypting(_encryptedBlockArray[i]);
            Array.Copy(b, 0, _blockArray[i], 0, 8);
        }

        _fileBytes = ConcatenateByteArrays(_blockArray);
    }

    byte[] SingleBlockEncrypting(byte[] block)
    {
        BigInteger m = ConvertBlockToNumber(block);
        BigInteger r = RandomBigIntInRange(1, pValue);
        BigInteger c1, c2;

        c1 = BigInteger.ModPow(gValue, r, pValue);
        c2 = BigInteger.ModPow(hValue, r, pValue);
        c2 = (m * c2) % pValue;
        byte[] b1 = ConvertNumberToBlock(c1, 32);
        byte[] b2 = ConvertNumberToBlock(c2, 32);

        byte[] outputBlock = new byte[64];

        Array.Copy(b1, 0, outputBlock, 0, 32);
        Array.Copy(b2, 0, outputBlock, 32, 32);

        return outputBlock;
    }

    byte[] SingleBlockDecrypting(byte[] block)
    {
        BigInteger c1, c2;
        byte[] firstHalf = new byte[32];
        byte[] secondHalf = new byte[32];

        Array.Copy(block, 0, firstHalf, 0, 32);
        Array.Copy(block, 32, secondHalf, 0, 32);

        c1 = ConvertBlockToNumber(firstHalf);
        c2 = ConvertBlockToNumber(secondHalf);

        BigInteger result = InvModDecryption(c2, c1, aValue, pValue);

        byte[] outputBlock = ConvertNumberToBlock(result, 64);
        return outputBlock;
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
            _blockArray = SplitTo8ByteBlocks(_fileBytes);
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

    byte[][] SplitTo8ByteBlocks(byte[] data)
    {
        // Calculate the number of 8-byte blocks needed
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

    public void Quit()
    {
        Application.Quit();
    }
}