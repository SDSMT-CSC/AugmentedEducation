package com.augmentededucation.ar.augmentededucationar;

import android.content.Context;
import android.util.AttributeSet;
import android.widget.ArrayAdapter;
import android.widget.ListView;

import java.util.ArrayList;
import java.util.HashMap;

/**
 * Created by kpetr on 2/27/2018.
 */

public class ModelListView extends ListView
{
	Context context;

	private ArrayList<String> editableModelList;
	private HashMap<Integer, String> URIs;
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
		URIs = new HashMap<>();
	}

	public void add(String name, String uri)
	{
		URIs.put(editableModelList.size(), uri);
		editableModelList.add(name);
	}

	public void refreshList()
	{
		modelList = new String[editableModelList.size()];
		editableModelList.toArray(modelList);
		ArrayAdapter<String> adapter = new ArrayAdapter<>(context, android.R.layout.simple_list_item_1, modelList);
		this.setAdapter(adapter);
	}

	public String getURI(int location) {
		return URIs.get(location);
	}
}
