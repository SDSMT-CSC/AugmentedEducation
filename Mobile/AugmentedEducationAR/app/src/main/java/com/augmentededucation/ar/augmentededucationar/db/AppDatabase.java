package com.augmentededucation.ar.augmentededucationar.db;

import android.arch.persistence.room.Database;
import android.arch.persistence.room.Room;
import android.arch.persistence.room.RoomDatabase;
import android.content.Context;

import com.augmentededucation.ar.augmentededucationar.db.dao.ModelDao;
import com.augmentededucation.ar.augmentededucationar.db.entity.Model;

/**
 * Creates a database holder called AppDatabase that extends RoomDatabase. Contains methods to
 * create and return an instance of the database, and destroy the instance. Entities such as Model
 * are stored in the AppDatabase object.
 */
@Database(entities = {Model.class}, version = 1)
public abstract class AppDatabase extends RoomDatabase{

    private static AppDatabase sInstance;

    public static AppDatabase getInstance(Context context, String dbName) {
        if (sInstance == null) {
            sInstance = Room.databaseBuilder(context, AppDatabase.class, dbName).allowMainThreadQueries().build();
        }

        return sInstance;
    }

    public abstract ModelDao modelDao();

    public static void destroyInstance() {
        sInstance = null;
    }
}
