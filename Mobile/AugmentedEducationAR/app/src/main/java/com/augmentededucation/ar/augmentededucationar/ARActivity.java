package com.augmentededucation.ar.augmentededucationar;

import android.content.Intent;
import android.opengl.GLES20;
import android.opengl.GLSurfaceView;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.GestureDetector;
import android.view.MotionEvent;
import android.view.View;
import android.widget.EditText;
import android.widget.Toast;

import com.augmentededucation.ar.augmentededucationar.arcore.CameraPermissionHelper;
import com.augmentededucation.ar.augmentededucationar.arcore.DisplayRotationHelper;
import com.augmentededucation.ar.augmentededucationar.arcore.rendering.BackgroundRenderer;
import com.augmentededucation.ar.augmentededucationar.arcore.rendering.ObjectRenderer;
import com.augmentededucation.ar.augmentededucationar.arcore.rendering.PlaneRenderer;
import com.augmentededucation.ar.augmentededucationar.arcore.rendering.PointCloudRenderer;
import com.google.ar.core.Anchor;
import com.google.ar.core.Camera;
import com.google.ar.core.Config;
import com.google.ar.core.Frame;
import com.google.ar.core.HitResult;
import com.google.ar.core.Plane;
import com.google.ar.core.PointCloud;
import com.google.ar.core.Session;
import com.google.ar.core.Trackable;
import com.google.ar.core.TrackingState;
import com.google.ar.core.exceptions.UnavailableApkTooOldException;
import com.google.ar.core.exceptions.UnavailableArcoreNotInstalledException;
import com.google.ar.core.exceptions.UnavailableSdkTooOldException;

import java.io.IOException;
import java.util.concurrent.ArrayBlockingQueue;

import javax.microedition.khronos.egl.EGLConfig;
import javax.microedition.khronos.opengles.GL10;

/**
 * The activity to host the ObjectRenderer to display a selected model.  The user should
 * have entered this activity through the HomeActivity where they choose a model they
 * would like to view.
 *
 * A large portion of this code was based on the sample provided by Google to learn
 * ARCore.
 */
public class ARActivity extends AppCompatActivity implements GLSurfaceView.Renderer {

    /**
     * The tag used to store the model name to render in the bundle.
     */
    public static final String FILENAME_TAG = "filename_tag";

    /**
     * Tag used for logging
     */
    private static final String TAG = ARActivity.class.getSimpleName();

    /**
     * The surface view of where to draw the model
     */
    private GLSurfaceView surfaceView;

    /**
     * The AR Core session
     */
    private Session session;

    /**
     * Used to detect taps to put the model on the screen
     */
    private GestureDetector gestureDetector;

    /**
     * Helps handle screen rotations
     */
    private DisplayRotationHelper displayRotationHelper;

    /**
     * AR Core and other instances used to draw the model
     */
    private final BackgroundRenderer backgroundRenderer = new BackgroundRenderer();
    private final ObjectRenderer virtualObject = new ObjectRenderer();
    private final PlaneRenderer planeRenderer = new PlaneRenderer();
    private final PointCloudRenderer pointCloudRenderer = new PointCloudRenderer();

    /**
     * Anchor information of where the model is located
     */
    private Anchor anchor;
    private final float[] anchorMatrix = new float[16];

    /**
     * Scale factor information that is modified when the user hits the "+" and "-" buttons
     */
    private float scaleFactor = .1f;
    private float defaultScaleFactorDiff = 1f;
    private float scaleFactorDiff = defaultScaleFactorDiff;

    /**
     * Queue of taps
     */
    private final ArrayBlockingQueue<MotionEvent> queuedSingleTaps = new ArrayBlockingQueue<MotionEvent>(16);

    /**
     * Name of the file to draw
     */
    private String objectFileName;

    /**
     * Creates the activity by setting up the member variables and registering event handlers.
     * @param savedInstanceState Bundle to hold saved data
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_ar);
        surfaceView = findViewById(R.id.surfaceview);
        displayRotationHelper = new DisplayRotationHelper(this);

        Intent intent = getIntent();

        EditText editText = findViewById(R.id.scale_factor);
        editText.setText(String.valueOf(scaleFactorDiff));
        editText.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {

            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                if (s.length() != 0) {
                    try{
                        scaleFactorDiff = Float.parseFloat(s.toString());
                    }
                    catch (Exception e){
                        scaleFactorDiff = defaultScaleFactorDiff;
                    }
                }
            }

            @Override
            public void afterTextChanged(Editable s) {

            }
        });

        // TODO: Add tap listener
        gestureDetector= new GestureDetector(this, new GestureDetector.SimpleOnGestureListener() {
            @Override
            public boolean onSingleTapUp(MotionEvent e) {
                onSingleTap(e);
                return true;
            }

            @Override
            public boolean onDown(MotionEvent e) {
                return true;
            }
        });

        surfaceView.setOnTouchListener((v, event) -> gestureDetector.onTouchEvent(event));

        // Set up renderer
        surfaceView.setPreserveEGLContextOnPause(true);
        surfaceView.setEGLContextClientVersion(2);
        surfaceView.setEGLConfigChooser(8, 8, 8, 8, 16, 0);
        surfaceView.setRenderer(this);
        surfaceView.setRenderMode(GLSurfaceView.RENDERMODE_CONTINUOUSLY);

        // Create session
        Exception exception = null;
        String message = null;
        try {
            session = new Session(/* context= */ this);
        } catch (UnavailableArcoreNotInstalledException e) {
            message = "Please install ARCore";
            exception = e;
        } catch (UnavailableApkTooOldException e) {
            message = "Please update ARCore";
            exception = e;
        } catch (UnavailableSdkTooOldException e) {
            message = "Please update this app";
            exception = e;
        } catch (Exception e) {
            message = "This device does not support AR";
            exception = e;
        }

        if (message != null) {
            Toast.makeText(this, message, Toast.LENGTH_SHORT).show();
            Log.e(TAG, "Exception creating session", exception);
            return;
        }

        // Create default config and check if supported.
        Config config = new Config(session);
        if (!session.isSupported(config)) {
            Toast.makeText(this, "This device does not support AR", Toast.LENGTH_SHORT).show();
        }
        session.configure(config);

        // TODO remove default file names
        objectFileName = intent.getStringExtra(FILENAME_TAG);
        if (objectFileName == null) {
            objectFileName = "cone2.obj";
        }
    }

    /**
     * Handles single taps to the screen
     * @param e The MotionEvent that is generated
     */
    private void onSingleTap(MotionEvent e) {
        queuedSingleTaps.offer(e);
    }

    /**
     * Loads in the model nd sets up the surface view for Open GL.
     * @param gl GL object
     * @param config Configuration
     */
    @Override
    public void onSurfaceCreated(GL10 gl, EGLConfig config) {
        GLES20.glClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        // Create the texture
        backgroundRenderer.createOnGlThread(this);
        if (session != null) {
            session.setCameraTextureName(backgroundRenderer.getTextureId());
        }

        // This is where the model is loaded
        try
        {
            try
            {
                virtualObject.createOnGlThread(this, objectFileName);
                virtualObject.setMaterialProperties(0.0f, 3.5f, 1.0f, 6.0f);
            } catch (IOException e)
            {
                Log.e(TAG, "Failed to read obj file or material");
            }
            try
            {
                planeRenderer.createOnGlThread(this, "trigrid.png");
            } catch (IOException e)
            {
                Log.e(TAG, "Failed to read plane texture");
            }
            pointCloudRenderer.createOnGlThread(this);
        }
        catch (Exception e)
        {
            Log.e(TAG, "Error during onSurfaceCreated");
        }
    }

    /**
     * When the surface is changed
     * @param gl GL object
     * @param width The Width
     * @param height The Height
     */
    @Override
    public void onSurfaceChanged(GL10 gl, int width, int height) {
        displayRotationHelper.onSurfaceChanged(width, height);
        GLES20.glViewport(0, 0, width, height);
    }

    /**
     * Draws the screen
     * @param gl The gl object used to draw
     */
    @Override
    public void onDrawFrame(GL10 gl) {
        GLES20.glClear(GLES20.GL_COLOR_BUFFER_BIT | GLES20.GL_DEPTH_BUFFER_BIT);

        if (session == null) {
            return;
        }

        displayRotationHelper.updateSessionIfNeeded(session);

        try {
            Frame frame = session.update();
            Camera camera = frame.getCamera();

            // TODO MotionEvent tap
            MotionEvent tap = queuedSingleTaps.poll();
            if (tap != null && camera.getTrackingState() == TrackingState.TRACKING) {
                for (HitResult hit : frame.hitTest(tap)) {
                    Trackable trackable = hit.getTrackable();
                    if (trackable instanceof  Plane && ((Plane) trackable).isPoseInPolygon(hit.getHitPose())) {
                        anchor = hit.createAnchor();
                    }
                }
            }

            backgroundRenderer.draw(frame);

            if (camera.getTrackingState() == TrackingState.PAUSED) {
                    return;
            }

            float[] projmtx = new float[16];
            camera.getProjectionMatrix(projmtx, 0, 0.1f, 100.0f);

            float[] viewmtx = new float[16];
            camera.getViewMatrix(viewmtx, 0);

            final float lightIntensity = frame.getLightEstimate().getPixelIntensity();

            PointCloud pointCloud = frame.acquirePointCloud();
            pointCloudRenderer.update(pointCloud);
            pointCloudRenderer.draw(viewmtx, projmtx);

            pointCloud.release();

            // Visualize planes
            planeRenderer.drawPlanes(
                    session.getAllTrackables(Plane.class), camera.getDisplayOrientedPose(), projmtx);

            if (anchor != null) {
                if (anchor.getTrackingState() == TrackingState.TRACKING) {
                    anchor.getPose().toMatrix(anchorMatrix, 0);

                    virtualObject.updateModelMatrix(anchorMatrix, scaleFactor);
                    virtualObject.draw(viewmtx, projmtx, lightIntensity);
                }
            }
        } catch (Throwable t) {
            Log.e(TAG, "Exception on the OpenGL thread", t);
        }
    }

    /**
     * When the activity resumes after calling onPause()
     * Resumes the session and viewers.
     */
    @Override
    protected void onResume() {
        super.onResume();

        if (CameraPermissionHelper.hasCameraPermission(this))
        {
            if (session != null) {
                session.resume();
            }
            surfaceView.onResume();
            displayRotationHelper.onResume();
        } else {
            CameraPermissionHelper.requestCameraPermission(this);
        }
    }

    @Override
    protected void onPause() {
        super.onPause();
        displayRotationHelper.onPause();
        surfaceView.onPause();
        if (session != null) {
            session.pause();
        }
    }

    public void onBig(View v) {
        scaleFactor += scaleFactorDiff;
    }

    public void onLittle(View v) {
        scaleFactor -= scaleFactorDiff;
        if (scaleFactor < 0.01f) {
            scaleFactor = 0.01f;
        }
    }
}
