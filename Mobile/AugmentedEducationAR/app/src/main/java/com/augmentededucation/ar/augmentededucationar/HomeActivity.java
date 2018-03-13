package com.augmentededucation.ar.augmentededucationar;

import android.Manifest;
import android.app.DownloadManager;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.graphics.Point;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Toast;

import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.augmentededucation.ar.augmentededucationar.WebAccess.WebAccessor;
import com.augmentededucation.ar.augmentededucationar.barcode.ScanQRCodeActivity;
import com.augmentededucation.ar.augmentededucationar.db.entity.Model;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.vision.barcode.Barcode;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.File;
import java.util.ArrayList;
import java.util.Map;


public class HomeActivity extends AppCompatActivity {
	private static final String LOG_TAG = MainActivity.class.getSimpleName();
	private static final int READ_BARCODE = 1;
	private static final int REQUEST_WRITE_EXTERNAL_STORAGE = 3;
	private Model model;

	WebAccessor accessor;
	private String authToken;

	private ModelListView modelsList;
	private String[] models;

	private FileManager fileManager;

	private boolean itemTouched;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_home);

		itemTouched = false;

//		if (ContextCompat.checkSelfPermission(this,
//				Manifest.permission.READ_EXTERNAL_STORAGE) != PackageManager.PERMISSION_GRANTED) {
//
//			// Permission is not granted
//			// Should we show an explanation?
//			if (ActivityCompat.shouldShowRequestPermissionRationale(this,
//					Manifest.permission.READ_EXTERNAL_STORAGE)) {
//					Toast.makeText(this, "Storage permissions needed to read/save models", Toast.LENGTH_LONG).show();
//				ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.READ_EXTERNAL_STORAGE}, REQUEST_WRITE_EXTERNAL_STORAGE);
//			} else {
//				// No explanation needed; request the permission
//				ActivityCompat.requestPermissions(this, new String[]{Manifest.permission.READ_EXTERNAL_STORAGE}, REQUEST_WRITE_EXTERNAL_STORAGE);
//
//			}
//		}

		if (modelsList == null){
			modelsList = findViewById(R.id.modelsList);
		}

		accessor = new WebAccessor(this);
		fileManager = new FileManager(this);

		if (savedInstanceState != null)
			authToken = savedInstanceState.getString(getString(R.string.web_AuthToken));
		else
			authToken = getIntent().getExtras().getString(getString(R.string.web_AuthToken));

		accessor.getAllModelsListing(authToken, WebAccessor.FileDescriptor.ALL, new Response.Listener<JSONObject>()
		{
			@Override
			public void onResponse(JSONObject response) {
				try {
					if (response.get("success").toString().equals("True")) {
						JSONObject result = response.getJSONObject("result");

						int numFiles = result.getInt("totalCount");
						if (numFiles == 0)
							return;

						JSONArray files = result.getJSONArray("files");

						for (int i = 0; i < numFiles; i++)
						{
							JSONObject obj = files.getJSONObject(i);
							Model m = new Model();
							m.url = obj.getString("uri");
							m.name = obj.getString("name");
//							modelsList.add(m);
							if (!m.name.endsWith(".zip"))
								fileManager.addModelToDatabase(m);
						}
						modelsList.refreshList();
					}
					else {
						Toast.makeText(getBaseContext(), response.get("reason").toString(), Toast.LENGTH_SHORT).show();
					}
				}
				catch (JSONException ex) {
					Toast.makeText(getBaseContext(), "Unable to list files", Toast.LENGTH_SHORT).show();
				}
			}
		},
		new Response.ErrorListener() {
			@Override
			public void onErrorResponse(VolleyError error) {
				Toast.makeText(getBaseContext(), error.toString(), Toast.LENGTH_LONG).show();
				//Toast.makeText(getBaseContext(), "Unable to authenticate", Toast.LENGTH_LONG).show();
			}
		});

		try
		{
			String[] assets = getAssets().list("");
			for (String asset : assets)
			{
				if (asset.contains(".obj") && !asset.contains(".mtl"))
				{
					Model m = new Model();
					m.url = FileManager.assetsFileNameSubstring + asset;
					m.location = m.url;
					m.name = asset.substring(0, asset.indexOf(".obj"));
//					modelsList.add(m);
					fileManager.setModelDB(m);
				}
			}

			modelsList.refreshList();

			modelsList.setOnItemClickListener(new AdapterView.OnItemClickListener()
			{
				@Override
				public void onItemClick(AdapterView<?> adapterView, final View view, int i, long l)
				{
					if (ContextCompat.checkSelfPermission(getApplicationContext(), Manifest.permission.READ_EXTERNAL_STORAGE) != PackageManager.PERMISSION_GRANTED) {
						Toast.makeText(getApplicationContext(), "Please enable storage permissions in settings", Toast.LENGTH_SHORT).show();
						return;
					}

					if (!itemTouched) {
						itemTouched = true;
						model = modelsList.getModel(i);
						if (model.location == null)
						{
							fileManager.downloadModel(model, authToken, new BroadcastReceiver()
							{
								@Override
								public void onReceive(Context context, Intent intent)
								{
									ViewInAR();
								}
							});
						} else
						{
							ViewInAR();
						}
					}
				}
			});
		}
		catch (Exception e){
			Log.e("ASSETS", "Unable to read assets");
		}

	}

	public void ViewInAR() {
		Intent intent = new Intent(this, ARActivity.class);
		intent.putExtra(ARActivity.FILENAME_TAG, model.location);
		itemTouched = false;
		startActivity(intent);
	}

	public void scanQRCode(View view) {
		 Intent scanQRCodeIntent = new Intent(this, ScanQRCodeActivity.class);
		 startActivityForResult(scanQRCodeIntent, READ_BARCODE);
	}

	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		// Check which request we're responding to
		if (requestCode == READ_BARCODE) {
			// Make sure the request was successful
			if (resultCode == CommonStatusCodes.SUCCESS) {
				if (data != null) {
					Barcode barcode = (Barcode) data.getExtras().get(ScanQRCodeActivity.BarcodeObject);
					Toast.makeText(this, barcode.rawValue, Toast.LENGTH_LONG).show();
					Point[] p = barcode.cornerPoints;

					Model m = new Model();
					m.url = barcode.rawValue;
					m.name = m.url;
					fileManager.addModelToDatabase(m);

					modelsList.refreshList();

				} else Toast.makeText(this, R.string.no_barcode_captured, Toast.LENGTH_LONG).show();
			} else Log.e(LOG_TAG, String.format(getString(R.string.barcode_error_format),
					CommonStatusCodes.getStatusCodeString(resultCode)));
		} else super.onActivityResult(requestCode, resultCode, data);
	}

	@Override
	public void onSaveInstanceState(Bundle savedInstanceState) {
		super.onSaveInstanceState(savedInstanceState);
		savedInstanceState.putString(getString(R.string.web_AuthToken), authToken);
	}

	@Override
	public void onRequestPermissionsResult(int requestCode,
	                                       @NonNull String[] permissions,
	                                       @NonNull int[] grantResults)
	{
		super.onRequestPermissionsResult(requestCode, permissions, grantResults);
		if (requestCode == REQUEST_WRITE_EXTERNAL_STORAGE)
		{

		}
	}

}
