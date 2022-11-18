using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Unity;

namespace InfoSequrity2
{
    public class CryptoLogic
    {
        //зависимость IoC - контейнера
        [Dependency]
        public IUnityContainer Container { get; set; }

        private const int sizeOfBlock = 128; //в DES размер блока 64 бит, но поскольку в unicode символ в два раза длинее, то увеличим блок тоже в два раза
        private const int sizeOfChar = 16; //размер одного символа (in Unicode 16 bit)

        private const int quantityOfRounds = 16; //количество раундов

        private int[] StartMatrix =
        {
            58,   50,  42,  34,  26,  18,  10,  2,
            60,  52,  44,  36,  28,  20,  12,  4,
            62,  54,  46,  38,  30,  22,  14,  6,
            64,  56,  48,  40,  32,  24,  16,  8,
            57,  49,  41,  33,  25,  17,  9,   1,
            59,  51,  43,  35,  27,  19,  11,  3,
            61,  53,  45,  37,  29,  21,  13,  5,
            63,  55,  47,  39, 31,  23,  15,  7,
        };

        private int[] EndMatrix =
        {
            40, 8,   48,  16,  56,  24,  64,  32,
            39,  7,   47,  15,  55,  23,  63,  31,
            38,  6,   46,  14,  54,  22,  62,  30,
            37,  5,   45,  13,  53,  21,  61,  29,
            36,  4,   44,  12,  52,  20,  60,  28,
            35,  3,   43,  11,  51,  19,  59,  27,
            34,  2,   42,  10,  50,  18,  58,  26,
            33,  1,   41,  9,   49,  17,  57,  25,
        };

        private int[] CoMatrix =
        {
            57,  49,  41,  33,  25,  17,  9,
            1,   58,  50,  42,  34,  26,  18,
            10,  2,   59,  51,  43,  35,  27,
            19,  11,  3,   60,  52,  44,  36,
            63,  55,  47,  39,  31,  23,  15 ,
            7,   62,  54,  46,  38,  30,  22,
            14,  6,   61,  53,  45,  37,  29,
            21,  13,  5,   28,  20,  12,  4
        };
        private int[] P =
        {
            16,    7,   20,  21,
            29,  12,  28,  17,
            1,   15,  23,  26 ,
            5 ,  18,  31,  10,
            2,   8,   24 , 14,
            0,  27,  3,   9,
            19,  13,  30,  6,
            22,  11,  4,   25,

        };
        private int[][][] S = new int[][][]
        {
            new int[][]
            {
                new int[] {14,  4,   13 , 1 ,  2 ,  15 , 11,  8 ,  3 ,  10 , 6 ,  12 , 5 ,  9 ,  0 ,  7 },
                new int[] {0,   15,  7,   4,   14,  2,   13,  1 ,  10,  6 ,  12 , 11 , 9 ,  5,   3,   8 },
                new int[] {4,   1,   14,  8,   13,  6,   2 ,  11,  15,  12,  9,   7,   3,   10,  5,   0 },
                new int[] {15,  12,  8,   2,   4 ,  9,   1,   7,   5,   11,  3,   14,  10,  0,   6 ,  13 }
            },
             new int[][]
            {
                new int[] {15,  1,   8,   14,  6,   11 , 3 ,  4 ,  9 ,  7 ,  2,   13,  12,  0 ,  5,   10 },
                new int[] {3,   13,  4,   7,   15 , 2 ,  8 ,  14,  12,  0,   1,   10,  6,   9,   11,  5 },
                new int[] {0,   14,  7,   11,  10 , 4,   13,  1,   5,   8,   12,  6 ,  9 ,  3 ,  2,   15 },
                new int[] {13,  8,   10 , 1,   3,   15,  4,   2,   11,  6 ,  7,   12 , 0,   5,  14,  9 }
            },
             new int[][]
             {
                 new int[] {10, 0,   9,   14,  6,   3,   15,  5,   1,   13,  12,  7,   11,  4,   2,   8 },
                 new int[] {13,  7,   0,   9,   3,   4,   6,   10,  2,   8,   5,   14,  12,  11,  15,  1 },
                 new int[] {13,  6,   4,   9,   8,   15,  3,   0,   11,  1,   2,   12,  5,   10,  14,  7 },
                 new int[] {1,   10,  13,  0,   6,   9,   8,   7,   4,   15,  14,  3,   11,  5,   2,   12 }
             },
              new int[][]
             {
                 new int[] {7,  13,  14,  3,   0,   6,   9 ,  10,  1,   2 ,  8,   5 ,  11 , 12,  4,   15 },
                 new int[] {13,  8 ,  11,  5 ,  6 ,  15,  0 ,  3,   4 ,  7 ,  2,   12,  1,   10,  14,  9 },
                 new int[] {10,  6,   9,   0,   12,  11 , 7,   13,  15,  1,   3,   14,  5,   2,   8 ,  4 },
                 new int[] {3 ,  15,  0,   6,   10,  1,   13,  8,   9,   4 ,  5 ,  11,  12,  7,   2,   14 }
             },
              new int[][]
              {
                  new int[] {2 ,12,  4,   1 ,  7 ,  10 , 11,  6,   8,   5 ,  3,   15,  13,  0,   14,  9 },
                  new int[] {14 , 11,  2,   12,  4,   7 ,  13,  1,   5,   0,   15,  10,  3,   9 ,  8 ,  6 },
                  new int[] {4 ,  2,   1 ,  11,  10,  13,  7,   8,   15,  9 ,  12 , 5 ,  6,   3,   0,   14 },
                  new int[] {11,  8,   12,  7,   1,   14,  2,   13,  6,   15,  0,   9,   10,  4,   5,   3 }
              },
               new int[][]
              {
                  new int[] {12,    1,   10,  15,  9,   2,   6 ,  8,   0,   13,  3,   4,   14,  7,   5,   11 },
                  new int[] {10,  15,  4 ,  2 ,  7,   12,  9,   5,   6,   1,   13,  14,  0,   11,  3,   8 },
                  new int[] {9,   14,  15,  5,   2,   8,   12,  3,   7,   0,   4,   10,  1,   13,  11,  6 },
                  new int[] {4 ,  3,   2,   12,  9 ,  5,   15,  10,  11,  14,  1 ,  7,   6,   0,   8,   13 }
              },
               new int[][]
               {
                     new int[] {4 ,   11,  2,   14,  15,  0 ,  8,   13,  3,   12,  9,   7,   5,   10,  6,   1 },
                     new int[] {13,  0,   11,  7,   4,   9,   1,   10,  14 , 3,   5,   12,  2,   15,  8,   6 },
                     new int[] {1,   4,   11,  13,  12,  3,   7,   14,  10,  15,  6,   8,   0,   5,   9,   2 },
                     new int[] {6,   11,  13 , 8 ,  1 ,  4 ,  10,  7 ,  9,   5,   0 ,  15,  14,  2,   3,   12 }
               },
               new int[][]
               {
                    new int[] {13,   2,   8,   4,   6,   15,  11,  1,   10,  9,   3,   14,  5,   0,   12,  7 },
                    new int[] {1,   15,  13,  8,   10,  3,   7,   4,   12,  5,   6,   11,  0,   14,  9,   2 },
                    new int[] {7,   11,  4,   1,   9,   12,  14,  2,   0,   6,   10,  13,  15,  3,   5,   8 },
                    new int[] {2,   1,   14,  7,   4 ,  10,  8 ,  13,  15 , 12,  9,   0,   3,   5,   6 ,  11 }
               }
        };
 
        private int[] KeyMatrix = new int[]
        {
            14 , 17,  11,  24,  1,   5 ,
            3 ,  28,  15,  6 ,  21,  10 ,
            23,  19,  12 , 4,   26,  8,
            16,  7 ,  27,  20,  13,  2,
            41 , 52,  31,  37,  47,  55,
            30 , 40 , 51,  45 , 33 , 48,
            44,  49,  39,  54,  34 , 53 ,
            46,  42 , 50,  36 , 29 , 32
        };

        private int[] Si = { 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1 };
        public CryptoLogic()
        {

        }
       
        public void Encode(string key, string inputFilePath, string outputFilePath)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new Exception("Входной файл не найден");
            }
            try
            {
                //считываем строку и задаем правильную длину
                string text = File.ReadAllText(inputFilePath);
                text = StringToRightLength(text);

                //получаем байтовое представление, а затем битовое
                byte[] strBytes = Encoding.UTF8.GetBytes(text);
                BitArray bits = new BitArray(strBytes);

                //массив итоговых битов, который мы запишем в файл
                BitArray writeBits = new BitArray(bits.Length);

                //получаем ключи
                BitArray[] keys = GetKeys(key);

                for (int i = 0; i < bits.Length; i += 64)
                {
                    //сначала IP подстановка по таблице
                    BitArray prev = StartReplacement(bits, i);
                    BitArray prevL = Copy(prev, 0, 32);
                    BitArray prevR = Copy(prev, 32, 32);

                    for (int j = 0; j < quantityOfRounds; j++)
                    {
                        BitArray L = prevR;
                        BitArray R = prevL.Xor(F(prevR, keys[j]));

                        prev = ConcatBytes(L, R);
                        prevL = L;
                        prevR = R;
                    }
                    BitArray iterRes = EndReplacement(prev, 0);
                    writeBits = Add(writeBits, i, iterRes);
                }

                byte[] byteArray = new byte[(int)Math.Ceiling((double)writeBits.Length / 8)];
                writeBits.CopyTo(byteArray, 0);
                File.WriteAllBytes(outputFilePath, byteArray);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private BitArray[] GetKeys(string key)
        {
            //сначала занесем ключи на 16 раундов
            //получаем ключ в битах (64) - 8 байт
            byte[] strBytesKey = Encoding.UTF8.GetBytes(key);
            //.Take(8).ToArray()
            //если ключ слишком маленький
            if (strBytesKey == null || strBytesKey.Length < 8)
            {
                byte[] newKey = new byte[8];
                for(int i = 0; i < newKey.Length; i++)
                {
                    if(i < strBytesKey.Length)
                    {
                        newKey[i] = strBytesKey[i];
                    }
                    else
                    {
                        newKey[i] = 0;
                    }
                }
                strBytesKey = newKey;
            }
            else
            {
                //если большой - берем первые 8
                strBytesKey = Encoding.UTF8.GetBytes(key).Take(8).ToArray();
            }
            BitArray K = new BitArray(strBytesKey);

            //получаем Co в битах
            BitArray C = new BitArray(28);
            BitArray D = new BitArray(28);
            //преобразуем по таблице Key в Co (части C и D)
            //1ый бит станет 57 из ключа
            for (int i = 0; i < 28; i++)
            {
                C[i] = K[CoMatrix[i]];
            }
            for (int i = 28; i < 56; i++)
            {
                D[i - 28] = K[CoMatrix[i]];
            }
            BitArray[] keys = new BitArray[quantityOfRounds];
            for (int i = 0; i < quantityOfRounds; i++)
            {
                C = KeyRoundLeft(C);
                D = KeyRoundLeft(D);

                K = ConcatBytes(C, D);
                K = KeyReplacement(K);
                keys[i] = K;
            }
            return keys;
        }
        public void Decode(string key, string inputFilePath, string outputFilePath)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new Exception("Входной файл не найден");
            }
            try
            {
                //получаем байтовое представление, а затем битовое
                byte[] strBytes = File.ReadAllBytes(inputFilePath);
                BitArray bits = new BitArray(strBytes);

                //массив итоговых битов, который мы запишем в файл
                BitArray writeBits = new BitArray(bits.Length);

                //получаем ключи
                BitArray[] keys = GetKeys(key);

                for (int i = bits.Length - 64; i >= 0 ; i -= 64)
                {
                    //сначала IP подстановка по таблице
                    BitArray prev = StartReplacement(bits, i);
                    BitArray prevL = Copy(prev, 0, 32);
                    BitArray prevR = Copy(prev, 32, 32);

                    for (int j = quantityOfRounds - 1; j >= 0; j--)
                    {
                        BitArray R = prevL;
                        BitArray L = prevR.Xor(F(prevL, keys[j]));

                        prev = ConcatBytes(L, R);
                        prevL = L;
                        prevR = R;
                    }
                    BitArray iterRes = EndReplacement(prev, 0);
                    writeBits = Add(writeBits, i, iterRes);
                }

                byte[] byteArray = new byte[(int)Math.Ceiling((double)writeBits.Length / 8)];
                writeBits.CopyTo(byteArray, 0);
                
                //если мы дописывали символы в конец файла - нужно их убрать
                File.WriteAllBytes(outputFilePath, GetFilterBytes(byteArray));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private byte[] GetFilterBytes(byte[] arr)
        {
            int sharpCount = 0;
            for (int i = arr.Length - 1; i >=0 ; i--)
            {
                //35 - символ решетки
                if (arr[i] == 35)
                {
                    sharpCount++;
                }
            }
            byte[] res = new byte[arr.Length - sharpCount];
            for(int i = 0; i < res.Length; i++)
            {
                res[i] = arr[i];
            }
            return res;
        }
        private BitArray KeyReplacement(BitArray key)
        {
            BitArray res = new BitArray(48);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = key[KeyMatrix[i]];
            }
            return res;
        }

        private BitArray StartReplacement(BitArray array, int startIndex)
        {
            BitArray res = new BitArray(64);
            for(int i = 0; i < res.Length; i++)
            {
                res[i] = array[startIndex + StartMatrix[i] -1];
            }
            return res;
        }

        private BitArray EndReplacement(BitArray array, int startIndex)
        {
            BitArray res = new BitArray(64);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = array[startIndex + EndMatrix[i] - 1 ];
            }
            return res;
        }
        private BitArray F(BitArray R, BitArray K)
        {
            //сначала расширяем R до 48 битов
            BitArray newR48 = new BitArray(48);

            //расширяем
            for(int i = 0; i < R.Length; i+=4)
            {
                int newI = (int)(i * 1.5);

                newR48[newI] = i - 1 < 0 ? R[R.Length-1] : R[i-1];

                newR48[newI + 1] = R[i];
                newR48[newI + 2] = R[i + 1];
                newR48[newI + 3] = R[i + 2];
                newR48[newI + 4] = R[i + 3];
                //заполняем следующим первым значением блока или первым, если блок последний
                newR48[newI + 5] = i + 4 >= R.Length ? R[0] : R[i + 4];

            }
            BitArray xor = newR48.Xor(K);
            //инициализируем s-боксы
            BitArray sBoxes = new BitArray(32);
            //проходимся по результату XOR и выносим данные из таблиц S
            for (int i = 0; i < sBoxes.Length; i += 4)
            {
                int resI = (int)(i * 1.5);
                BitArray sBox = GetSValue(xor, resI, i/4);
                sBoxes[i] = sBox[0];
                sBox[i + 1] = sBox[1];
                sBox[i + 2] = sBox[2];
                sBox[i + 3] = sBox[3];
            }

            BitArray res = new BitArray(32);
            for(int i = 0; i < res.Length; i++)
            {
                res[i] = sBoxes[P[i]];
            }
            return res;
        }

        private BitArray GetSValue(BitArray array, int startIndex, int boxCount)
        {
            int row = GetNum(array[startIndex]) * 2 + GetNum(array[startIndex + 5]);
            int column = GetNum(array[startIndex + 1]) * 8 + GetNum(array[startIndex + 2])* 4
                + GetNum(array[startIndex + 4]) * 2 + GetNum(array[startIndex + 4]);
            int num = S[boxCount][row][column];

            byte[] numBytes = BitConverter.GetBytes(num);
            BitArray bits = new BitArray(numBytes);
            return bits;
        }
        private int GetNum(bool bit)
        {
            return bit ? 1 : 0;
        }

        private BitArray Add(BitArray array, int index, BitArray addArr)
        {
            for (int i = index; i < addArr.Length + index; i++)
            {
                array[i] = addArr[i - index];
            }
            return array;
        }

        //возвращает битовый массив указанной длины, копируя из базового значения с определенного индекса
        private BitArray Copy(BitArray array, int index, int lenth)
        {
            BitArray res = new BitArray(lenth);
            
            for(int i = index; i < lenth + index; i++)
            {
                res[i - index] = array[i];
            }
            return res;
        }

        private BitArray KeyRoundLeft(BitArray key)
        {
            bool firstVal = key[0];
            for(int i = 0; i < key.Length - 1; i++)
            {
                key[i] = key[i + 1];
            }
            key[key.Length - 1] = firstVal;

            return key;
        }

        private BitArray ConcatBytes(BitArray first, BitArray second)
        {
            BitArray res = new BitArray(first.Length + second.Length);
            for(int i = 0; i < first.Length; i++)
            {
                res[i] = first[i];
            }
            for (int i = first.Length; i < first.Length + second.Length; i++)
            {
                res[i] = second[i - first.Length];
            }
            return res;
        }

        private string StringToRightLength(string input)
        {
            while (((input.Length * sizeOfChar) % sizeOfBlock) != 0)
                input += "#";

            return input;
        }

        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
