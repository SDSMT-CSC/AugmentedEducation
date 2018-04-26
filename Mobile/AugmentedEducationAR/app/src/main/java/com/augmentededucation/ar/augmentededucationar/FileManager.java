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
 * A class for file management.  It keeps track of models downloaded from the website through a room database.
 *
 * TODO: scan models folder for existing models
 */
public class FileManager
{
	/**
	 * DownloadManager to facilitate downloading models from the website
	 */
	private DownloadManager downloadManager;

	/**
	 * Interface with the website's API
	 */
	private WebAccessor webAccessor;

	/**
	 * The database to store the model information
	 */
	private AppDatabase db;

	/**
	 * Context needed by various calls
	 */
	private Context context;

	/**
	 * String prepended to model's that are located in the assets folder
	 */
	public final static String assetsFileNameSubstring = "file:///android_asset/";

	/**
	 * Name of the database to access/change
	 */
	private final static String dbName = "augmentededucationdb";

	/**
	 * Path for the downloads directory to download the zip archive into
	 */
	private final static String downloadsPath = Environment.getExternalStorageDirectory().getPath() + "/" + Environment.DIRECTORY_DOWNLOADS;

	/**
	 * Path for where to extract the zip archive to
	 */
	private final static String folderName = Environment.getExternalStorageDirectory().getPath() + "/" + "Models/";

	/**
	 * Set with the location where a model is saved to
	 */
	private String fileLocation;

	/**
	 * Download ID for the currently downloading model
	 * If null, then not currently downloading a file
	 */
	private Long downloadingFile;

	/**
	 * The constructor to initialize the  private variables
	 *
	 * TODO: Scan the models directory and check if any files were added/deleted and update the database accordingly
	 *
	 * @param context The app context needed by various methods
	 */
	public FileManager(Context context) {
		webAccessor = new WebAccessor(context);
		db = AppDatabase.getInstance(context, dbName);
		downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
		File saveDir = new File(folderName);
		if (!saveDir.exists())
			saveDir.mkdir();
		this.context = context;
	}

	/**
	 * Download a model.  The function will do nothing if a model is currently downloading.  The provided broadcast receiver
	 * will be executed when a successful download is completed.  If an error occurs, the onDownloadError object provided will be
	 * executed.
	 *
	 * TODO: Remove a failed download from the downloads folder, as successive downloads will create misnamed archives and never succeed
	 *
	 * @param model The model to download
	 * @param authToken The authorization token to communicate with the website
	 * @param receiver The receiver to handle a download
	 * @param downloadError An error receiver that will be called if an error occurs
	 */
	public void downloadModel(final Model model, String authToken, final BroadcastReceiver receiver, WebAccessor.onDownloadError downloadError) {
		if (downloadingFile != null) {
			return;
		}

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
					UnZipFile unZipFile = new UnZipFile(result -> {
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
					});
					unZipFile.execute(source, dest);
				}
			}
		}, new IntentFilter(DownloadManager.ACTION_DOWNLOAD_COMPLETE));

		/*
		 * The model will be downloaded as <model name>-obj.zip
		 */
		webAccessor.downloadFile(context, authToken, model.url, model.name + "-obj.zip",
			downloadId -> {
				if (downloadingFile == null)
					downloadingFile = downloadId;
			}, downloadError);
	}

	/**
	 * Add a model to the database.  If the model currently exists in the database, it will do nothing.
	 * @param model  The model to add
	 */
	public void addModelToDatabase(Model model) {
		List<Model> models = db.modelDao().loadModelWhereURL(model.url);
		if (models.size() == 0) {
			db.modelDao().insertModel(model);
		}
	}

	/**
	 * Sets a model in the database.  It has the same funcitonality as the addModelToDatabase function, but will
	 * overwrite a model if it exists.
	 *
	 * @param model The model to set
	 */
	public void setModelDB(Model model) {
		List<Model> models = db.modelDao().loadModelWhereURL(model.url);
		if (models.size() == 0) {
			db.modelDao().insertModel(model);
		}
		else {
			db.modelDao().updateModel(model.url, model.name, model.location);
		}
	}

	/**
	 * Get an updated model.  Given a model, get whats in the database, even if the non-primary key fields are different.
	 *
	 * @param model The model to get
	 * @return The model as stored in the database
	 */
	public Model getUpdatedModel(Model model) {
		List<Model> models = db.modelDao().loadModelWhereURL(model.url);
		if (models.size() > 0) {
			return models.get(0);
		}
		else
			return null;
	}

	/**
	 * Get a list of models tracked in the database
	 * @return A list of models from the database
	 */
	public ArrayList<Model> getListOfModels() {
		return (ArrayList<Model>) db.modelDao().loadAllModels();
	}

	/**
	 * A subset of getListOfModels where only the models where location is not empty will be returned
	 * @return A list of models where the location is not empty (the model is on the phone)
	 */
	public ArrayList<Model> getLocalModels() {
		ArrayList<Model> arrayList = (ArrayList<Model>) db.modelDao().loadAllModels();
		arrayList.removeIf(s -> s.location == null || s.location == "");
		return arrayList;
	}
}
