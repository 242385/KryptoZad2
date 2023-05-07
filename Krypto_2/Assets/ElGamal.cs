using System;
using System.Collections;
using AnotherFileBrowser.Windows;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class ElGamal : MonoBehaviour
{
    byte[] _fileBytes;
    byte[][] _blockArray;
    private byte[] _output;
    byte[] _leftHalf = new byte[4];
    byte[] _rightHalf = new byte[4];
    string exampleKey1 = "P1w3rk0!";
    string exampleKey2 = "R0mP3r3k";
    string exampleKey3 = "H4rN0lD!";
    string exampleNValue = "H4rN0lD!";
    
    public TMP_Text path;
    public TMP_InputField key1Field;   
    public TMP_InputField key2Field;   
    public TMP_InputField key3Field;
    
    BitArray _bitKey;
    BitArray _LPT;
    BitArray _RPT;

    int[] _initialPermutation = new int[]
    {
        58, 50, 42, 34, 26, 18, 10, 2,
        60, 52, 44, 36, 28, 20, 12, 4,
        62, 54, 46, 38, 30, 22, 14, 6,
        64, 56, 48, 40, 32, 24, 16, 8,
        57, 49, 41, 33, 25, 17, 9, 1,
        59, 51, 43, 35, 27, 19, 11, 3,
        61, 53, 45, 37, 29, 21, 13, 5,
        63, 55, 47, 39, 31, 23, 15, 7
    };

    int[] _inverseInitialPermutation = new int[]
    {
        40, 8, 48, 16, 56, 24, 64, 32,
        39, 7, 47, 15, 55, 23, 63, 31,
        38, 6, 46, 14, 54, 22, 62, 30,
        37, 5, 45, 13, 53, 21, 61, 29,
        36, 4, 44, 12, 52, 20, 60, 28,
        35, 3, 43, 11, 51, 19, 59, 27,
        34, 2, 42, 10, 50, 18, 58, 26,
        33, 1, 41, 9, 49, 17, 57, 25
    };

    int[] _pc1 = new int[]
    {
        57, 49, 41, 33, 25, 17, 9,
        1, 58, 50, 42, 34, 26, 18,
        10, 2, 59, 51, 43, 35, 27,
        19, 11, 3, 60, 52, 44, 36,
        63, 55, 47, 39, 31, 23, 15,
        7, 62, 54, 46, 38, 30, 22,
        14, 6, 61, 53, 45, 37, 29,
        21, 13, 5, 28, 20, 12, 4
    };

    int[] _pc2 = new int[]
    {
        14, 17, 11, 24, 1, 5,
        3, 28, 15, 6, 21, 10,
        23, 19, 12, 4, 26, 8,
        16, 7, 27, 20, 13, 2,
        41, 52, 31, 37, 47, 55,
        30, 40, 51, 45, 33, 48,
        44, 49, 39, 56, 34, 53,
        46, 42, 50, 36, 29, 32
    };

    int[] _eSelection = new int[]
    {
        32, 1, 2, 3, 4, 5,
        4, 5, 6, 7, 8, 9,
        8, 9, 10, 11, 12, 13,
        12, 13, 14, 15, 16, 17,
        16, 17, 18, 19, 20, 21,
        20, 21, 22, 23, 24, 25,
        24, 25, 26, 27, 28, 29,
        28, 29, 30, 31, 32, 1
    };

    int[] _pPermutation =
    {
        16, 7, 20, 21,
        29, 12, 28, 17,
        1, 15, 23, 26,
        5, 18, 31, 10,
        2, 8, 24, 14,
        32, 27, 3, 9,
        19, 13, 30, 6,
        22, 11, 4, 25
    };


    // S-boxes:
    int[,] S1 = new int[,]
    {
        { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 },
        { 0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8 },
        { 4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0 },
        { 15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13 }
    };

    int[,] S2 = new int[,]
    {
        { 15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10 },
        { 3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5 },
        { 0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15 },
        { 13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9 }
    };

    int[,] S3 = new int[,]
    {
        { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8 },
        { 13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1 },
        { 13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7 },
        { 1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12 }
    };

    int[,] S4 = new int[,]
    {
        { 7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15 },
        { 13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9 },
        { 10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4 },
        { 3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14 }
    };

    int[,] S5 = new int[,]
    {
        { 2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9 },
        { 14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6 },
        { 4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14 },
        { 11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3 }
    };

    int[,] S6 = new int[4, 16]
    {
        { 12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11 },
        { 10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8 },
        { 9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6 },
        { 4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13 }
    };

    int[,] S7 = new int[4, 16]
    {
        { 4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1 },
        { 13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6 },
        { 1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2 },
        { 6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12 }
    };

    int[,] S8 = new int[4, 16]
    {
        { 13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7 },
        { 1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2 },
        { 7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8 },
        { 2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11 }
    };

    public void Quit()
    {
        Application.Quit();
    }
    
    public void Encrypt()
    {
        if (key1Field.text.Length == 0)
            key1Field.text = exampleKey1;
        
        if (key2Field.text.Length == 0)
            key2Field.text = exampleKey2;
        
        if (key3Field.text.Length == 0)
            key3Field.text = exampleKey3;

        key1Field.text = key1Field.text.PadRight(8, '0');
        key2Field.text = key2Field.text.PadRight(8, '0');
        key3Field.text = key3Field.text.PadRight(8, '0');

        exampleKey1 = key1Field.text;
        exampleKey2 = key2Field.text;
        exampleKey3 = key3Field.text;
        
        Debug.Log(exampleKey1);
        
        // 3DES
        for (int i = 0; i < _blockArray.Length; i++)
        {
            _blockArray[i] = SingleBlockEncrypting(_blockArray[i], exampleKey1, false);
        }

        for (int i = 0; i < _blockArray.Length; i++)
        {
            _blockArray[i] = SingleBlockEncrypting(_blockArray[i], exampleKey2, true);
        }

        for (int i = 0; i < _blockArray.Length; i++)
        {
            _blockArray[i] = SingleBlockEncrypting(_blockArray[i], exampleKey3, false);
        }

        _fileBytes = ConcatenateByteArrays(_blockArray);
    }

    public void Decrypt()
    {
        if (key1Field.text.Length == 0)
            key1Field.text = exampleKey1;
        
        if (key2Field.text.Length == 0)
            key2Field.text = exampleKey2;
        
        if (key3Field.text.Length == 0)
            key3Field.text = exampleKey3;

        key1Field.text = key1Field.text.PadRight(8, '0');
        key2Field.text = key2Field.text.PadRight(8, '0');
        key3Field.text = key3Field.text.PadRight(8, '0');

        exampleKey1 = key1Field.text;
        exampleKey2 = key2Field.text;
        exampleKey3 = key3Field.text;
        
        Debug.Log(exampleKey1);
        
        // 3DES
        for (int i = 0; i < _blockArray.Length; i++)
        {
            _blockArray[i] = SingleBlockEncrypting(_blockArray[i], exampleKey3, true);
        }

        for (int i = 0; i < _blockArray.Length; i++)
        {
            _blockArray[i] = SingleBlockEncrypting(_blockArray[i], exampleKey2, false);
        }

        for (int i = 0; i < _blockArray.Length; i++)
        {
            _blockArray[i] = SingleBlockEncrypting(_blockArray[i], exampleKey1, true);
        }

        _fileBytes = ConcatenateByteArrays(_blockArray);
    }

    byte[] SingleBlockEncrypting(byte[] block, string key, bool isDecrypting)
    {
        _output = new byte[8];
        // Initial permutation
        for (int i = 0; i < _initialPermutation.Length; i++)
        {
            // Calculate the bit index and byte index for the input array
            int bitIndex = _initialPermutation[i] - 1;
            int byteIndex = bitIndex / 8;
            int offset = bitIndex % 8;

            // Extract the bit value from the input array and append it to the output array
            byte bitValue = (byte)((block[byteIndex] >> (7 - offset)) & 1);
            _output[i / 8] |= (byte)(bitValue << (7 - (i % 8)));
        }

        // Splitting into two halves
        _leftHalf = new byte[4];
        _rightHalf = new byte[4];

        Array.Copy(_output, 0, _leftHalf, 0, 4);
        Array.Copy(_output, 4, _rightHalf, 0, 4);

        byte[] byteKey = ConvertStringToBytes(key);
        int numOfBytes = (_pc1.Length + 7) / 8; // calculate the number of bytes in the output
        byte[] pc1Key = new byte[numOfBytes];

        // PC1
        for (int i = 0; i < _pc1.Length; i++)
        {
            int pc1BitPos = _pc1[i] - 1; // convert from 1-based index to 0-based index
            int bytePos = pc1BitPos / 8;
            int bitPos = 7 - (pc1BitPos % 8);
            byte mask = (byte)(1 << bitPos);

            if ((byteKey[bytePos] & mask) != 0)
            {
                pc1Key[i / 8] |= (byte)(1 << (7 - (i % 8)));
            }
        }

        _bitKey = new BitArray(pc1Key.Length * 8);
        for (int i = 0; i < pc1Key.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _bitKey.Set(i * 8 + j, (pc1Key[i] & (1 << (7 - j))) != 0);
            }
        }

        int halfLengthBits = _bitKey.Length / 2;
        _LPT = new BitArray(halfLengthBits);
        _RPT = new BitArray(halfLengthBits);

        for (int i = 0; i < halfLengthBits; i++)
        {
            _LPT[i] = _bitKey[i];
            _RPT[i] = _bitKey[halfLengthBits + i];
        }

        return Algorithm(_bitKey, _LPT, _RPT, isDecrypting);
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

    byte[] Algorithm(BitArray key, BitArray LPT, BitArray RPT, bool isDecrypting)
    {
        // Arrays of shifted key halves and concatenated values, shifting them. Then they will be used for generating 48-bit keys
        BitArray[] arrayC = new BitArray[16];
        BitArray[] arrayD = new BitArray[16];
        BitArray[] concatenatedValues = new BitArray[16];

        arrayC[0] = LeftShift(LPT, 1);
        arrayD[0] = LeftShift(RPT, 1);
        concatenatedValues[0] = key;

        int n;
        for (int i = 1; i < 16; i++)
        {
            n = (i == 1 || i == 8 || i == 15) ? 1 : 2;
            arrayC[i] = LeftShift(arrayC[i - 1], n);
            arrayD[i] = LeftShift(arrayD[i - 1], n);
            concatenatedValues[i] = ConcatenateBitArrays(arrayC[i], arrayD[i]);
        }

        // Perform PC2 permutation on each BitArray
        BitArray[] subKeys = new BitArray[16];

        for (int i = 0; i < 16; i++)
        {
            subKeys[i] = new BitArray(48);
            BitArray k = concatenatedValues[i];
            for (int j = 0; j < 48; j++)
            {
                subKeys[i].Set(j, concatenatedValues[i].Get(_pc2[j] - 1));
            }
        }

        BitArray lnMinus1 = BytesToBitArray(_leftHalf);
        BitArray rnMinus1 = BytesToBitArray(_rightHalf);
        BitArray ln = new BitArray(32);
        BitArray rn = new BitArray(32);
        // Perform actual Feistel operations

        if (isDecrypting)
        {
            for (int i = 16; i >= 1; i--)
            {
                ln = rnMinus1;
                rn = XOR(lnMinus1, FeistelFunctions(rnMinus1, subKeys[i - 1]));
                lnMinus1 = ln;
                rnMinus1 = rn;
            }
        }
        else
        {
            for (int i = 1; i <= 16; i++)
            {
                ln = rnMinus1;
                rn = XOR(lnMinus1, FeistelFunctions(rnMinus1, subKeys[i - 1]));
                lnMinus1 = ln;
                rnMinus1 = rn;
            }
        }


        // Apply reverse of initial permutation
        BitArray afterFeistel = ConcatenateBitArrays(rn, ln);
        BitArray result = new BitArray(64);
        for (int i = 0; i < 64; i++)
        {
            result[i] = afterFeistel[_inverseInitialPermutation[i] - 1];
        }

        // Convert result to bytes
        byte[] bytes = new byte[result.Count / 8];

        int byteIndex = 0;
        int bitIndex = 0;
        byte currentByte = 0;

        for (int i = 0; i < result.Count; i++)
        {
            currentByte <<= 1;
            currentByte |= (byte)(result[i] ? 1 : 0);
            bitIndex++;

            if (bitIndex == 8)
            {
                bytes[byteIndex] = currentByte;
                byteIndex++;
                bitIndex = 0;
                currentByte = 0;
            }
        }

        return bytes;
    }

    BitArray FeistelFunctions(BitArray Rn_less1, BitArray Kn)
    {
        // E bit-selection
        BitArray eSelection48 = new BitArray(48);
        for (int i = 0; i < 48; i++)
        {
            eSelection48[i] = Rn_less1[_eSelection[i] - 1];
        }

        // XOR the output from E bit-selection
        BitArray XORedOutput = XOR(Kn, eSelection48);
        BitArray[] chunksOf6Bits = new BitArray[8];

        for (int i = 0; i < 8; i++)
        {
            bool[] sixBits = new bool[6];
            for (int j = 0; j < 6; j++)
            {
                sixBits[j] = XORedOutput[i * 6 + j];
            }

            chunksOf6Bits[i] = new BitArray(sixBits);
        }

        // Performing SBox function on 8 6-bit chunks and converting them into 4-bit chunks
        BitArray concatenatedResultAfterSBoxes = new BitArray(32);
        BitArray[] chunksOf4Bits = new BitArray[8];
        int[,] chosenSBox = S1;
        for (int i = 0; i < 8; i++)
        {
            switch (i)
            {
                case 0:
                    chosenSBox = S1;
                    break;
                case 1:
                    chosenSBox = S2;
                    break;
                case 2:
                    chosenSBox = S3;
                    break;
                case 3:
                    chosenSBox = S4;
                    break;
                case 4:
                    chosenSBox = S5;
                    break;
                case 5:
                    chosenSBox = S6;
                    break;
                case 6:
                    chosenSBox = S7;
                    break;
                case 7:
                    chosenSBox = S8;
                    break;
            }

            chunksOf4Bits[i] = SBoxFunction(chosenSBox, chunksOf6Bits[i]);
        }

        for (int k = 0; k < chunksOf4Bits.Length; k++)
        {
            for (int j = 0; j < 4; j++)
            {
                concatenatedResultAfterSBoxes.Set(k * 4 + j, chunksOf4Bits[k].Get(j));
            }
        }

        // Perform P permutation
        BitArray permutationP = new BitArray(32);
        for (int i = 0; i < 32; i++)
        {
            permutationP[i] = concatenatedResultAfterSBoxes[_pPermutation[i] - 1];
        }

        return permutationP;
    }

    BitArray SBoxFunction(int[,] sBox, BitArray chunk)
    {
        BitArray result = new BitArray(4);

        // Calculating y position in SBOX
        bool y1 = chunk.Get(0);
        bool y2 = chunk.Get(chunk.Length - 1);
        BitArray posY = new BitArray(2);
        posY.Set(0, y1);
        posY.Set(1, y2);

        // Calculating x position in SBOX
        bool[] xarray = { chunk.Get(1), chunk.Get(2), chunk.Get(3), chunk.Get(4) };
        BitArray posX = new BitArray(4);
        for (int i = 0; i < posX.Length; i++)
        {
            posX.Set(i, xarray[i]);
        }

        int x = BitArrayToInt(posX);
        int y = BitArrayToInt(posY);

        result = IntToBitArray(sBox[y, x], 4);
        return result;
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
}