package com.augmentededucation.ar.augmentededucationar;

import android.content.Context;
import android.util.AttributeSet;
import android.widget.ArrayAdapter;
import android.widget.ListView;

import com.augmentededucation.ar.augmentededucationar.db.entity.Model;

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
	}

	public void add(Model model)
	{
		Models.put(editableModelList.size(), model);
		editableModelList.add(model.name);
	}

	public void refreshList()
	{
		modelList = new String[editableModelList.size()];
		editableModelList.toArray(modelList);
		ArrayAdapter<String> adapter = new ArrayAdapter<>(context, android.R.layout.simple_list_item_1, modelList);
		this.setAdapter(adapter);
	}

	public Model getModel(int location) {
		return Models.get(location);
	}
}
