package com.augmentededucation.ar.augmentededucationar;

import android.app.DownloadManager;
import android.content.Context;

import com.augmentededucation.ar.augmentededucationar.WebAccess.WebAccessor;

/**
 * Created by kpetr on 2/28/2018.
 */

public class FileManager
{
	private DownloadManager downloadManager;
	private WebAccessor webAccessor;

	public final static String assetsFileNameSubstring = "file:///android_asset/";

	public FileManager(Context context) {
		webAccessor = new WebAccessor(context);
	}

	public void downloadFile(String location) {

	}
}
