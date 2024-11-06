using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PontoLidar
{
    public uint ID;
    public float Range;//Cm
                       //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
    public float Ang;//Rad
}


public struct Scan
{
    public uint ID;
    public DateTime Tempo;
    public Vector3 PosSLAM;
    public int Len;
}



public class SistemaEncodeDecode
{
    public unsafe static byte[] EncodeStructArrayToByteArray<T>(T[] structArray) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        byte[] byteArray = new byte[structArray.Length * size];

        fixed (byte* arr = byteArray)
        {
            fixed (T* p = structArray)
            {
                Buffer.MemoryCopy(p, arr, structArray.Length * size, structArray.Length * size);
            }
        }

        return byteArray;
    }

    public unsafe static T[] DecodeByteArrayToStructArray<T>(byte[] byteArray, int lenByteArr) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        int length = lenByteArr / size;
        T[] structArray = new T[length];

        fixed (byte* arr = byteArray)
        {
            fixed (T* p = structArray)
            {
                Buffer.MemoryCopy(arr, p, lenByteArr, lenByteArr);
            }
        }


        return structArray;
    }




    public unsafe static byte[] EncodeStructToByteArray<T>(T dado) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        byte[] byteArray = new byte[size];
        fixed (byte* arr = byteArray)
        {
            T* p = &dado;

            //fixed (PontoLidar* p = &dado)
            {
                Buffer.MemoryCopy(p, arr, size, size);
            }
        }

        return byteArray;
    }

    public unsafe static T DecodeByteArrayToStruct<T>(byte[] byteArray, int size) where T : struct
    {
        //int size = Marshal.SizeOf(typeof(T));
        //int size = byteArray.Length;
        T structArray = new T();

        fixed (byte* arr = byteArray)
        {
            T* p = &structArray;
            // {
            Buffer.MemoryCopy(arr, p, size, size);
            // }
        }


        return structArray;
    }



}
public class SistemaSessaoScans
{
    public static string NomePastaScans = "Scans";
    public string PastaScan = "";
    public uint UltIDX = 0;

    public FileStream fileStreamScans;
    public FileStream fileStreamPontos;


    private int sizeScan = Marshal.SizeOf(typeof(Scan));
    private int sizePonto = Marshal.SizeOf(typeof(PontoLidar));


    public void CriarSessao(string Nome)
    {
        sizeScan = Marshal.SizeOf(typeof(Scan));
        sizePonto = Marshal.SizeOf(typeof(PontoLidar));

        PastaScan = Path.Join(NomePastaScans, Nome);

        //Criamos pasta
        Directory.CreateDirectory(PastaScan);
        fileStreamScans = new FileStream(Path.Join(PastaScan, "scans"), FileMode.Create, FileAccess.Write);
        fileStreamPontos = new FileStream(Path.Join(PastaScan, "pontos"), FileMode.Create, FileAccess.Write);

        UltIDX = 0;
    }

    public uint LerSessao(string Nome)
    {
        sizeScan = Marshal.SizeOf(typeof(Scan));
        sizePonto = Marshal.SizeOf(typeof(PontoLidar));

        PastaScan = Path.Join(NomePastaScans, Nome);
        fileStreamScans = new FileStream(Path.Join(PastaScan, "scans"), FileMode.Open, FileAccess.Read);
        fileStreamPontos = new FileStream(Path.Join(PastaScan, "pontos"), FileMode.Open, FileAccess.Read);
        UltIDX = 0;
        return (uint)fileStreamScans.Length / (uint)sizeScan;
    }


    public Scan LerScan()
    {
        byte[] scan = new byte[sizeScan];

        fileStreamScans.Read(scan, 0, sizeScan);// (int)UltIDX * sizeScan //UltIDX += 1;
        
        return SistemaEncodeDecode.DecodeByteArrayToStruct<Scan>(scan, sizeScan);
    }
    public PontoLidar[] LerPontosScan(int lenPontos)
    {
        byte[] pontos = new byte[sizePonto * lenPontos];

        fileStreamPontos.Read(pontos, 0, sizePonto * lenPontos);// LenPontosLidos + (int)UltIDX * sizePonto
        return SistemaEncodeDecode.DecodeByteArrayToStructArray<PontoLidar>(pontos, pontos.Length);


    }
    public void AdicionarPontos(PontoLidar[] pontos, Vector3 PosSLAM)
    {
        Scan scan = new();

        scan.ID = UltIDX;
        scan.Tempo = DateTime.UtcNow;
        scan.PosSLAM = PosSLAM;
        scan.Len = pontos.Length;

        fileStreamScans.Write(SistemaEncodeDecode.EncodeStructToByteArray(scan));
        fileStreamPontos.Write(SistemaEncodeDecode.EncodeStructArrayToByteArray(pontos));

        UltIDX += 1;
    }


    public void Close()
    {
        fileStreamScans.Flush();
        fileStreamPontos.Flush();

        fileStreamScans.Close();
        fileStreamPontos.Close();
    }
}