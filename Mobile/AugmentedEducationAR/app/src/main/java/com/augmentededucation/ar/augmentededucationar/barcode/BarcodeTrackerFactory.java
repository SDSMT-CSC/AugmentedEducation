package com.augmentededucation.ar.augmentededucationar.barcode;

import android.content.Context;

import com.google.android.gms.vision.MultiProcessor;
import com.google.android.gms.vision.Tracker;
import com.google.android.gms.vision.barcode.Barcode;

/**
 * Created by 7296837 on 2/6/2018.
 */

public class BarcodeTrackerFactory implements MultiProcessor.Factory<Barcode>{
    private Context mContext;

    BarcodeTrackerFactory(Context context) {
        mContext = context;
    }

    @Override
    public Tracker<Barcode> create(Barcode barcode) {
        return new BarcodeTracker(mContext);
    }
}
