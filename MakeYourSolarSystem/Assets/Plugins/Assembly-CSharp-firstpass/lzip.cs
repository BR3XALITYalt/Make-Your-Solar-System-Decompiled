// Reference: ICSharpCode_SharpZipLib.dll
// Add to your .csproj:
//   <Reference Include="ICSharpCode_SharpZipLib">
//     <HintPath>path\to\ICSharpCode_SharpZipLib.dll</HintPath>
//   </Reference>

using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

public class lzip
{
    public static List<string> ninfo = new List<string>();
    public static List<long> uinfo = new List<long>();
    public static List<long> cinfo = new List<long>();
    public static int zipFiles = 0;
    public static int zipFolders = 0;
    public static int cProgress = 0;

    // -------------------------------------------------------------------------
    // Archive inspection
    // -------------------------------------------------------------------------

    public static int getTotalFiles(string zipArchive, byte[] FileBuffer = null)
    {
        ZipFile zf = FileBuffer != null
            ? new ZipFile(new MemoryStream(FileBuffer))
            : new ZipFile(zipArchive);

        try { return (int)zf.Count; }
        finally { zf.Close(); }
    }

    public static long getFileInfo(string zipArchive, string path, byte[] FileBuffer = null)
    {
        ninfo.Clear();
        uinfo.Clear();
        cinfo.Clear();
        zipFiles = 0;
        zipFolders = 0;

        ZipFile zf = null;
        try
        {
            zf = FileBuffer != null
                ? new ZipFile(new MemoryStream(FileBuffer))
                : new ZipFile(zipArchive);

            string prefix = (path ?? "").Replace('\\', '/').TrimStart('/');
            long total = 0L;

            foreach (ZipEntry entry in zf)
            {
                string name = entry.Name.Replace('\\', '/');

                if (prefix.Length > 0 &&
                    !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                long uSize = entry.Size;
                long cSize = entry.CompressedSize;

                ninfo.Add(name);
                uinfo.Add(uSize);
                cinfo.Add(cSize);
                total += uSize;

                if (entry.IsDirectory) zipFolders++;
                else zipFiles++;
            }

            return total;
        }
        catch { return -1L; }
        finally
        {
            if (zf != null) zf.Close();
        }
    }

    public static int getEntrySize(string zipArchive, string entry, byte[] FileBuffer = null)
    {
        ZipFile zf = null;
        try
        {
            zf = FileBuffer != null
                ? new ZipFile(new MemoryStream(FileBuffer))
                : new ZipFile(zipArchive);

            ZipEntry ze = zf.GetEntry(entry);
            return ze == null ? -1 : (int)ze.Size;
        }
        catch { return -1; }
        finally
        {
            if (zf != null) zf.Close();
        }
    }

    // -------------------------------------------------------------------------
    // Buffer compress / decompress  (raw Deflate)
    // -------------------------------------------------------------------------

    public static bool compressBuffer(byte[] source, ref byte[] outBuffer, int levelOfCompression)
    {
        byte[] result = compressBuffer(source, levelOfCompression);
        if (result == null) return false;
        outBuffer = result;
        return true;
    }

    public static int compressBufferFixed(byte[] source, ref byte[] outBuffer,
                                          int levelOfCompression, bool safe = true)
    {
        byte[] result = compressBuffer(source, levelOfCompression);
        if (result == null) return 0;
        if (result.Length > outBuffer.Length)
        {
            if (safe) return 0;
            Buffer.BlockCopy(result, 0, outBuffer, 0, outBuffer.Length);
            return outBuffer.Length;
        }
        Buffer.BlockCopy(result, 0, outBuffer, 0, result.Length);
        return result.Length;
    }

    public static byte[] compressBuffer(byte[] source, int levelOfCompression)
    {
        int level = Clamp(levelOfCompression, 0, 9);
        MemoryStream ms = new MemoryStream();
        DeflaterOutputStream dos = new DeflaterOutputStream(ms, new Deflater(level));
        try
        {
            dos.Write(source, 0, source.Length);
            dos.Finish();
            return ms.ToArray();
        }
        catch { return null; }
        finally
        {
            dos.Close();
            ms.Close();
        }
    }

    public static bool decompressBuffer(byte[] source, ref byte[] outBuffer)
    {
        byte[] result = decompressBuffer(source);
        if (result == null) return false;
        outBuffer = result;
        return true;
    }

    public static int decompressBufferFixed(byte[] source, ref byte[] outBuffer, bool safe = true)
    {
        byte[] result = decompressBuffer(source);
        if (result == null) return 0;
        if (result.Length > outBuffer.Length)
        {
            if (safe) return 0;
            Buffer.BlockCopy(result, 0, outBuffer, 0, outBuffer.Length);
            return outBuffer.Length;
        }
        Buffer.BlockCopy(result, 0, outBuffer, 0, result.Length);
        return result.Length;
    }

    public static byte[] decompressBuffer(byte[] source)
    {
        MemoryStream ms = new MemoryStream(source);
        InflaterInputStream iis = new InflaterInputStream(ms);
        MemoryStream output = new MemoryStream();
        try
        {
            byte[] buf = new byte[4096];
            int read;
            while ((read = iis.Read(buf, 0, buf.Length)) > 0)
                output.Write(buf, 0, read);
            return output.ToArray();
        }
        catch { return null; }
        finally
        {
            iis.Close();
            ms.Close();
            output.Close();
        }
    }

    // -------------------------------------------------------------------------
    // Entry <-> buffer
    // -------------------------------------------------------------------------

    public static bool entry2Buffer(string zipArchive, string entry,
                                    ref byte[] buffer, byte[] FileBuffer = null)
    {
        byte[] result = entry2Buffer(zipArchive, entry, FileBuffer);
        if (result == null) return false;
        buffer = result;
        return true;
    }

    public static byte[] entry2Buffer(string zipArchive, string entry, byte[] FileBuffer = null)
    {
        ZipFile zf = null;
        try
        {
            zf = FileBuffer != null
                ? new ZipFile(new MemoryStream(FileBuffer))
                : new ZipFile(zipArchive);

            ZipEntry ze = zf.GetEntry(entry);
            if (ze == null) return null;

            Stream s = zf.GetInputStream(ze);
            MemoryStream ms = new MemoryStream((int)Math.Max(0, ze.Size));
            try
            {
                byte[] buf = new byte[4096];
                int read;
                while ((read = s.Read(buf, 0, buf.Length)) > 0)
                    ms.Write(buf, 0, read);
                return ms.ToArray();
            }
            finally
            {
                s.Close();
                ms.Close();
            }
        }
        catch { return null; }
        finally
        {
            if (zf != null) zf.Close();
        }
    }

    public static bool buffer2File(int levelOfCompression, string zipArchive,
                                   string arc_filename, byte[] buffer, bool append = false)
    {
        if (!append && File.Exists(zipArchive))
            File.Delete(zipArchive);

        int sharpLevel = Clamp(levelOfCompression, 0, 9);
        ZipOutputStream zos = new ZipOutputStream(
            new FileStream(zipArchive,
                           append ? FileMode.Append : FileMode.Create,
                           FileAccess.Write));
        try
        {
            zos.SetLevel(sharpLevel);
            ZipEntry ze = new ZipEntry(arc_filename);
            ze.DateTime = DateTime.Now;
            zos.PutNextEntry(ze);
            zos.Write(buffer, 0, buffer.Length);
            zos.CloseEntry();
            return true;
        }
        catch { return false; }
        finally { zos.Close(); }
    }

    // -------------------------------------------------------------------------
    // Compress files / directories
    // -------------------------------------------------------------------------

    public static int compress_File(int levelOfCompression, string zipArchive,
                                    string inFilePath, bool append = false,
                                    string fileName = "", string comment = "")
    {
        if (!File.Exists(inFilePath)) return -10;
        if (!append && File.Exists(zipArchive)) File.Delete(zipArchive);

        if (fileName == string.Empty)
            fileName = Path.GetFileName(inFilePath);

        int sharpLevel = Clamp(levelOfCompression, 0, 9);

        ZipOutputStream zos = new ZipOutputStream(
            new FileStream(zipArchive,
                           append ? FileMode.Append : FileMode.Create,
                           FileAccess.Write));
        try
        {
            zos.SetLevel(sharpLevel);
            if (!string.IsNullOrEmpty(comment)) zos.SetComment(comment);

            ZipEntry ze = new ZipEntry(fileName);
            ze.DateTime = File.GetLastWriteTime(inFilePath);
            zos.PutNextEntry(ze);

            FileStream fs = File.OpenRead(inFilePath);
            try
            {
                byte[] buf = new byte[4096];
                int read;
                while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                    zos.Write(buf, 0, read);
            }
            finally { fs.Close(); }

            zos.CloseEntry();
            return 0;
        }
        catch { return -1; }
        finally { zos.Close(); }
    }

    public static void compressDir(string sourceDir, int levelOfCompression,
                                   string zipArchive, bool includeRoot = false)
    {
        string dir = sourceDir.Replace('\\', '/').TrimEnd('/');
        if (!Directory.Exists(dir)) return;
        if (File.Exists(zipArchive)) File.Delete(zipArchive);

        string rootName = Path.GetFileName(dir);
        int sharpLevel = Clamp(levelOfCompression, 0, 9);
        cProgress = 0;

        ZipOutputStream zos = new ZipOutputStream(File.Create(zipArchive));
        try
        {
            zos.SetLevel(sharpLevel);

            string[] files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string normalised = file.Replace('\\', '/');
                string arcName = normalised.Replace(dir + "/", rootName + "/");
                if (!includeRoot)
                    arcName = arcName.Replace(rootName + "/", string.Empty);

                ZipEntry ze = new ZipEntry(arcName);
                ze.DateTime = File.GetLastWriteTime(file);
                zos.PutNextEntry(ze);

                FileStream fs = File.OpenRead(file);
                try
                {
                    byte[] buf = new byte[4096];
                    int read;
                    while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                        zos.Write(buf, 0, read);
                }
                finally { fs.Close(); }

                zos.CloseEntry();
                cProgress++;
            }
        }
        catch { }
        finally { zos.Close(); }
    }

    // -------------------------------------------------------------------------
    // Extract
    // -------------------------------------------------------------------------

    public static int extract_entry(string zipArchive, string arc_filename,
                                    string outpath, byte[] FileBuffer = null,
                                    int[] proc = null)
    {
        string dir = Path.GetDirectoryName(outpath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) return -1;

        ZipFile zf = null;
        try
        {
            zf = FileBuffer != null
                ? new ZipFile(new MemoryStream(FileBuffer))
                : new ZipFile(zipArchive);

            ZipEntry ze = zf.GetEntry(arc_filename);
            if (ze == null) return -2;

            Stream s = zf.GetInputStream(ze);
            FileStream fs = File.Create(outpath);
            try
            {
                byte[] buf = new byte[4096];
                int read;
                while ((read = s.Read(buf, 0, buf.Length)) > 0)
                    fs.Write(buf, 0, read);
            }
            finally
            {
                s.Close();
                fs.Close();
            }

            if (proc != null && proc.Length > 0) proc[0] = 100;
            return 0;
        }
        catch { return -1; }
        finally
        {
            if (zf != null) zf.Close();
        }
    }

    public static int decompress_File(string zipArchive, string outPath,
                                      int[] progress, byte[] FileBuffer = null,
                                      int[] proc = null)
    {
        if (!outPath.EndsWith("/")) outPath += "/";
        Directory.CreateDirectory(outPath);

        ZipFile zf = null;
        try
        {
            zf = FileBuffer != null
                ? new ZipFile(new MemoryStream(FileBuffer))
                : new ZipFile(zipArchive);

            int total = (int)zf.Count;
            int done = 0;

            foreach (ZipEntry entry in zf)
            {
                string dest = Path.Combine(outPath,
                    entry.Name.Replace('/', Path.DirectorySeparatorChar));

                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(dest);
                }
                else
                {
                    string destDir = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    Stream s = zf.GetInputStream(entry);
                    FileStream fs = File.Create(dest);
                    try
                    {
                        byte[] buf = new byte[4096];
                        int read;
                        while ((read = s.Read(buf, 0, buf.Length)) > 0)
                            fs.Write(buf, 0, read);
                    }
                    finally
                    {
                        s.Close();
                        fs.Close();
                    }
                }

                done++;
                int pct = total > 0 ? (int)((done / (float)total) * 100f) : 100;
                if (progress != null && progress.Length > 0) progress[0] = pct;
                if (proc != null && proc.Length > 0) proc[0] = pct;
            }

            return 0;
        }
        catch { return -1; }
        finally
        {
            if (zf != null) zf.Close();
        }
    }

    // -------------------------------------------------------------------------
    // GZip
    // -------------------------------------------------------------------------

    public static int gzip(byte[] source, byte[] outBuffer, int level,
                           bool addHeader = true, bool addFooter = true)
    {
        level = Clamp(level, 0, 9);
        MemoryStream ms = new MemoryStream(outBuffer, true);
        GZipOutputStream gz = new GZipOutputStream(ms);
        try
        {
            gz.SetLevel(level);
            gz.Write(source, 0, source.Length);
            gz.Finish();
            return (int)ms.Position;
        }
        catch { return 0; }
        finally
        {
            gz.Close();
            ms.Close();
        }
    }

    public static int gzipUncompressedSize(byte[] source)
    {
        int n = source.Length;
        return (source[n - 4] & 0xFF)
             | ((source[n - 3] & 0xFF) << 8)
             | ((source[n - 2] & 0xFF) << 16)
             | ((source[n - 1] & 0xFF) << 24);
    }

    public static int unGzip(byte[] source, byte[] outBuffer,
                             bool hasHeader = true, bool hasFooter = true)
    {
        MemoryStream ms = new MemoryStream(source);
        GZipInputStream gz = new GZipInputStream(ms);
        try
        {
            return gz.Read(outBuffer, 0, outBuffer.Length);
        }
        catch { return -1; }
        finally
        {
            gz.Close();
            ms.Close();
        }
    }

    public static int unGzip2(byte[] source, byte[] outBuffer)
    {
        return unGzip(source, outBuffer);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    public static int getAllFiles(string Dir)
    {
        return Directory.GetFiles(Dir, "*", SearchOption.AllDirectories).Length;
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}