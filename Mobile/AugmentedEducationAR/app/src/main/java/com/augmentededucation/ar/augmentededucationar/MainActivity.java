package com.augmentededucation.ar.augmentededucationar;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.View;
import android.widget.TextView;
import android.widget.Toast;

import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.augmentededucation.ar.augmentededucationar.WebAccess.WebAccessor;

import org.json.JSONException;
import org.json.JSONObject;

public class MainActivity extends AppCompatActivity {

    private TextView username;
    private TextView password;

    private WebAccessor accessor;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        if (username == null) {
            username = findViewById(R.id.usernameInput);
        }

        if (password == null) {
            password = findViewById(R.id.passwordInput);
        }

        if (accessor == null) {
            accessor = new WebAccessor(this);
        }
    }

    public void onNext(View v) {
        accessor.authenticate(username.getText().toString(), password.getText().toString(),
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        try
                        {
                            if (response.get("success").toString().equals("True"))
                            {
                                Intent intent = new Intent(getBaseContext(), HomeActivity.class);
                                intent.putExtra(getString(R.string.web_AuthToken), response.getString("token"));
                                startActivity(intent);
                            }
                            else {
                                Toast.makeText(getBaseContext(), response.get("reason").toString(), Toast.LENGTH_SHORT).show();
                            }
                        }
                        catch (JSONException ex) {
                            //Toast.makeText(getBaseContext(), "Unable to authenticate", Toast.LENGTH_SHORT).show();
                            Toast.makeText(getBaseContext(), response.toString(), Toast.LENGTH_SHORT).show();
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        Toast.makeText(getBaseContext(), error.toString(), Toast.LENGTH_LONG).show();
                        //Toast.makeText(getBaseContext(), "Unable to authenticate", Toast.LENGTH_LONG).show();
                    }
                }
        );

    }
}
