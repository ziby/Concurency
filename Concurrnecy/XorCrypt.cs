using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Concurrnecy
{
    public class XorCrypt
    {
        
        private static void AtomicWrite (Stream stream, byte[] buffer, int offset, int count)
        {
            lock (stream)
            {
                long position = stream.Position;
                stream.Position = offset;
                stream.Write(buffer, offset, count);
                stream.Position = position;
            }
        }

        private static List<byte> ReadFromStream(Stream stream)
        {
            byte b = 0;
            List<byte> buffer = new List<byte>();
            do
            {
                while (true)
                {
                    int res;
                    lock (stream)
                    {
                        res = stream.ReadByte();
                    }
                    if (res == -1) Thread.Sleep(20);
                    else
                    {
                        b = (byte)res;
                        break;
                    }
                }
                buffer.Add(b);
            } while (b != 0);
            return buffer;
        }

        public static List<byte> Crypt()
        {
            MemoryStream dataMemoryStream = new MemoryStream();
            MemoryStream keyMemoryStream = new MemoryStream();

            // делегаты, моделирующие запись в поток
            WriterOnStream dataWriter = dataBinWr =>
            {
                Thread.Sleep(500);
                byte[] buffer = {12, 32, 65, 150, 13, 56, 11, 32, 201, 11, 25, 7, 16, 0};
                AtomicWrite(dataBinWr, buffer, 0, 3);
                Thread.Sleep(200);
                AtomicWrite(dataBinWr, buffer, 3, 8);
                Thread.Sleep(300);
                AtomicWrite(dataBinWr, buffer, 11, 2);
                Thread.Sleep(600);
                AtomicWrite(dataBinWr, buffer, 13, 1);
            };

            WriterOnStream keyWriter = keyBinWr =>
            {
                byte[] buffer = { 11, 43, 15, 200, 3, 6, 32, 1, 5, 8, 154, 10, 4, 0 };
                Thread.Sleep(800);
                AtomicWrite(keyBinWr, buffer, 0, 6);
                Thread.Sleep(100);
                AtomicWrite(keyBinWr, buffer, 6, 5);
                Thread.Sleep(200);
                AtomicWrite(keyBinWr, buffer, 11, 2);
                Thread.Sleep(400);
                AtomicWrite(keyBinWr, buffer, 13, 1);
            };

            ReaderFromStream readData = ReadFromStream;
            ReaderFromStream readKey = ReadFromStream;

            // запуск потоков
            IAsyncResult dataResult = dataWriter.BeginInvoke(dataMemoryStream, null, null);
            IAsyncResult keyResult = keyWriter.BeginInvoke(keyMemoryStream, null, null);
            IAsyncResult dataAR = readData.BeginInvoke(dataMemoryStream, null, null);
            IAsyncResult keyAR = readKey.BeginInvoke(keyMemoryStream, null, null);

            // ожидание потоков
            dataWriter.EndInvoke(dataResult);
            keyWriter.EndInvoke(keyResult);
            List<byte> data = readData.EndInvoke(dataAR);
            List<byte> key = readKey.EndInvoke(keyAR);

            int cryptLength = data.Count;
            if (cryptLength != key.Count)
            {
                Console.WriteLine("Внимание! Потоки неодинаковой длины");
                if (cryptLength > key.Count) cryptLength = key.Count;
            }

            List<byte> cryptedBytes = new List<byte>();
            for (int i = 0; i < key.Count; i++)
            {
                cryptedBytes.Add((byte)(data[i] ^ key[i]));
            }
            return cryptedBytes;
        }
    }
}
