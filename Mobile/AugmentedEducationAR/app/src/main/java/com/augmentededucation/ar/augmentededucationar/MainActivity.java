package com.augmentededucation.ar.augmentededucationar;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.View;
import android.widget.CheckBox;
import android.widget.TextView;
import android.widget.Toast;

import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.augmentededucation.ar.augmentededucationar.WebAccess.WebAccessor;

import org.json.JSONException;
import org.json.JSONObject;

public class MainActivity extends AppCompatActivity {
    private static final String USERNAME = "USERNAME";
    private static final String PASSWORD = "PASSWORD";
    private TextView username;
    private TextView password;
    private String username_string = "";
    private String password_string = "";

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

        SharedPreferences sharedPreferences = getPreferences(Context.MODE_PRIVATE);
        username_string = sharedPreferences.getString(USERNAME, "");
        password_string = sharedPreferences.getString(PASSWORD, "");
        if ("".equals(username_string)) {
            password_string = "";
            ((CheckBox)findViewById(R.id.rememberMeBox)).setChecked(false);
        } else {
            ((CheckBox)findViewById(R.id.rememberMeBox)).setChecked(true);
        }

        username.setText(username_string);
        password.setText(password_string);
    }

    public void continueOffline(View view) {
        Intent intent = new Intent(getBaseContext(), HomeActivity.class);
        intent.putExtra(getString(R.string.web_AuthToken), "");
        startActivity(intent);
    }

    public void onNext(View v) {
        SharedPreferences sharedPreferences = getPreferences(Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        username_string = username.getText().toString();
        password_string = password.getText().toString();
        if (((CheckBox)findViewById(R.id.rememberMeBox)).isChecked()) {
            editor.putString(USERNAME, username_string);
            editor.putString(PASSWORD, password_string);
        } else {
            editor.putString(USERNAME, "");
            editor.putString(PASSWORD, "");
        }
        editor.apply();

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
