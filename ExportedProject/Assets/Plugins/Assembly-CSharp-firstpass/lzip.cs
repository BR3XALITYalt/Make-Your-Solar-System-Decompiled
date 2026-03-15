using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class lzip
{
	private const string libname = "zipw";

	public static List<string> ninfo = new List<string>();

	public static List<long> uinfo = new List<long>();

	public static List<long> cinfo = new List<long>();

	public static int zipFiles;

	public static int zipFolders;

	public static int cProgress = 0;

	[DllImport("zipw")]
	internal static extern int zsetPermissions(string filePath, string _user, string _group, string _other);

	[DllImport("zipw")]
	internal static extern int zipGetTotalFiles(string zipArchive, IntPtr FileBuffer, int fileBufferLength);

	[DllImport("zipw")]
	internal static extern int zipGetInfo(string zipArchive, string path, IntPtr FileBuffer, int fileBufferLength);

	[DllImport("zipw")]
	internal static extern void releaseBuffer(IntPtr buffer);

	[DllImport("zipw")]
	internal static extern int zipGetEntrySize(string zipArchive, string entry, IntPtr FileBuffer, int fileBufferLength);

	[DllImport("zipw")]
	internal static extern int zipCD(int levelOfCompression, string zipArchive, string inFilePath, string fileName, string comment);

	[DllImport("zipw")]
	internal static extern bool zipBuf2File(int levelOfCompression, string zipArchive, string arc_filename, IntPtr buffer, int bufferSize);

	[DllImport("zipw")]
	internal static extern bool zipEntry2Buffer(string zipArchive, string entry, IntPtr buffer, int bufferSize, IntPtr FileBuffer, int fileBufferLength);

	[DllImport("zipw")]
	internal static extern IntPtr zipCompressBuffer(IntPtr source, int sourceLen, int levelOfCompression, ref int v);

	[DllImport("zipw")]
	internal static extern IntPtr zipDecompressBuffer(IntPtr source, int sourceLen, ref int v);

	[DllImport("zipw")]
	internal static extern int zipEX(string zipArchive, string outPath, IntPtr progress, IntPtr FileBuffer, int fileBufferLength, IntPtr proc);

	[DllImport("zipw")]
	internal static extern int zipEntry(string zipArchive, string arc_filename, string outpath, IntPtr FileBuffer, int fileBufferLength, IntPtr proc);

	[DllImport("zipw")]
	internal static extern int zipGzip(IntPtr source, int sourceLen, IntPtr outBuffer, int levelOfCompression, bool addHeader, bool addFooter);

	[DllImport("zipw")]
	internal static extern int zipUnGzip(IntPtr source, int sourceLen, IntPtr outBuffer, int outLen, bool hasHeader, bool hasFooter);

	[DllImport("zipw")]
	internal static extern int zipUnGzip2(IntPtr source, int sourceLen, IntPtr outBuffer, int outLen);

	public static int setFilePermissions(string filePath, string _user, string _group, string _other)
	{
		return zsetPermissions(filePath, _user, _group, _other);
	}

	public static int getTotalFiles(string zipArchive, byte[] FileBuffer = null)
	{
		if (FileBuffer != null)
		{
			GCHandle gCHandle = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			int result = zipGetTotalFiles(null, gCHandle.AddrOfPinnedObject(), FileBuffer.Length);
			gCHandle.Free();
			return result;
		}
		return zipGetTotalFiles(zipArchive, IntPtr.Zero, 0);
	}

	public static long getFileInfo(string zipArchive, string path, byte[] FileBuffer = null)
	{
		ninfo.Clear();
		uinfo.Clear();
		cinfo.Clear();
		zipFiles = 0;
		zipFolders = 0;
		int num;
		if (FileBuffer != null)
		{
			GCHandle gCHandle = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			num = zipGetInfo(null, path, gCHandle.AddrOfPinnedObject(), FileBuffer.Length);
			gCHandle.Free();
		}
		else
		{
			num = zipGetInfo(zipArchive, path, IntPtr.Zero, 0);
		}
		switch (num)
		{
		case -1:
			return -1L;
		case -2:
			return -2L;
		case -3:
			return -3L;
		default:
		{
			string path2 = path + "/uziplog.txt";
			if (!File.Exists(path2))
			{
				return -4L;
			}
			StreamReader streamReader = new StreamReader(path2);
			long result = 0L;
			long num2 = 0L;
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split('|');
				if (array != null && array.Length > 0)
				{
					ninfo.Add(array[0]);
					long.TryParse(array[1], out result);
					num2 += result;
					uinfo.Add(result);
					if (result > 0)
					{
						zipFiles++;
					}
					else
					{
						zipFolders++;
					}
					long.TryParse(array[2], out result);
					cinfo.Add(result);
				}
			}
			streamReader.Close();
			streamReader.Dispose();
			File.Delete(path2);
			return num2;
		}
		}
	}

	public static int getEntrySize(string zipArchive, string entry, byte[] FileBuffer = null)
	{
		if (FileBuffer != null)
		{
			GCHandle gCHandle = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			int result = zipGetEntrySize(null, entry, gCHandle.AddrOfPinnedObject(), FileBuffer.Length);
			gCHandle.Free();
			return result;
		}
		return zipGetEntrySize(zipArchive, entry, IntPtr.Zero, 0);
	}

	public static bool compressBuffer(byte[] source, ref byte[] outBuffer, int levelOfCompression)
	{
		if (levelOfCompression < 0)
		{
			levelOfCompression = 0;
		}
		if (levelOfCompression > 10)
		{
			levelOfCompression = 10;
		}
		GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
		int v = 0;
		IntPtr intPtr = zipCompressBuffer(gCHandle.AddrOfPinnedObject(), source.Length, levelOfCompression, ref v);
		if (v == 0 || intPtr == IntPtr.Zero)
		{
			gCHandle.Free();
			releaseBuffer(intPtr);
			return false;
		}
		Array.Resize(ref outBuffer, v);
		Marshal.Copy(intPtr, outBuffer, 0, v);
		gCHandle.Free();
		releaseBuffer(intPtr);
		return true;
	}

	public static int compressBufferFixed(byte[] source, ref byte[] outBuffer, int levelOfCompression, bool safe = true)
	{
		if (levelOfCompression < 0)
		{
			levelOfCompression = 0;
		}
		if (levelOfCompression > 10)
		{
			levelOfCompression = 10;
		}
		GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
		int v = 0;
		IntPtr intPtr = zipCompressBuffer(gCHandle.AddrOfPinnedObject(), source.Length, levelOfCompression, ref v);
		if (v == 0 || intPtr == IntPtr.Zero)
		{
			gCHandle.Free();
			releaseBuffer(intPtr);
			return 0;
		}
		if (v > outBuffer.Length)
		{
			if (safe)
			{
				gCHandle.Free();
				releaseBuffer(intPtr);
				return 0;
			}
			v = outBuffer.Length;
		}
		Marshal.Copy(intPtr, outBuffer, 0, v);
		gCHandle.Free();
		releaseBuffer(intPtr);
		return v;
	}

	public static byte[] compressBuffer(byte[] source, int levelOfCompression)
	{
		if (levelOfCompression < 0)
		{
			levelOfCompression = 0;
		}
		if (levelOfCompression > 10)
		{
			levelOfCompression = 10;
		}
		GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
		int v = 0;
		IntPtr intPtr = zipCompressBuffer(gCHandle.AddrOfPinnedObject(), source.Length, levelOfCompression, ref v);
		if (v == 0 || intPtr == IntPtr.Zero)
		{
			gCHandle.Free();
			releaseBuffer(intPtr);
			return null;
		}
		byte[] array = new byte[v];
		Marshal.Copy(intPtr, array, 0, v);
		gCHandle.Free();
		releaseBuffer(intPtr);
		return array;
	}

	public static bool decompressBuffer(byte[] source, ref byte[] outBuffer)
	{
		GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
		int v = 0;
		IntPtr intPtr = zipDecompressBuffer(gCHandle.AddrOfPinnedObject(), source.Length, ref v);
		if (v == 0 || intPtr == IntPtr.Zero)
		{
			gCHandle.Free();
			releaseBuffer(intPtr);
			return false;
		}
		Array.Resize(ref outBuffer, v);
		Marshal.Copy(intPtr, outBuffer, 0, v);
		gCHandle.Free();
		releaseBuffer(intPtr);
		return true;
	}

	public static int decompressBufferFixed(byte[] source, ref byte[] outBuffer, bool safe = true)
	{
		GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
		int v = 0;
		IntPtr intPtr = zipDecompressBuffer(gCHandle.AddrOfPinnedObject(), source.Length, ref v);
		if (v == 0 || intPtr == IntPtr.Zero)
		{
			gCHandle.Free();
			releaseBuffer(intPtr);
			return 0;
		}
		if (v > outBuffer.Length)
		{
			if (safe)
			{
				gCHandle.Free();
				releaseBuffer(intPtr);
				return 0;
			}
			v = outBuffer.Length;
		}
		Marshal.Copy(intPtr, outBuffer, 0, v);
		gCHandle.Free();
		releaseBuffer(intPtr);
		return v;
	}

	public static byte[] decompressBuffer(byte[] source)
	{
		GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
		int v = 0;
		IntPtr intPtr = zipDecompressBuffer(gCHandle.AddrOfPinnedObject(), source.Length, ref v);
		if (v == 0 || intPtr == IntPtr.Zero)
		{
			gCHandle.Free();
			releaseBuffer(intPtr);
			return null;
		}
		byte[] array = new byte[v];
		Marshal.Copy(intPtr, array, 0, v);
		gCHandle.Free();
		releaseBuffer(intPtr);
		return array;
	}

	public static bool entry2Buffer(string zipArchive, string entry, ref byte[] buffer, byte[] FileBuffer = null)
	{
		int num;
		if (FileBuffer != null)
		{
			GCHandle gCHandle = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			num = zipGetEntrySize(null, entry, gCHandle.AddrOfPinnedObject(), FileBuffer.Length);
			gCHandle.Free();
		}
		else
		{
			num = zipGetEntrySize(zipArchive, entry, IntPtr.Zero, 0);
		}
		if (num <= 0)
		{
			return false;
		}
		Array.Resize(ref buffer, num);
		GCHandle gCHandle2 = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		bool result;
		if (FileBuffer != null)
		{
			GCHandle gCHandle3 = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			result = zipEntry2Buffer(null, entry, gCHandle2.AddrOfPinnedObject(), num, gCHandle3.AddrOfPinnedObject(), FileBuffer.Length);
			gCHandle3.Free();
		}
		else
		{
			result = zipEntry2Buffer(zipArchive, entry, gCHandle2.AddrOfPinnedObject(), num, IntPtr.Zero, 0);
		}
		gCHandle2.Free();
		return result;
	}

	public static byte[] entry2Buffer(string zipArchive, string entry, byte[] FileBuffer = null)
	{
		int num;
		if (FileBuffer != null)
		{
			GCHandle gCHandle = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			num = zipGetEntrySize(null, entry, gCHandle.AddrOfPinnedObject(), FileBuffer.Length);
			gCHandle.Free();
		}
		else
		{
			num = zipGetEntrySize(zipArchive, entry, IntPtr.Zero, 0);
		}
		if (num <= 0)
		{
			return null;
		}
		byte[] array = new byte[num];
		GCHandle gCHandle2 = GCHandle.Alloc(array, GCHandleType.Pinned);
		bool flag;
		if (FileBuffer != null)
		{
			GCHandle gCHandle3 = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			flag = zipEntry2Buffer(null, entry, gCHandle2.AddrOfPinnedObject(), num, gCHandle3.AddrOfPinnedObject(), FileBuffer.Length);
			gCHandle3.Free();
		}
		else
		{
			flag = zipEntry2Buffer(zipArchive, entry, gCHandle2.AddrOfPinnedObject(), num, IntPtr.Zero, 0);
		}
		gCHandle2.Free();
		if (!flag)
		{
			return null;
		}
		return array;
	}

	public static bool buffer2File(int levelOfCompression, string zipArchive, string arc_filename, byte[] buffer, bool append = false)
	{
		if (!append && File.Exists(zipArchive))
		{
			File.Delete(zipArchive);
		}
		GCHandle gCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		if (levelOfCompression < 0)
		{
			levelOfCompression = 0;
		}
		if (levelOfCompression > 10)
		{
			levelOfCompression = 10;
		}
		bool result = zipBuf2File(levelOfCompression, zipArchive, arc_filename, gCHandle.AddrOfPinnedObject(), buffer.Length);
		gCHandle.Free();
		return result;
	}

	public static int extract_entry(string zipArchive, string arc_filename, string outpath, byte[] FileBuffer = null, int[] proc = null)
	{
		if (!Directory.Exists(Path.GetDirectoryName(outpath)))
		{
			return -1;
		}
		int num = -1;
		if (proc == null)
		{
			proc = new int[1];
		}
		GCHandle gCHandle = GCHandle.Alloc(proc, GCHandleType.Pinned);
		if (FileBuffer != null)
		{
			GCHandle gCHandle2 = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			num = ((proc == null) ? zipEntry(null, arc_filename, outpath, gCHandle2.AddrOfPinnedObject(), FileBuffer.Length, IntPtr.Zero) : zipEntry(null, arc_filename, outpath, gCHandle2.AddrOfPinnedObject(), FileBuffer.Length, gCHandle.AddrOfPinnedObject()));
			gCHandle2.Free();
			gCHandle.Free();
			return num;
		}
		num = ((proc == null) ? zipEntry(zipArchive, arc_filename, outpath, IntPtr.Zero, 0, IntPtr.Zero) : zipEntry(zipArchive, arc_filename, outpath, IntPtr.Zero, 0, gCHandle.AddrOfPinnedObject()));
		gCHandle.Free();
		return num;
	}

	public static int decompress_File(string zipArchive, string outPath, int[] progress, byte[] FileBuffer = null, int[] proc = null)
	{
		if (outPath.Substring(outPath.Length - 1, 1) != "/")
		{
			outPath += "/";
		}
		GCHandle gCHandle = GCHandle.Alloc(progress, GCHandleType.Pinned);
		if (proc == null)
		{
			proc = new int[1];
		}
		GCHandle gCHandle2 = GCHandle.Alloc(proc, GCHandleType.Pinned);
		int result;
		if (FileBuffer != null)
		{
			GCHandle gCHandle3 = GCHandle.Alloc(FileBuffer, GCHandleType.Pinned);
			result = ((proc == null) ? zipEX(null, outPath, gCHandle.AddrOfPinnedObject(), gCHandle3.AddrOfPinnedObject(), FileBuffer.Length, IntPtr.Zero) : zipEX(null, outPath, gCHandle.AddrOfPinnedObject(), gCHandle3.AddrOfPinnedObject(), FileBuffer.Length, gCHandle2.AddrOfPinnedObject()));
			gCHandle3.Free();
			gCHandle.Free();
			gCHandle2.Free();
			return result;
		}
		result = ((proc == null) ? zipEX(zipArchive, outPath, gCHandle.AddrOfPinnedObject(), IntPtr.Zero, 0, IntPtr.Zero) : zipEX(zipArchive, outPath, gCHandle.AddrOfPinnedObject(), IntPtr.Zero, 0, gCHandle2.AddrOfPinnedObject()));
		gCHandle.Free();
		gCHandle2.Free();
		return result;
	}

	public static int compress_File(int levelOfCompression, string zipArchive, string inFilePath, bool append = false, string fileName = "", string comment = "")
	{
		if (!append && File.Exists(zipArchive))
		{
			File.Delete(zipArchive);
		}
		if (!File.Exists(inFilePath))
		{
			return -10;
		}
		if (fileName == string.Empty)
		{
			fileName = Path.GetFileName(inFilePath);
		}
		if (levelOfCompression < 0)
		{
			levelOfCompression = 0;
		}
		if (levelOfCompression > 10)
		{
			levelOfCompression = 10;
		}
		return zipCD(levelOfCompression, zipArchive, inFilePath, fileName, comment);
	}

	public static void compressDir(string sourceDir, int levelOfCompression, string zipArchive, bool includeRoot = false)
	{
		string text = sourceDir.Replace("\\", "/");
		if (!Directory.Exists(text))
		{
			return;
		}
		if (File.Exists(zipArchive))
		{
			File.Delete(zipArchive);
		}
		string[] array = text.Split('/');
		string text2 = array[array.Length - 1];
		string text3 = text2;
		cProgress = 0;
		if (levelOfCompression < 0)
		{
			levelOfCompression = 0;
		}
		if (levelOfCompression > 10)
		{
			levelOfCompression = 10;
		}
		string[] files = Directory.GetFiles(text, "*", SearchOption.AllDirectories);
		foreach (string text4 in files)
		{
			string text5 = text4.Replace(text, text2).Replace("\\", "/");
			if (!includeRoot)
			{
				text5 = text5.Replace(text3 + "/", string.Empty);
			}
			compress_File(levelOfCompression, zipArchive, text4, true, text5, string.Empty);
			cProgress++;
		}
	}

	public static int getAllFiles(string Dir)
	{
		string[] files = Directory.GetFiles(Dir, "*", SearchOption.AllDirectories);
		int result = files.Length;
		files = null;
		return result;
	}

	public static int gzip(byte[] source, byte[] outBuffer, int level, bool addHeader = true, bool addFooter = true)
	{
		GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
		GCHandle gCHandle2 = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);
		int num = zipGzip(gCHandle.AddrOfPinnedObject(), source.Length, gCHandle2.AddrOfPinnedObject(), level, addHeader, addFooter);
		gCHandle.Free();
		gCHandle2.Free();
		int num2 = 0;
		if (addHeader)
		{
			num2 += 10;
		}
		if (addFooter)
		{
			num2 += 8;
		}
		return num + num2;
	}

	public static int gzipUncompressedSize(byte[] source)
	{
		int num = source.Length;
		return (source[num - 4] & 0xFF) | ((source[num - 3] & 0xFF) << 8) | ((source[num - 2] & 0xFF) << 16) | ((source[num - 1] & 0xFF) << 24);
	}

	public static int unGzip(byte[] source, byte[] outBuffer, bool hasHeader = true, bool hasFooter = true)
	{
		GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
		GCHandle gCHandle2 = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);
		int result = zipUnGzip(gCHandle.AddrOfPinnedObject(), source.Length, gCHandle2.AddrOfPinnedObject(), outBuffer.Length, hasHeader, hasFooter);
		gCHandle.Free();
		gCHandle2.Free();
		return result;
	}

	public static int unGzip2(byte[] source, byte[] outBuffer)
	{
		GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
		GCHandle gCHandle2 = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);
		int result = zipUnGzip2(gCHandle.AddrOfPinnedObject(), source.Length, gCHandle2.AddrOfPinnedObject(), outBuffer.Length);
		gCHandle.Free();
		gCHandle2.Free();
		return result;
	}
}
