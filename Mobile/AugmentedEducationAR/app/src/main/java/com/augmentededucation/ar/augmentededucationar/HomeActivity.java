package com.augmentededucation.ar.augmentededucationar;

import android.Manifest;
import android.app.ProgressDialog;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.graphics.Point;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.v4.content.ContextCompat;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ProgressBar;
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

/**
 * The Home activity where the list of models and scan QE code button are displayed.  Tapping on a button in the
 * list will move to the ARActivity where the model is displayed.  Clicking on the Scan QR code button will move
 * to the ScanQRCodeActivity where the user can scan a QR code.
 */
public class HomeActivity extends AppCompatActivity {

	/**
	 * Logging tag
	 */
	private static final String LOG_TAG = MainActivity.class.getSimpleName();

	/**
	 * Permissions ID numbers
	 */
	private static final int READ_BARCODE = 1;
	private static final int REQUEST_WRITE_EXTERNAL_STORAGE = 3;

	/**
	 * The model that is set when the user taps on a model.  It will be used to send to the ARActivity to display
	 * the model.
	 */
	private Model model;

	/**
	 * The authorization token that should be received from the MainActivity when the user authenticates with the
	 * website.  The value be empty in which online models will not be shown.
	 */
	private String authToken;

	/**
	 * The list that is displayed in the view
	 */
	private ModelListView modelsList;

	/**
	 * Manage files on the phone via a database
	 */
	private FileManager fileManager;

	/**
	 * A flag for once a user taps a model.  If this is true, then taps on models in the list will be ignored.
	 */
	private boolean itemTouched;

	/**
	 * A spinning progress bar to show the user that a model is downloading.
	 */
	private ProgressBar progressBar;

	/**
	 * Called when the activity is created.  It performs the following operations:
	 *      1) TODO: Check for permissions, and ask if not granted
	 *      2) Add models from the website to the list
	 *      3) Add models in the assets folder to the list
	 *
	 * TODO: Put in better error messages/toasts
	 *
	 * @param savedInstanceState The bundle if recreating the activity
	 */
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_home);

		itemTouched = false;

		// Check for permissions (TODO)
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

		if (modelsList == null) {
			modelsList = findViewById(R.id.modelsList);
		}

		if (progressBar == null) {
			progressBar = findViewById(R.id.progressBar);
			progressBar.setVisibility(View.INVISIBLE);
		}

		WebAccessor accessor = new WebAccessor(this);
		fileManager = new FileManager(this);

		if (savedInstanceState != null)
			authToken = savedInstanceState.getString(getString(R.string.web_AuthToken));
		else
			authToken = getIntent().getExtras().getString(getString(R.string.web_AuthToken));

		// Get a list of models from the website
		if (!authToken.equals("")) {
			accessor.getAllModelsListing(authToken, WebAccessor.FileDescriptor.OWNED_PRIVATE, response -> {
				try {
					/*
						Expects json response in the form:
						{
							"success": <"True"/"False">,
							"totalCount": <number of models>,
							"files": [
								{
									"uri": <file url>
									"name": <file name">
								}, ...
							],
							"reason": "<reason>"
						}
					 */
					if (response.get("success").toString().equals("True")) {
						JSONObject result = response.getJSONObject("result");

						int numFiles = result.getInt("totalCount");
						if (numFiles == 0)
							return;

						JSONArray files = result.getJSONArray("files");

						for (int i = 0; i < numFiles; i++) {
							JSONObject obj = files.getJSONObject(i);
							Model m = new Model();
							m.url = obj.getString("uri");
							m.name = obj.getString("name");

							// zip folders are listed, so ignore them because they are duplicates of other files
							if (!m.name.endsWith(".zip"))
								fileManager.addModelToDatabase(m);
						}
						modelsList.refreshList();  // refreshing the list will get the models stored in the database
					} else {
						Toast.makeText(getBaseContext(), response.get("reason").toString(), Toast.LENGTH_SHORT).show();
					}
				} catch (JSONException ex) {
					Toast.makeText(getBaseContext(), "Unable to list files", Toast.LENGTH_SHORT).show();
				}
			},
			error -> {
				Toast.makeText(getBaseContext(), error.toString(), Toast.LENGTH_LONG).show();
				//Toast.makeText(getBaseContext(), "Unable to authenticate", Toast.LENGTH_LONG).show();
			});
		} else {
			modelsList.setIsLocal(true);
		}

		// Add assets to the database, which will be added to the list
		try {
			String[] assets = getAssets().list("");
			for (String asset : assets) {
				// only add obj files
				if (asset.contains(".obj") && !asset.contains(".mtl")) {
					Model m = new Model();
					m.url = FileManager.assetsFileNameSubstring + asset;
					m.location = m.url;
					m.name = asset.substring(0, asset.indexOf(".obj"));
					fileManager.setModelDB(m);
				}
			}

			modelsList.refreshList();

			// Set listener for when a user taps a model in the list
			if (!authToken.equals("")) {
				modelsList.setOnItemClickListener((adapterView, view, i, l) -> {
					if (ContextCompat.checkSelfPermission(getApplicationContext(), Manifest.permission.READ_EXTERNAL_STORAGE) != PackageManager.PERMISSION_GRANTED) {
						Toast.makeText(getApplicationContext(), "Please enable storage permissions in settings", Toast.LENGTH_SHORT).show();
						return;
					}

					// Only register taps if a model is not downloading/processing
					if (!itemTouched) {
						itemTouched = true;
						model = modelsList.getModel(i);
						if (model.location == null) {
							progressBar.setVisibility(View.VISIBLE);
							fileManager.downloadModel(model, authToken,
									new BroadcastReceiver() {
										@Override
										public void onReceive(Context context, Intent intent) {
											progressBar.setVisibility(View.INVISIBLE);
											ViewInAR();
										}
									},
									() -> progressBar.setVisibility(View.INVISIBLE));
						} else {
							ViewInAR();
						}
					}
				});
			} else {
				modelsList.setOnItemClickListener(new AdapterView.OnItemClickListener() {
					@Override
					public void onItemClick(AdapterView<?> adapterView, View view, int i, long l) {
						model = modelsList.getModel(i);
						ViewInAR();
					}
				});
			}
		} catch (Exception e) {
			Log.e("ASSETS", "Unable to read assets");
		}

	}

	/**
	 * Transition to the ARActivity when the a model is downloaded and the user wants to view it
	 */
	public void ViewInAR() {
		Intent intent = new Intent(this, ARActivity.class);
		intent.putExtra(ARActivity.FILENAME_TAG, model.location);
		itemTouched = false;
		startActivity(intent);
	}

	/**
	 * Transition to the ScanQRCodeActivity where the user can add a model to the list via a QR code.
	 * @param view Needed by the onClick to specify which view called it
	 */
	public void scanQRCode(View view) {
		Intent scanQRCodeIntent = new Intent(this, ScanQRCodeActivity.class);
		startActivityForResult(scanQRCodeIntent, READ_BARCODE);
	}

	/**
	 * Catches the result from teh QR scanning activity.  It will catch all results from the startActivityForResult
	 * function, so tags need to be set to only case the results.
	 *
	 * @param requestCode The request code/tag used to specify which call is returning
	 * @param resultCode The result to specify success/failure
	 * @param data Data returned from the activity
	 */
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
					String[] splitUrl = m.url.split("/");
					m.name = splitUrl[splitUrl.length - 1];
					fileManager.addModelToDatabase(m);

					modelsList.refreshList();

				}
				else
					Toast.makeText(this, R.string.no_barcode_captured, Toast.LENGTH_LONG).show();
			}
			else
				Log.e(LOG_TAG, String.format(getString(R.string.barcode_error_format), CommonStatusCodes.getStatusCodeString(resultCode)));
		}
		else
			super.onActivityResult(requestCode, resultCode, data);
	}

	/**
	 * If moving away from the activity, save the auth token so future accesses to the sever are allowed.
	 * @param savedInstanceState The bundle to save the data into
	 */
	@Override
	public void onSaveInstanceState(Bundle savedInstanceState) {
		super.onSaveInstanceState(savedInstanceState);
		savedInstanceState.putString(getString(R.string.web_AuthToken), authToken);
	}

	/**
	 * Called after requesting permissions from the user
	 *
	 * TODO: Actually implement permission requests, and handle the results
	 *
	 * @param requestCode Permission requested
	 * @param permissions The permissions asked
	 * @param grantResults The permissions granted
	 */
	@Override
	public void onRequestPermissionsResult(int requestCode,
	                                       @NonNull String[] permissions,
	                                       @NonNull int[] grantResults) {
		super.onRequestPermissionsResult(requestCode, permissions, grantResults);
		if (requestCode == REQUEST_WRITE_EXTERNAL_STORAGE) {

		}
	}

}
