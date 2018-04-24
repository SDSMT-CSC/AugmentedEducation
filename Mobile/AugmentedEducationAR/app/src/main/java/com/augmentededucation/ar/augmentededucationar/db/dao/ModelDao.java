package com.augmentededucation.ar.augmentededucationar.db.dao;

import android.arch.persistence.room.Dao;
import android.arch.persistence.room.Delete;
import android.arch.persistence.room.Insert;
import android.arch.persistence.room.Query;
import android.arch.persistence.room.Update;

import com.augmentededucation.ar.augmentededucationar.db.entity.Model;

import java.util.List;

import static android.arch.persistence.room.OnConflictStrategy.IGNORE;

/**
 * ModelDao defines the methods used to interact with the database. This DAO corresponds to the
 * Model entity that is in AppDatabase.
 */
@Dao
public interface ModelDao {
    @Query("SELECT * FROM Model")
    List<Model> loadAllModels();

    @Query("SELECT * FROM Model WHERE Model.url = :url")
    List<Model> loadModelWhereURL(String url);

    @Query("UPDATE Model SET location = :newLocation, name = :newName WHERE Model.url = :url")
    void updateModel(String url, String newName, String newLocation);

    @Insert(onConflict = IGNORE)
    void insertModel(Model model);

    @Delete
    void deleteModel(Model model);

    @Query("DELETE FROM Model")
    void deleteAll();
}
