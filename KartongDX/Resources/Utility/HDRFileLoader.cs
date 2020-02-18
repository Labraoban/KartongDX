using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Resources.Utility
{
    // Based on https://www.flipcode.com/archives/HDR_Image_Reader.shtml
    //


    enum HDRFormat
    {
        _32_bit_rle_rgb,
        other
    }

    struct HDRHeader
    {
        public float gamma;
        public List<int> primaries;
        public int x;
        public int y;
    }

    struct RGBE
    {
        public int r;
        public int g;
        public int b;
        public int e;
    }

    class HDRFileLoader
    {
        private const int MINELEN = 8;
        private const int MAXELEN = 0x7ffff;

        public static void LoadHDRFile(string filename)
        {
            HDRHeader header = new HDRHeader();
            header.gamma = 1;
            header.primaries = new List<int>();
            header.x = 2048;
            header.y = 4096;

            System.IO.FileStream stream = System.IO.File.OpenRead(filename);
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 4);
            Logger.Write(LogType.Debug, buffer.ToString());

            RGBE[] scanline = new RGBE[header.x];
            for (int y = header.x - 1; y >= 0; y--)
            {
                if (Decrunch(scanline, header.x, stream) == false)
                {

                }
            }
        }

        private static bool Decrunch(RGBE[] scanline, int len, System.IO.FileStream stream)
        {
            int i, j;
            if(len < MINELEN || len > MAXELEN)
            {
                // old decrunch
            }
            i = stream.ReadByte();
            if(i != 2)
            {
                stream.Position = -1;
            }

            scanline[0].g = stream.ReadByte();
            scanline[0].b = stream.ReadByte();
            i =  stream.ReadByte();

            //if(scanline[0].g != 2 || scanline[0].b & 128)
            //{
            //    scanline[0].r = 2;
            //    scanline[0].e = i;
            //}

            //for(i = 0; i < 4; ++i)
            //{
            //    for(j = 0; j < len;)
            //    {
            //        unsigned char code = stream.ReadByte();
            //        if(code > 128)
            //        {
            //            code &= 127;
            //            unsigned char val = stream.ReadByte();
            //            while(code--)
            //                scanline[]
            //        }
            //    }
            //}
            return false;
        }


        private static void ExtractHeader(string filename)
        {
            System.IO.FileStream stream = System.IO.File.OpenRead(filename);




            stream.Close();
        }
    }
}
