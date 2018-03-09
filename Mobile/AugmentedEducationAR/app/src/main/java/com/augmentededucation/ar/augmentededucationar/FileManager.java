package com.augmentededucation.ar.augmentededucationar;

import android.app.DownloadManager;
import android.arch.persistence.room.Room;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.database.Cursor;
import android.os.Environment;

import com.augmentededucation.ar.augmentededucationar.WebAccess.WebAccessor;
import com.augmentededucation.ar.augmentededucationar.db.AppDatabase;
import com.augmentededucation.ar.augmentededucationar.db.entity.Model;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by kpetr on 2/28/2018.
 */

public class FileManager
{
	private DownloadManager downloadManager;
	private WebAccessor webAccessor;
	private AppDatabase db;
	private Context context;

	public final static String assetsFileNameSubstring = "file:///android_asset/";
	private final static String dbName = "augmentededucationdb";
	private final static String folderName = Environment.DIRECTORY_DOWNLOADS;

	public String fileLocation;

	private Long downloadingFile;

	public FileManager(Context context) {
		webAccessor = new WebAccessor(context);
		db = AppDatabase.getInstance(context, dbName);
		downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
		this.context = context;
	}

	public void downloadModel(final Model model, String authToken, final BroadcastReceiver receiver) {
		if (downloadingFile != null) {
			return;
		}

		if (true) return;

		context.registerReceiver(new BroadcastReceiver()
		{
			@Override
			public void onReceive(Context context, Intent intent)
			{
				Cursor cursor = downloadManager.query(new DownloadManager.Query().setFilterById(downloadingFile));
				if (cursor == null) {
					return;
				}
				cursor.moveToFirst();

				int status = cursor.getInt(cursor.getColumnIndex(DownloadManager.COLUMN_STATUS));
				if (status == DownloadManager.STATUS_SUCCESSFUL) {
					fileLocation = folderName + "/" + model.name;
					model.location = fileLocation;
					setModelDB(model);
					downloadingFile = null;
					receiver.onReceive(context, intent);
				}
			}
	}, new IntentFilter(DownloadManager.ACTION_DOWNLOAD_COMPLETE));

		downloadingFile = webAccessor.downloadFile(context, authToken, model.url, folderName, model.name);
	}

	public void addModelToDatabase(Model model) {
		List<Model> models = db.modelDao().loadModelWhereURL(model.url);
		if (models.size() == 0) {
			db.modelDao().insertModel(model);
		}
	}

	public void setModelDB(Model model) {
		List<Model> models = db.modelDao().loadModelWhereURL(model.url);
		if (models.size() == 0) {
			db.modelDao().insertModel(model);
		}
		else {
			db.modelDao().updateModel(model.url, model.name, model.location);
		}
	}

	public Model getUpdatedModel(Model model) {
		List<Model> models = db.modelDao().loadModelWhereURL(model.url);
		if (models.size() > 0) {
			return models.get(0);
		}
		else
			return null;
	}

	public ArrayList<Model> getListOfModels() {
		return (ArrayList<Model>) db.modelDao().loadAllModels();
	}
}
