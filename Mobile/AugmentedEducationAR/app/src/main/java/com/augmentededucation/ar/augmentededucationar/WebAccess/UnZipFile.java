package com.augmentededucation.ar.augmentededucationar.WebAccess;

import android.os.AsyncTask;
import android.util.Log;

import org.apache.commons.io.IOUtils;

import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.Enumeration;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;

/**
 * Class to unzip file
 */

public class UnZipFile extends AsyncTask<String, Void, Boolean>
{

	private static final String TAG = "UNZIPPING";
	private ZipComplete onComplete;

	public UnZipFile(ZipComplete complete)
	{
		onComplete = complete;
	}

	@Override
	protected Boolean doInBackground(String... params)
	{
		String filePath = params[0];
		String destinationPath = params[1];

		File archive = new File(filePath);
		try
		{
			ZipFile zipfile = new ZipFile(archive);
			for (Enumeration e = zipfile.entries(); e.hasMoreElements(); )
			{
				ZipEntry entry = (ZipEntry) e.nextElement();
				unzipEntry(zipfile, entry, destinationPath);
			}
		} catch (Exception e)
		{
			Log.e(TAG, "Error while unzipping");
			return false;
		}

		archive.delete();
		return true;
	}

	@Override
	protected void onPostExecute(Boolean result)
	{
		onComplete.onZipComplete(result);
	}

	private void unzipEntry(ZipFile zipfile, ZipEntry entry, String outputDir) throws IOException
	{

		if (entry.isDirectory())
		{
			createDir(new File(outputDir, entry.getName()));
			return;
		}

		File outputFile = new File(outputDir, entry.getName());
		if (!outputFile.getParentFile().exists())
		{
			createDir(outputFile.getParentFile());
		}

		Log.v(TAG, "Extracting: " + entry);
		BufferedInputStream inputStream = new BufferedInputStream(zipfile.getInputStream(entry));
		BufferedOutputStream outputStream = new BufferedOutputStream(new FileOutputStream(outputFile));

		try
		{
			IOUtils.copy(inputStream, outputStream);
		} finally
		{
			outputStream.close();
			inputStream.close();
		}
	}

	private void createDir(File dir)
	{
		if (dir.exists())
		{
			return;
		}
		Log.v(TAG, "Creating dir " + dir.getName());
		if (!dir.mkdirs())
		{
			throw new RuntimeException("Can not create dir " + dir);
		}
	}

	public interface ZipComplete
	{
		void onZipComplete(Boolean result);
	}
}

