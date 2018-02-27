package com.augmentededucation.ar.augmentededucationar.WebAccess;

import android.app.DownloadManager;
import android.content.Context;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.Volley;
import com.augmentededucation.ar.augmentededucationar.R;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;

/**
 * Created by kpetr on 2/25/2018.
 */

public class WebAccessor
{
	private RequestQueue requestQueue;

	private static String baseAddress = "http://sdminesaugmentededucation.azurewebsites.net";
	private static String mobileAuth = "MobileAuth";
	private static String requestAuthToken = "RequestAuthToken";
	private static String listFiles = "ListFiles";

	public WebAccessor(Context context) {
		requestQueue = Volley.newRequestQueue(context);
	}

	public void authenticate(String username, String password, Response.Listener<JSONObject> listener, Response.ErrorListener eListener) {
		JSONObject jsonObject = new JSONObject();

		try
		{
			jsonObject.put("userName", username);
			jsonObject.put("password", password);
		}
		catch (JSONException ex) {
			return;
		}

		String url = String.format("%s/%s/%s/", baseAddress, mobileAuth, requestAuthToken);
		JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url, jsonObject, listener, eListener);
		requestQueue.add(jsonObjectRequest);
	}

	public void getModelListing(String authToken, FileDescriptor descriptor, int pageNumber, Response.Listener<JSONObject> listener, Response.ErrorListener eListener) {
		JSONObject jsonObject = new JSONObject();

		try
		{
			jsonObject.put("token", authToken);
		}
		catch (JSONException ex) {
			return;
		}

		String url = String.format("%s/%s/%s/%d/%d/", baseAddress, mobileAuth, listFiles, descriptor, pageNumber);
		JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(url, jsonObject, listener, eListener);
		requestQueue.add(jsonObjectRequest);
	}

	public void getModelListing(String authToken, FileDescriptor descriptor, Response.Listener<JSONObject> listener, Response.ErrorListener eListener) {
		getModelListing(authToken, descriptor, 1, listener, eListener);
	}
		public enum FileDescriptor
	{
		ALL,
		OWNED_ALL,
		OWNED_PRIVATE,
		OWNED_PUBLIC,
		NOT_OWNED_PUBLIC
	}
}
