package com.augmentededucation.ar.augmentededucationar;

import android.app.DownloadManager;
import android.arch.persistence.room.Room;
import android.content.Context;

import com.augmentededucation.ar.augmentededucationar.WebAccess.WebAccessor;
import com.augmentededucation.ar.augmentededucationar.db.AppDatabase;

/**
 * Created by kpetr on 2/28/2018.
 */

public class FileManager
{
	private DownloadManager downloadManager;
	private WebAccessor webAccessor;
	private AppDatabase db;

	public final static String assetsFileNameSubstring = "file:///android_asset/";

	private final static String dbName = "augmentededucationdb";

	public FileManager(Context context) {
		webAccessor = new WebAccessor(context);
		db = Room.databaseBuilder(context, AppDatabase.class, dbName).build();
	}

	public void downloadFile(String location) {

	}
}
