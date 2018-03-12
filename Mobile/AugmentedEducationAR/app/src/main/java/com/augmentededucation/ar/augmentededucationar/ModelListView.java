package com.augmentededucation.ar.augmentededucationar;

import android.content.Context;
import android.util.AttributeSet;
import android.util.Log;
import android.widget.ArrayAdapter;
import android.widget.ListView;

import com.augmentededucation.ar.augmentededucationar.db.entity.Model;

import java.io.File;
import java.util.ArrayList;
import java.util.HashMap;

/**
 * Created by kpetr on 2/27/2018.
 */

public class ModelListView extends ListView
{
	Context context;

	private ArrayList<String> editableModelList;
	private HashMap<Integer, Model> Models;
	private String[] modelList;
	private FileManager fileManager;

	private ArrayAdapter<String> adapter;

	public ModelListView(Context context) {
		super(context);
		init(context);
	}

	public ModelListView(Context context, AttributeSet attrs)
	{
		super(context, attrs);
		init(context);
	}

	public ModelListView(Context context, AttributeSet attrs, int defStyle)
	{
		super(context, attrs, defStyle);
		init(context);
	}

	private void init(Context context)
	{
		this.context = context;
		editableModelList = new ArrayList<>();
		Models = new HashMap<>();
		fileManager = new FileManager(context);
	}

	public void add(Model model)
	{
		Models.put(editableModelList.size(), model);
		editableModelList.add(model.name);
	}

	public void clear()
	{
		Models.clear();
		editableModelList.clear();
	}

	public void refreshList()
	{
		this.clear();

		ArrayList<Model> mList = fileManager.getListOfModels();
		for (Model m : mList) {
			if (m.location != null && !m.location.startsWith(FileManager.assetsFileNameSubstring)) {
				File file = new File(m.location);
				if (!file.exists()){
					m.location = null;
					fileManager.setModelDB(m);
				}
			}
			add(m);
		}

		modelList = new String[editableModelList.size()];
		editableModelList.toArray(modelList);
		adapter = new ArrayAdapter<>(context, android.R.layout.simple_list_item_1, modelList);
		Log.e("listview", "refreshing with " + editableModelList.size() + " items");
		this.setAdapter(adapter);
	}

	public Model getModel(int location) {
		return Models.get(location);
	}
}
