package com.augmentededucation.ar.augmentededucationar;

import android.content.Intent;
import android.graphics.Point;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Toast;

import com.augmentededucation.ar.augmentededucationar.barcode.ScanQRCodeActivity;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.vision.barcode.Barcode;

import java.util.ArrayList;


public class HomeActivity extends AppCompatActivity {
	private static final String LOG_TAG = MainActivity.class.getSimpleName();
	private static final int READ_BARCODE = 1;
	private String fileName = "cone2.obj";

	private ListView modelsList;
	private String[] models;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_home);

		if (modelsList == null){
			modelsList = findViewById(R.id.modelsList);
		}

		try
		{
			String[] assets = getAssets().list("");
			ArrayList<String> tempModelList = new ArrayList<>();
			for (String asset : assets)
			{
				if (asset.contains(".obj") && !asset.contains(".mtl"))
				{
					tempModelList.add(asset);
				}
			}

			models = new String[tempModelList.size()];
			tempModelList.toArray(models);
			ArrayAdapter<String> adapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, models);
			modelsList.setAdapter(adapter);

			modelsList.setOnItemClickListener(new AdapterView.OnItemClickListener()
			{
				@Override
				public void onItemClick(AdapterView<?> adapterView, View view, int i, long l)
				{
					fileName = models[i];
					ViewInAR(view);
				}
			});
		}
		catch (Exception e){
			Log.e("ASSETS", "Unable to read assets");
		}

	}

	public void ViewInAR(View view) {
		Intent intent = new Intent(this, ARActivity.class);
		intent.putExtra(ARActivity.FILENAME_TAG, fileName);
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
					fileName = barcode.rawValue;
				} else Toast.makeText(this, R.string.no_barcode_captured, Toast.LENGTH_LONG).show();
			} else Log.e(LOG_TAG, String.format(getString(R.string.barcode_error_format),
					CommonStatusCodes.getStatusCodeString(resultCode)));
		} else super.onActivityResult(requestCode, resultCode, data);
	}
}
