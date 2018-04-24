/*
 * Copyright (C) The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.augmentededucation.ar.augmentededucationar.barcode;

import android.content.Context;

import com.google.android.gms.vision.Tracker;
import com.google.android.gms.vision.barcode.Barcode;

/**
 * Tracker used for tracking or reading a barcode. This is used to receive newly detected items.
 */
class BarcodeTracker extends Tracker<Barcode> {
    private BarcodeGraphicTrackerCallback mListener;

    /**
     * An interface where the function onDetectedQrCode will be called after attempting to
     * find a barcode at the Activity level.
     */
    public interface BarcodeGraphicTrackerCallback {
        void onDetectedQrCode(Barcode barcode);
    }

    BarcodeTracker(Context listener) {
        mListener = (BarcodeGraphicTrackerCallback) listener;
    }

    /**
     * This method is called to assert a new item when it is detected.
     *
     * @param id The id representing the detected item
     * @param item Detection information regarding the newly detected item
     */
    @Override
    public void onNewItem(int id, Barcode item) {
        if (item.displayValue != null) {
            mListener.onDetectedQrCode(item);
        }
    }
}
