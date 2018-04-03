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
 * Class to unzip a folder.  Executes asynchronously as to not block the calling thread.
 */
public class UnZipFile extends AsyncTask<String, Void, Boolean> {
	/**
	 * The tag used for logging information
	 */
	private static final String TAG = "UNZIPPING";

	/**
	 * An instance of the interface defined that will be called after an unzip attempt.
	 */
	private ZipComplete onComplete;

	/**
	 * Constructor to set the ZipComplete action.
	 *
	 * @param complete Instance of the ZipComplete that will be executed when done trying to unzip.
	 */
	public UnZipFile(ZipComplete complete) {
		onComplete = complete;
	}

	/**
	 * Perform the unzipping asynchronously.
	 *
	 * @param params params[0] = source path, params[1] = destination path
	 * @return True if successful, false otherwise
	 */
	@Override
	protected Boolean doInBackground(String... params) {
		String filePath = params[0];
		String destinationPath = params[1];

		File archive = new File(filePath);
		try {
			ZipFile zipfile = new ZipFile(archive);
			for (Enumeration e = zipfile.entries(); e.hasMoreElements(); ) {
				ZipEntry entry = (ZipEntry) e.nextElement();
				unzipEntry(zipfile, entry, destinationPath);
			}
		} catch (Exception e) {
			Log.e(TAG, "Error while unzipping");
			return false;
		}

		archive.delete();
		return true;
	}

	/**
	 * After executing, call execute the ZipComplete interface.
	 *
	 * @param result The result of unzipping, True = success, False = otherwise
	 */
	@Override
	protected void onPostExecute(Boolean result) {
		onComplete.onZipComplete(result);
	}

	/**
	 * Unzip an entry from a zip archive.
	 *
	 * @param zipfile The zip file to extract from
	 * @param entry The entry to extract
	 * @param outputDir The directory to place the resulting contents
	 * @throws IOException If unzipping goes wrong
	 */
	private void unzipEntry(ZipFile zipfile, ZipEntry entry, String outputDir) throws IOException {
		if (entry.isDirectory()) {
			createDir(new File(outputDir, entry.getName()));
			return;
		}

		File outputFile = new File(outputDir, entry.getName());
		if (!outputFile.getParentFile().exists()) {
			createDir(outputFile.getParentFile());
		}

		BufferedInputStream inputStream = new BufferedInputStream(zipfile.getInputStream(entry));
		BufferedOutputStream outputStream = new BufferedOutputStream(new FileOutputStream(outputFile));

		try {
			IOUtils.copy(inputStream, outputStream);
		} finally {
			outputStream.close();
			inputStream.close();
		}
	}

	/**
	 * Create a directory, used when zipped folders have subdirectories, and when creating the destination folder.
	 * @param dir Directory to create.
	 */
	private void createDir(File dir) {
		if (dir.exists()) {
			return;
		}

		if (!dir.mkdirs()) {
			throw new RuntimeException("Can not create dir " + dir);
		}
	}

	/**
	 * An interface where the function onZipComplete will be called after an unzip attempt.
	 */
	public interface ZipComplete {
		void onZipComplete(Boolean result);
	}
}

