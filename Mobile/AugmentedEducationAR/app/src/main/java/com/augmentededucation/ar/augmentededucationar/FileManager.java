package com.augmentededucation.ar.augmentededucationar;

import android.app.DownloadManager;
import android.arch.persistence.room.Room;
import android.content.Context;

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

	public final static String assetsFileNameSubstring = "file:///android_asset/";

	private final static String dbName = "augmentededucationdb";

	public FileManager(Context context) {
		webAccessor = new WebAccessor(context);
		db = AppDatabase.getInstance(context, dbName);
	}

	public void downloadFile(String location) {

	}

	public void addModelToDatabase(Model model) {
		List<Model> models = db.modelDao().loadModelWhereURL(model.url);
		if (models.size() == 0) {
			db.modelDao().insertModel(model);
		}
		else {
			db.modelDao().updateModel(model.url, model.name, model.location);
		}
	}

	public ArrayList<Model> getListOfModels() {
		return (ArrayList<Model>) db.modelDao().loadAllModels();
	}
}
