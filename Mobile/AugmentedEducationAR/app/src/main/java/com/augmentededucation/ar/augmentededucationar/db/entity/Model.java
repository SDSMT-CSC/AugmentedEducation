package com.augmentededucation.ar.augmentededucationar.db.entity;

/**
 * Created by 7296837 on 2/27/2018.
 */

import android.arch.persistence.room.Entity;
import android.arch.persistence.room.PrimaryKey;
import android.support.annotation.NonNull;

@Entity
public class Model {
    @PrimaryKey
    @NonNull
    public String url;  // url stores place on the website to download the model

    public String name;

    public String location;  // empty string indicates not downloaded
}
