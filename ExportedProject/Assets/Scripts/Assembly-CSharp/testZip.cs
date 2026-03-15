using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;

public class testZip : MonoBehaviour
{
	private int zres;

	private float t1;

	private float t;

	private string myFile;

	private WWW www;

	private string log;

	private string ppath;

	private bool compressionStarted;

	private bool pass;

	private bool downloadDone;

	private byte[] reusableBuffer;

	private byte[] reusableBuffer2;

	private byte[] reusableBuffer3;

	private byte[] fixedInBuffer = new byte[262144];

	private byte[] fixedOutBuffer = new byte[786432];

	private int[] progress = new int[1];

	private int[] progress2 = new int[1];

	private void plog(string t)
	{
		log = log + t + "\n";
	}

	private void Start()
	{
		ppath = Application.persistentDataPath;
		Debug.Log(ppath);
		Screen.sleepTimeout = -1;
		reusableBuffer = new byte[4096];
		reusableBuffer2 = new byte[0];
		reusableBuffer3 = new byte[0];
		Screen.sleepTimeout = -1;
		StartCoroutine(DownloadZipFile());
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	private void OnGUI()
	{
		if (downloadDone)
		{
			GUI.Label(new Rect(10f, 0f, 250f, 30f), "package downloaded, ready to extract");
			GUI.Label(new Rect(10f, 50f, 650f, 100f), ppath);
		}
		if (compressionStarted)
		{
			GUI.TextArea(new Rect(10f, 160f, Screen.width - 20, Screen.height - 170), log);
			GUI.Label(new Rect(Screen.width - 30, 0f, 80f, 40f), progress[0].ToString());
			GUI.Label(new Rect(Screen.width - 140, 0f, 80f, 40f), progress2[0].ToString());
		}
		if (downloadDone)
		{
			if (GUI.Button(new Rect(10f, 90f, 230f, 50f), "start zip test"))
			{
				log = string.Empty;
				compressionStarted = true;
				DoDecompression();
			}
			if (GUI.Button(new Rect(260f, 90f, 230f, 50f), "start File Buffer test"))
			{
				log = string.Empty;
				compressionStarted = true;
				DoDecompression_FileBuffer();
			}
		}
	}

	private void DoDecompression()
	{
		t = Time.realtimeSinceStartup;
		zres = lzip.decompress_File(ppath + "/testZip.zip", ppath + "/", progress, null, progress2);
		plog("decompress: " + zres);
		zres = lzip.extract_entry(ppath + "/testZip.zip", "dir1/dir2/test2.bmp", ppath + "/test22.bmp", null, progress2);
		plog("extract entry: " + zres);
		t1 = Time.realtimeSinceStartup - t;
		plog("time taken: " + t1);
		plog(string.Empty);
		t = Time.realtimeSinceStartup;
		zres = lzip.compress_File(9, ppath + "/test2Zip.zip", ppath + "/dir1/dir2/test2.bmp", false, "dir1/dir2/test2.bmp", string.Empty);
		plog("compress: " + zres);
		zres = lzip.compress_File(9, ppath + "/test2Zip.zip", ppath + "/dir1/dir2/dir3/Unity_1.jpg", true, "dir1/dir2/dir3/Unity_1.jpg", string.Empty);
		plog("append: " + zres);
		t1 = Time.realtimeSinceStartup - t;
		plog("time taken: " + t1);
		plog(string.Empty);
		plog("Buffer2File: " + lzip.buffer2File(9, ppath + "/test3Zip.zip", "buffer.bin", reusableBuffer));
		plog("Buffer2File append: " + lzip.buffer2File(9, ppath + "/test3Zip.zip", "dir4/buffer.bin", reusableBuffer, true));
		plog(string.Empty);
		plog("get entry size: " + lzip.getEntrySize(ppath + "/testZip.zip", "dir1/dir2/test2.bmp"));
		plog(string.Empty);
		plog("entry2Buffer1: " + lzip.entry2Buffer(ppath + "/testZip.zip", "dir1/dir2/test2.bmp", ref reusableBuffer2));
		plog(string.Empty);
		byte[] array = new byte[reusableBuffer2.Length + 18];
		int num = lzip.gzip(reusableBuffer2, array, 9);
		plog("gzip compressed size: " + num);
		byte[] array2 = new byte[num];
		Buffer.BlockCopy(array, 0, array2, 0, num);
		File.WriteAllBytes(ppath + "/test2.bmp.gz", array2);
		byte[] array3 = new byte[lzip.gzipUncompressedSize(array2)];
		int num2 = lzip.unGzip(array2, array3);
		if (num2 > 0)
		{
			File.WriteAllBytes(ppath + "/test2GZIP.bmp", array3);
			plog("gzip decompression: success " + num2);
		}
		else
		{
			plog("gzip decompression error: " + num2);
		}
		array = null;
		array2 = null;
		array3 = null;
		plog(string.Empty);
		byte[] array4 = lzip.entry2Buffer(ppath + "/testZip.zip", "dir1/dir2/test2.bmp");
		zres = 0;
		if (array4 != null)
		{
			zres = 1;
		}
		plog("entry2Buffer2: " + zres);
		plog(string.Empty);
		int num3 = lzip.compressBufferFixed(array4, ref fixedInBuffer, 10);
		plog(" # Compress Fixed size Buffer: " + num3);
		if (num3 > 0)
		{
			int num4 = lzip.decompressBufferFixed(fixedInBuffer, ref fixedOutBuffer);
			if (num4 > 0)
			{
				plog(" # Decompress Fixed size Buffer: " + num4);
			}
		}
		plog(string.Empty);
		pass = lzip.compressBuffer(reusableBuffer2, ref reusableBuffer3, 9);
		plog("compressBuffer1: " + pass);
		array4 = lzip.compressBuffer(reusableBuffer2, 9);
		zres = 0;
		if (array4 != null)
		{
			zres = 1;
		}
		plog("compressBuffer2: " + zres);
		plog(string.Empty);
		pass = lzip.decompressBuffer(reusableBuffer3, ref reusableBuffer2);
		plog("decompressBuffer1: " + pass);
		zres = 0;
		if (array4 != null)
		{
			zres = 1;
		}
		array4 = lzip.decompressBuffer(reusableBuffer3);
		plog("decompressBuffer2: " + zres);
		plog(string.Empty);
		plog("total bytes: " + lzip.getFileInfo(ppath + "/testZip.zip", ppath));
		if (lzip.ninfo != null)
		{
			for (int i = 0; i < lzip.ninfo.Count; i++)
			{
				string text = log;
				log = text + lzip.ninfo[i] + " - " + lzip.uinfo[i] + " / " + lzip.cinfo[i] + "\n";
			}
		}
		plog(string.Empty);
		t = Time.realtimeSinceStartup;
		lzip.compressDir(ppath + "/dir1", 10, ppath + "/recursive.zip");
		t1 = Time.realtimeSinceStartup - t;
		plog("recursive compress time: " + t1 + " - no. of files: " + lzip.cProgress);
		t = Time.realtimeSinceStartup;
		lzip.decompress_File(ppath + "/recursive.zip", ppath + "/recursive/", progress, null, progress2);
		t1 = Time.realtimeSinceStartup - t;
		plog("decompress recursive time: " + t1);
		Thread thread = new Thread(decompressFunc);
		thread.Start();
	}

	private void decompressFunc()
	{
		int num = lzip.decompress_File(ppath + "/recursive.zip", ppath + "/recursive/", progress, null, progress2);
		if (num == 1)
		{
			plog("multithreaded ok");
		}
		else
		{
			plog("multithreaded error: " + num);
		}
	}

	private void DoDecompression_FileBuffer()
	{
		byte[] fileBuffer = File.ReadAllBytes(ppath + "/testZip.zip");
		t = Time.realtimeSinceStartup;
		zres = lzip.decompress_File(null, ppath + "/", progress, fileBuffer, progress2);
		plog("decompress: " + zres);
		zres = lzip.extract_entry(null, "dir1/dir2/test2.bmp", ppath + "/test22.bmp", fileBuffer, progress2);
		plog("extract entry: " + zres);
		t1 = Time.realtimeSinceStartup - t;
		plog("time taken: " + t1);
		plog(string.Empty);
		plog("get entry size: " + lzip.getEntrySize(null, "dir1/dir2/test2.bmp", fileBuffer));
		plog(string.Empty);
		plog("entry2Buffer1: " + lzip.entry2Buffer(null, "dir1/dir2/test2.bmp", ref reusableBuffer2, fileBuffer));
		byte[] array = lzip.entry2Buffer(null, "dir1/dir2/test2.bmp", fileBuffer);
		zres = 0;
		if (array != null)
		{
			zres = 1;
		}
		plog("entry2Buffer2: " + zres);
		plog(string.Empty);
		plog("total bytes: " + lzip.getFileInfo(null, ppath, fileBuffer));
		if (lzip.ninfo != null)
		{
			for (int i = 0; i < lzip.ninfo.Count; i++)
			{
				string text = log;
				log = text + lzip.ninfo[i] + " - " + lzip.uinfo[i] + " / " + lzip.cinfo[i] + "\n";
			}
		}
		plog(string.Empty);
	}

	private IEnumerator DownloadZipFile()
	{
		Debug.Log("starting download");
		myFile = "testZip.zip";
		if (File.Exists(ppath + "/" + myFile))
		{
			downloadDone = true;
			yield break;
		}
		www = new WWW("https://dl.dropboxusercontent.com/u/13373268/tests/" + myFile);
		yield return www;
		if (www.error != null)
		{
			Debug.Log(www.error);
		}
		downloadDone = true;
		File.WriteAllBytes(ppath + "/" + myFile, www.bytes);
		www.Dispose();
		www = null;
	}
}
