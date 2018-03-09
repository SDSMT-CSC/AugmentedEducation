package com.augmentededucation.ar.augmentededucationar.WebAccess;

import android.app.DownloadManager;
import android.content.Context;
import android.net.Uri;
import android.os.Environment;

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
import java.util.HashMap;
import java.util.Map;

/**
 * Created by kpetr on 2/25/2018.
 */

public class WebAccessor
{
	DownloadManager downloadManager;
	private RequestQueue requestQueue;

	private static String baseAddress = "http://sdminesaugmentededucation.azurewebsites.net";
	private static String mobileAuth = "MobileAuth";
	private static String requestAuthToken = "RequestAuthToken";
	private static String listFiles = "ListFiles";
	private static String downloadFile = "DownloadFile";

	public WebAccessor(Context context) {
		requestQueue = Volley.newRequestQueue(context);
		downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
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

	public void getAllModelsListing(final String authToken, FileDescriptor types, Response.Listener<JSONObject> listener, Response.ErrorListener eListener) {

		String url = String.format("%s/%s/%s/?descriptor=%d", baseAddress, mobileAuth, listFiles, types.ordinal());
		JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null, listener, eListener)
		{
			@Override
			public Map<String, String> getHeaders() {
				HashMap<String, String> headers = new HashMap<>();
				headers.put("token", authToken);
				return headers;
			}
		};
		requestQueue.add(jsonObjectRequest);
	}

	public long downloadFile(Context context, String authToken, String uri, String destDir, String destName) {
		String url = String.format("%s/%s/%s/", baseAddress, mobileAuth, downloadFile);

		DownloadManager.Request request = new DownloadManager.Request(Uri.parse(url));
		request.addRequestHeader("token", authToken);
		request.addRequestHeader("fileUri", uri);
		request.setTitle(destName);
		//request.setDestinationInExternalFilesDir(context, destDir, destName);
		request.setDestinationInExternalPublicDir(Environment.DIRECTORY_DOWNLOADS, destName);

		return downloadManager.enqueue(request);
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
