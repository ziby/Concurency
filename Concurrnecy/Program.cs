using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Concurrnecy
{
    public delegate void WriterOnStream(Stream stream);
    public delegate List<byte> ReaderFromStream(Stream stream);


    internal class Program
    {
        private static void Main(string[] args)
        {
            List<byte> cryptedBytes = XorCrypt.Crypt();
            foreach (var b in cryptedBytes)
            {
                Console.Write("{0} ", b);
            }
            Console.ReadKey();
        }
    }
}
