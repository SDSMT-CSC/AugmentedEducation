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
 * An extension of a ListView to list the models available.  It provides an easier method for adding models to the list,
 * since there are multiple locations where the models can come from (assets, on the phone, online) and there may be some
 * latency between getting the listings.
 *
 * The general use overview is:
 *  1) Use add() to add a model to the list
 *  2) After done adding models to the list (for a time) call refreshList() to refresh the listing that will show in the
 *          view.  This is more efficient since refreshing the list means allocating new memory, which can be resource intensive.
 */
public class ModelListView extends ListView
{
	/**
	 * The context used to display the list
	 */
	private Context context;

	/**
	 * The temporary list where models can be manipulated.  When refreshList is called, the contents will be put in the official
	 * displayed list.
	 */
	private ArrayList<String> editableModelList;


	/**
	 * A map to keep track of where in the list each model is located.
	 */
	private HashMap<Integer, Model> Models;

	/**
	 * Interfaces with the database to keep track of file locations (i.e. on the phone, on the server, etc.)
	 */
	private FileManager fileManager;

	/**
	 * A flag to state whether to only load local files, or remote as well.
	 */
	private Boolean isLocal = false;

	public void setFileManager(FileManager manager) {
		fileManager = manager;
	}

	/**
	 * Setter to only load local models, or show remote as well.
	 * @param isLocal Set whether to show only local models or not
	 */
	public void setIsLocal(Boolean isLocal) {
		this.isLocal = isLocal;
	}

	/**
	 * A getter for the isLocal member variable.  Variable used to denote only
	 * viewing local or all models.
	 * @return isLocal member variable
	 */
	public Boolean getIsLocal() {return this.isLocal;}

	/**
	 * Constructor that initializes the context
	 * @param context Where to show the list
	 */
	public ModelListView(Context context) {
		super(context);
		init(context);
	}

	/**
	 * Additional constructor needed by extending ListView
	 * @param context context
	 * @param attrs attributes
	 */
	public ModelListView(Context context, AttributeSet attrs)
	{
		super(context, attrs);
		init(context);
	}

	/**
	 * Additional constructor needed by extending ListView
	 * @param context context
	 * @param attrs attributes
	 * @param defStyle defStyle
	 */
	public ModelListView(Context context, AttributeSet attrs, int defStyle)
	{
		super(context, attrs, defStyle);
		init(context);
	}

	/**
	 * Perform the class initialization, used by all three constructors.
	 * @param context Where to create the list view
	 */
	private void init(Context context)
	{
		this.context = context;
		editableModelList = new ArrayList<>();
		Models = new HashMap<>();
		fileManager = new FileManager(context);
	}

	/**
	 * Add a model to the list.
	 * NOTE: This will not reload the list.  So an added model will not be shown until refreshList() is called.
	 * @param model The model to add to the list
	 */
	public void add(Model model)
	{
		Models.put(editableModelList.size(), model);
		editableModelList.add(model.name);
	}

	/**
	 * Removes all models from the list.
	 * NOTE: This will not show in the list until refreshList() is called.
	 */
	public void clear()
	{
		Models.clear();
		editableModelList.clear();
	}

	/**
	 * Refresh the list with all changes.  Call this sparingly as it is fairly resource intensive due to the
	 * requirement of allocating new memory.
	 */
	public void refreshList()
	{
		// clear out the list in preparation of repopulating it.
		this.clear();

		// Get models tracked by the database
		ArrayList<Model> mList;
		if (isLocal) {
			mList = fileManager.getLocalModels();
		} else {
			mList = fileManager.getListOfModels();
		}

		// Add the model to the list
		for (Model m : mList) {
			if (m.location != null && !m.location.startsWith(FileManager.assetsFileNameSubstring)) {
				// Ensure that for a model the database thinks exists on the phone, that it actually does.
				// If not, set the database that it does not exist locally anymore.
				File file = new File(m.location);
				if (!file.exists()){
					m.location = null;
					fileManager.setModelDB(m);
				}
			}
			add(m);
		}

		// Redisplay the newly created list
		String[] modelList = new String[editableModelList.size()];
		editableModelList.toArray(modelList);
		ArrayAdapter<String> adapter = new ArrayAdapter<>(context, android.R.layout.simple_list_item_1, modelList);
		this.setAdapter(adapter);
	}

	/**
	 * A getter to get the model at a location in the list.  If the location is out of range, null is returned.
	 * @param location The location of the model to get
	 * @return The model at the desired location
	 */
	public Model getModel(int location) {
		if (location >= Models.size() || location < 0)
			return null;
		return Models.get(location);
	}
}
