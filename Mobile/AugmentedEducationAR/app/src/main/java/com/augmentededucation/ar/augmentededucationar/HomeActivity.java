package com.augmentededucation.ar.augmentededucationar;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.View;

import com.augmentededucation.ar.augmentededucationar.barcode.ScanQRCodeActivity;


public class HomeActivity extends AppCompatActivity
{
	private static final int READ_BARCODE = 1;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_home);
	}

	public void ViewInAR(View view) {
		Intent intent = new Intent(this, ARActivity.class);
		startActivity(intent);
	}

	public void scanQRCode(View view) {
		 Intent scanQRCodeIntent = new Intent(this, ScanQRCodeActivity.class);
		 startActivityForResult(scanQRCodeIntent, READ_BARCODE);
	}
}
