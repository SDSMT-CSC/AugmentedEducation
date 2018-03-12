package com.augmentededucation.ar.augmentededucationar.WebAccess;

import android.app.DownloadManager;
import android.content.Context;
import android.net.Uri;
import android.os.Environment;
import android.widget.Toast;

import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.TimeoutError;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.Volley;
import com.augmentededucation.ar.augmentededucationar.R;
import com.augmentededucation.ar.augmentededucationar.db.entity.Model;

import org.json.JSONArray;
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

	public void downloadFile(final Context context, final String authToken, final String uri, final String destName, final DownloadQueued queued) {
		String url = String.format("%s/%s/%s/", baseAddress, mobileAuth, downloadFile);
		JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null,
			new Response.Listener<JSONObject>() {
				@Override
				public void onResponse(JSONObject response)
				{
					try {
						if (response.get("success").toString().equals("True"))
						{
							String webUrl = response.getString("result");

							DownloadManager.Request request = new DownloadManager.Request(Uri.parse(webUrl));
							request.addRequestHeader("token", authToken);
							request.setTitle(destName);
							request.setDestinationInExternalPublicDir(Environment.DIRECTORY_DOWNLOADS, destName);

							queued.downloadQueued(downloadManager.enqueue(request));
						}
						else {
							Toast.makeText(context, "Unable to download file", Toast.LENGTH_SHORT).show();
						}
					}
					catch (JSONException ex) {
						Toast.makeText(context, "Error downloading file", Toast.LENGTH_SHORT).show();
					}

				}
			},
			new Response.ErrorListener()  {
				@Override
				public void onErrorResponse(VolleyError error)
				{
					if (error instanceof TimeoutError)
						Toast.makeText(context, "Request timeout", Toast.LENGTH_SHORT).show();
					else
						Toast.makeText(context, "Unable to download file", Toast.LENGTH_SHORT).show();
				}
			})
		{
			@Override
			public Map<String, String> getHeaders() {
				HashMap<String, String> headers = new HashMap<>();
				headers.put("token", authToken);
				headers.put("fileUri", uri);
				return headers;
			}
		};
		jsonObjectRequest.setRetryPolicy(new DefaultRetryPolicy(
				5000,
				DefaultRetryPolicy.DEFAULT_MAX_RETRIES,
				DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));

		requestQueue.add(jsonObjectRequest);
	}

	public enum FileDescriptor
	{
		ALL,
		OWNED_ALL,
		OWNED_PRIVATE,
		OWNED_PUBLIC,
		NOT_OWNED_PUBLIC
	}

	public interface DownloadQueued {
		void downloadQueued(long downloadId);
	}
}
