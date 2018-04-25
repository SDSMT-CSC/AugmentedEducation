package com.augmentededucation.ar.augmentededucationar;

import android.content.Context;
import android.support.test.InstrumentationRegistry;

import com.augmentededucation.ar.augmentededucationar.db.entity.Model;

import org.junit.Before;
import org.junit.Test;

import static org.junit.Assert.*;

/**
 * Class to test the ModelListView
 */
public class ModelListViewTest {
	private Context context;
	private Model model1;
	private Model model2;
	private Model model3;

	@Before
	public void setUp() throws Exception {
		context = InstrumentationRegistry.getContext();

		model1 = new Model();
		model1.name = "Model 1";
		model1.location = null;
		model1.url = null;

		model2 = new Model();
		model2.name = "Model 2";
		model2.location = "file:///somewhere";
		model2.url = null;

		model3 = new Model();
		model3.name = "Model 3";
		model3.location = null;
		model3.url = "http://somewhere.com";
	}

	@Test
	public void constructor() throws Exception {
		ModelListView modelListView = new ModelListView(context);
		assertTrue(true);
	}

	@Test
	public void setIsLocaLTrue() throws Exception {
		ModelListView modelListView = new ModelListView(context);
		modelListView.setIsLocal(true);
		assertTrue("Failed to set isLocal to true", modelListView.getIsLocal());
	}

	@Test
	public void setIsLocalFalse() throws Exception {
		ModelListView modelListView = new ModelListView(context);
		modelListView.setIsLocal(false);
		assertFalse("Failed to set isLocal to false", modelListView.getIsLocal());
	}

	/**
	 * TODO: Mock out the FileManager so it doesn't actually need a database
	 * @throws Exception
	 */
	@Test
	public void add() throws Exception {
//		ModelListView modelListView = new ModelListView(context);
//
//		modelListView.add(model1);
//		modelListView.refreshList();
//		assertEquals("Failed to get correct model at position 0", model1, modelListView.getModel(0));
//
//		modelListView.add(model2);
//		modelListView.refreshList();
//		assertEquals("Failed to get correct model at position 1", model2, modelListView.getModel(1));
	}

	@Test
	public void clear() throws Exception {
		assertTrue(true);

	}

	@Test
	public void refreshList() throws Exception {
		assertTrue(true);

	}

	@Test
	public void getModel() throws Exception {
		assertTrue(true);

	}

}