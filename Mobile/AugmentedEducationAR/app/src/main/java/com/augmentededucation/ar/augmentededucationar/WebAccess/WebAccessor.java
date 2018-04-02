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
import com.android.volley.ServerError;
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
 * A class to handle communication with the website.  The Volley library is used to perform the network operations.
 * The callbacks needed by Volley for a completed or error condition must be provided by the calling class.
 */
public class WebAccessor
{
	/**
	 * DownloadManager to perform file downloads
	 */
	private DownloadManager downloadManager;

	/**
	 * The queue to place the web requests
	 */
	private RequestQueue requestQueue;

	/**
	 * The base address of where to send the requests
	 */
	private static String baseAddress = "http://sdminesaugmentededucation.azurewebsites.net";

	/**
	 * A prefix to add to the base address in order to contact the website
	 */
	private static String mobileAuth = "MobileAuth";

	/**
	 * The string to append to the web request when getting an authentication token
	 */
	private static String requestAuthToken = "RequestAuthToken";

	/**
	 * String to append in order to get a file listing
	 */
	private static String listFiles = "ListFiles";

	/**
	 * Strign to append to download a file
	 */
	private static String downloadFile = "DownloadFile";

	/**
	 * The constructor to set up the DownloadManager and RequestQueue
	 * @param context context to create the DownloadManager and RequestQueue from
	 */
	public WebAccessor(Context context) {
		requestQueue = Volley.newRequestQueue(context);
		downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
	}

	/**
	 * Authenticate a user with a username and password.  A successful response will send an authentication token back
	 * that must be used for other requests to the website.
	 * @param username The username of the user
	 * @param password The password of the user
	 * @param listener A listener to accept a response from the website
	 * @param eListener An error listener if an error occurs during communication
	 *
	 * TODO: Setup/use TLS/SSL on both the web and mobile side
	 */
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

	/**
	 * Retrieve a listing of models that a user owns, from the website.  A call to authenticate to get an auth token must be performed before this
	 * will succeed.
	 *
	 * @param authToken The auth token retrieved from a call to authenticate
	 * @param types The type of file to be shown (an enum defined in this class)
	 * @param listener The response listener to handle a successful request
	 * @param eListener The error listener that is called when an error occurs during execution
	 */
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

	/**
	 * Downloads a file from the website.  There are two steps to download a file:
	 *  1) Contact the server at a predefined URL to get the actual download URL
	 *  2) Download the file from the download URL provided in step 1
	 *
	 * @param context The context to make Toasts if errors occur
	 * @param authToken The auth token to communicate with the server
	 * @param uri The URI/name of the file to download
	 * @param destName The folder name to save the file into
	 * @param queued An instance of an interface for when a download is complete
	 * @param downloadError A callback to call if an error occurs
	 */
	public void downloadFile(final Context context, final String authToken, final String uri, final String destName, final DownloadQueued queued, onDownloadError downloadError) {
		String url = String.format("%s/%s/%s/", baseAddress, mobileAuth, downloadFile);
		JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null,
				response -> {
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
							Toast.makeText(context, "Unable to download file - " + response.getString("reason"), Toast.LENGTH_SHORT).show();
							downloadError.onError();
						}
					}
					catch (JSONException ex) {
						Toast.makeText(context, "Error downloading file", Toast.LENGTH_SHORT).show();
						downloadError.onError();
					}

				},
				error -> {
					if (error instanceof TimeoutError)
						Toast.makeText(context, "Request timeout", Toast.LENGTH_SHORT).show();
					else if (error instanceof ServerError)
						Toast.makeText(context, "Server Error - unable to download file", Toast.LENGTH_SHORT).show();
					else
						Toast.makeText(context, "Unable to download file - " + error.toString(), Toast.LENGTH_SHORT).show();

					downloadError.onError();
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

	/**
	 * An enum to define the levels of access to retrieve a file with.  This should be the same as the website for consistency.
	 */
	public enum FileDescriptor
	{
		ALL,
		OWNED_ALL,
		OWNED_PRIVATE,
		OWNED_PUBLIC,
		NOT_OWNED_PUBLIC
	}

	/**
	 * An interface defined to call when a file download has been queued
	 */
	public interface DownloadQueued {
		void downloadQueued(long downloadId);
	}

	/**
	 * An interface that will be called when an error occurs
	 */
	public interface onDownloadError {
		void onError();
	}
}
