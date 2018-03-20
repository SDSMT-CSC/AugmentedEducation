package com.augmentededucation.ar.augmentededucationar;

import android.app.DownloadManager;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.database.Cursor;
import android.os.Environment;
import android.widget.Toast;

import com.augmentededucation.ar.augmentededucationar.WebAccess.UnZipFile;
import com.augmentededucation.ar.augmentededucationar.WebAccess.WebAccessor;
import com.augmentededucation.ar.augmentededucationar.db.AppDatabase;
import com.augmentededucation.ar.augmentededucationar.db.entity.Model;

import java.io.File;
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
	private final static String downloadsPath = Environment.getExternalStorageDirectory().getPath() + "/" + Environment.DIRECTORY_DOWNLOADS;
	private final static String folderName = Environment.getExternalStorageDirectory().getPath() + "/" + "Models/";

	public String fileLocation;

	private Long downloadingFile;

	public FileManager(Context context) {
		webAccessor = new WebAccessor(context);
		db = AppDatabase.getInstance(context, dbName);
		downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
		File saveDir = new File(folderName);
		if (!saveDir.exists())
			saveDir.mkdir();
		this.context = context;
	}

	public void downloadModel(final Model model, String authToken, final BroadcastReceiver receiver) {
		if (downloadingFile != null) {
			return;
		}

		//if (true) return;
		final String source = downloadsPath + "/" + model.name + "-obj.zip";
		final String dest = folderName + "/" + model.name.replace('.', '-') + "/";

		context.registerReceiver(new BroadcastReceiver()
		{
			@Override
			public void onReceive(final Context context, final Intent intent)
			{
				Cursor cursor = downloadManager.query(new DownloadManager.Query().setFilterById(downloadingFile));
				if (cursor == null) {
					return;
				}
				cursor.moveToFirst();

				int status = cursor.getInt(cursor.getColumnIndex(DownloadManager.COLUMN_STATUS));
				if (status == DownloadManager.STATUS_SUCCESSFUL) {

					UnZipFile unZipFile = new UnZipFile(new UnZipFile.ZipComplete()
					{
						@Override
						public void onZipComplete(Boolean result)
						{
							if (result)
							{
								fileLocation = dest + model.name.split("\\.")[0] + ".obj";
								model.location = fileLocation;
								setModelDB(model);
								downloadingFile = null;
								receiver.onReceive(context, intent);
							}
							else {
								Toast.makeText(context, "Error preparing file for viewing", Toast.LENGTH_SHORT).show();
							}
						}
					});
					unZipFile.execute(source, dest);

				}
			}
	}, new IntentFilter(DownloadManager.ACTION_DOWNLOAD_COMPLETE));

		webAccessor.downloadFile(context, authToken, model.url, model.name + "-obj.zip",
			new WebAccessor.DownloadQueued()
			{
				@Override
				public void downloadQueued(long downloadId) {
					if (downloadingFile == null)
						downloadingFile = downloadId;
				}
			});
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

	public ArrayList<Model> getLocalModels() {
		ArrayList<Model> arrayList = (ArrayList<Model>) db.modelDao().loadAllModels();
		arrayList.removeIf(s -> s.location == null || s.location == "");
		return arrayList;
	}
}
