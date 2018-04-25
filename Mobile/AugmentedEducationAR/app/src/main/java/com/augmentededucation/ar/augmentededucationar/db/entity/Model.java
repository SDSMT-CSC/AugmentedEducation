package com.augmentededucation.ar.augmentededucationar.db.entity;

import android.arch.persistence.room.Entity;
import android.arch.persistence.room.PrimaryKey;
import android.support.annotation.NonNull;

/**
 * Defines the field names in the Model entity where Model refers to the 3D models stored and viewed
 * by the user. This creates a table within the AppDatabase object to hold Model objects.
 */
@Entity
public class Model {
    @PrimaryKey
    @NonNull
    public String url;  // url stores place on the website to download the model

    public String name;

    public String location;  // empty string indicates not downloaded
}
