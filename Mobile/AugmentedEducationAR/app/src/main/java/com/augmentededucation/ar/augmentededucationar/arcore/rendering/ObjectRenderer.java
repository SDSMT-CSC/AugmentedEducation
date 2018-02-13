/*
 * Copyright 2017 Google Inc. All Rights Reserved.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.augmentededucation.ar.augmentededucationar.arcore.rendering;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Rect;
import android.opengl.GLES20;
import android.opengl.GLES30;
import android.opengl.GLUtils;
import android.opengl.Matrix;
import android.util.Log;

import com.augmentededucation.ar.augmentededucationar.R;

import java.io.IOException;
import java.io.InputStream;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.FloatBuffer;
import java.nio.IntBuffer;
import java.nio.ShortBuffer;
import java.util.List;

import de.javagl.obj.FloatTuple;
import de.javagl.obj.Mtl;
import de.javagl.obj.MtlReader;
import de.javagl.obj.Obj;
import de.javagl.obj.ObjData;
import de.javagl.obj.ObjFace;
import de.javagl.obj.ObjGroup;
import de.javagl.obj.ObjReader;
import de.javagl.obj.ObjUtils;

/**
 * Renders an object loaded from an OBJ file in OpenGL.
 */
public class ObjectRenderer {
    private static final String TAG = ObjectRenderer.class.getSimpleName();

    /**
     * Blend mode.
     *
     * @see #setBlendMode(BlendMode)
     */
    public enum BlendMode {
        /** Multiplies the destination color by the source alpha. */
        Shadow,
        /** Normal alpha blending. */
        Grid
    }

    private static final int COORDS_PER_VERTEX = 3;

    // Note: the last component must be zero to avoid applying the translational part of the matrix.
    private static final float[] LIGHT_DIRECTION = new float[] { 0.250f, 0.866f, 0.433f, 0.0f };
    private float[] mViewLightDirection = new float[4];

    // Object vertex buffer variables.
    private int mVertexBufferId;
    private int mVerticesBaseAddress;
    private int mTexCoordsBaseAddress;
    private int mNormalsBaseAddress;
    private int mIndexBufferId;
    private int mIndexCount;
    private int[] vectorArrayObjectIds;

    private int mProgram;
    private int[] mTextures;

    // Shader location: model view projection matrix.
    private int mModelViewUniform;
    private int mModelViewProjectionUniform;

    // Shader location: object attributes.
    private int mPositionAttribute;
    private int mNormalAttribute;
    private int mTexCoordAttribute;

    // Shader location: texture sampler.
    private int mTextureUniform;

    // Shader location: environment properties.
    private int mLightingParametersUniform;

    // Shader location: material properties.
    private int mMaterialParametersUniform;

    private BlendMode mBlendMode = null;

    private Obj mObj;

    // Temporary matrices allocated here to reduce number of allocations for each frame.
    private float[] mModelMatrix = new float[16];
    private float[] mModelViewMatrix = new float[16];
    private float[] mModelViewProjectionMatrix = new float[16];

    // Set some default material properties to use for lighting.
    private float mAmbient = 0.3f;
    private float mDiffuse = 1.0f;
    private float mSpecular = 1.0f;
    private float mSpecularPower = 6.0f;

    private Rect mrect = new Rect(0, 0, 1, 1);
    private Bitmap mtextureBitmap = Bitmap.createBitmap(mrect.width(), mrect.height(), Bitmap.Config.ARGB_8888);
    private Canvas canvas = new Canvas(mtextureBitmap);
    private Paint paint = new Paint();


    public ObjectRenderer() {
    }

    /**
     * Creates and initializes OpenGL resources needed for rendering the model.
     *
     * @param context Context for loading the shader and below-named model and texture assets.
     * @param objAssetName  Name of the OBJ file containing the model geometry.
     * @param diffuseTextureAssetName  Name of the PNG file containing the diffuse texture map.
     */
    public void createOnGlThread(Context context, String objAssetName,
                                 String diffuseTextureAssetName) throws IOException {
        // Read the obj file.
        InputStream objInputStream = context.getAssets().open(objAssetName);
        Obj obj = ObjReader.read(objInputStream);

        // Prepare the Obj so that its structure is suitable for
        // rendering with OpenGL:
        // 1. Triangulate it
        // 2. Make sure that texture coordinates are not ambiguous
        // 3. Make sure that normals are not ambiguous
        // 4. Convert it to single-indexed data
        obj = ObjUtils.convertToRenderable(obj);

        // OpenGL does not use Java arrays. ByteBuffers are used instead to provide data in a format
        // that OpenGL understands.

        // Obtain the data from the OBJ, as direct buffers:
        IntBuffer wideIndices = ObjData.getFaceVertexIndices(obj, 3);
        FloatBuffer vertices = ObjData.getVertices(obj);
        FloatBuffer texCoords = ObjData.getTexCoords(obj, 2);
        FloatBuffer normals = ObjData.getNormals(obj);

        // Convert int indices to shorts for GL ES 2.0 compatibility
        ShortBuffer indices = ByteBuffer.allocateDirect(2 * wideIndices.limit())
            .order(ByteOrder.nativeOrder()).asShortBuffer();
        while (wideIndices.hasRemaining()) {
            indices.put((short) wideIndices.get());
        }
        indices.rewind();

        int[] buffers = new int[2];
        GLES20.glGenBuffers(2, buffers, 0);
        mVertexBufferId = buffers[0];
        mIndexBufferId = buffers[1];

        // Load vertex buffer
        mVerticesBaseAddress = 0;
        mTexCoordsBaseAddress = mVerticesBaseAddress + 4 * vertices.limit();
        mNormalsBaseAddress = mTexCoordsBaseAddress + 4 * texCoords.limit();
        final int totalBytes = mNormalsBaseAddress + 4 * normals.limit();

        GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, mVertexBufferId);
        GLES20.glBufferData(GLES20.GL_ARRAY_BUFFER, totalBytes, null, GLES20.GL_STATIC_DRAW);
        GLES20.glBufferSubData(
            GLES20.GL_ARRAY_BUFFER, mVerticesBaseAddress, 4 * vertices.limit(), vertices);
        GLES20.glBufferSubData(
            GLES20.GL_ARRAY_BUFFER, mTexCoordsBaseAddress, 4 * texCoords.limit(), texCoords);
        GLES20.glBufferSubData(
            GLES20.GL_ARRAY_BUFFER, mNormalsBaseAddress, 4 * normals.limit(), normals);
        GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, 0);

        // Load index buffer
        GLES20.glBindBuffer(GLES20.GL_ELEMENT_ARRAY_BUFFER, mIndexBufferId);
        mIndexCount = indices.limit();
        GLES20.glBufferData(
            GLES20.GL_ELEMENT_ARRAY_BUFFER, 2 * mIndexCount, indices, GLES20.GL_STATIC_DRAW);
        GLES20.glBindBuffer(GLES20.GL_ELEMENT_ARRAY_BUFFER, 0);

        ShaderUtil.checkGLError(TAG, "OBJ buffer load");

        final int vertexShader = ShaderUtil.loadGLShader(TAG, context,
                GLES20.GL_VERTEX_SHADER, R.raw.object_vertex);
        final int fragmentShader = ShaderUtil.loadGLShader(TAG, context,
                GLES20.GL_FRAGMENT_SHADER, R.raw.object_fragment);

        mProgram = GLES20.glCreateProgram();
        GLES20.glAttachShader(mProgram, vertexShader);
        GLES20.glAttachShader(mProgram, fragmentShader);
        GLES20.glLinkProgram(mProgram);
        GLES20.glUseProgram(mProgram);

        ShaderUtil.checkGLError(TAG, "Program creation");

        mModelViewUniform = GLES20.glGetUniformLocation(mProgram, "u_ModelView");
        mModelViewProjectionUniform =
            GLES20.glGetUniformLocation(mProgram, "u_ModelViewProjection");

        mPositionAttribute = GLES20.glGetAttribLocation(mProgram, "a_Position");
        mNormalAttribute = GLES20.glGetAttribLocation(mProgram, "a_Normal");
        mTexCoordAttribute = GLES20.glGetAttribLocation(mProgram, "a_TexCoord");

        mTextureUniform = GLES20.glGetUniformLocation(mProgram, "u_Texture");

        mLightingParametersUniform = GLES20.glGetUniformLocation(mProgram, "u_LightingParameters");
        mMaterialParametersUniform = GLES20.glGetUniformLocation(mProgram, "u_MaterialParameters");

        ShaderUtil.checkGLError(TAG, "Program parameters");

        Matrix.setIdentityM(mModelMatrix, 0);
    }


    /**
     * Creates and initializes OpenGL resources needed for rendering the model.
     * Taken from https://github.com/JohnLXiang/arcore-sandbox
     * @param context
     *         Context for loading the shader and below-named model and texture assets.
     * @param OBJ_PATH
     *         Obj file path
     */
    public void createOnGlThread(Context context, String OBJ_PATH
    ) throws IOException {

        String parentDirectory = OBJ_PATH.split("/")[0] + "/";
        // Read the obj file.
        InputStream objInputStream = context.getAssets().open(OBJ_PATH);
        mObj = ObjReader.read(objInputStream);

        if (mObj.getNumMaterialGroups() == 0 && mObj.getMtlFileNames().size() == 0) {
            Log.e(TAG, "No mtl file defined for this model.");
            return;
        }

        // Prepare the Obj so that its structure is suitable for
        // rendering with OpenGL:
        // 1. Triangulate it
        // 2. Make sure that texture coordinates are not ambiguous
        // 3. Make sure that normals are not ambiguous
        // 4. Convert it to single-indexed data

        mObj = ObjUtils.convertToRenderable(mObj);

        vectorArrayObjectIds = new int[mObj.getNumMaterialGroups()];
        GLES30.glGenVertexArrays(mObj.getNumMaterialGroups(), vectorArrayObjectIds, 0);

        FloatBuffer vertices = ObjData.getVertices(mObj);
        FloatBuffer texCoords = ObjData.getTexCoords(mObj, 2);
        FloatBuffer normals = ObjData.getNormals(mObj);
        mTextures = new int[mObj.getNumMaterialGroups()];
        GLES20.glGenTextures(mTextures.length, mTextures, 0);

        //Iterate each material group to create a VAO
        for (int i = 0; i < mObj.getNumMaterialGroups(); i++) {
            int currentVAOId = vectorArrayObjectIds[i];
            ObjGroup currentMatGroup = mObj.getMaterialGroup(i);

            IntBuffer wideIndices = createDirectIntBuffer(currentMatGroup.getNumFaces() * 3);

            for (int j = 0; j < currentMatGroup.getNumFaces(); j++) {

                ObjFace currentFace = currentMatGroup.getFace(j);
                wideIndices.put(currentFace.getVertexIndex(0));
                wideIndices.put(currentFace.getVertexIndex(1));
                wideIndices.put(currentFace.getVertexIndex(2));

            }
            wideIndices.position(0);

            //Load texture
            if (!mObj.getMtlFileNames().isEmpty()) {
                List<Mtl> mtlList = MtlReader.read(
                        context.getAssets().open(mObj.getMtlFileNames().get(0))); // TODO: file location
                Mtl targetMat = null;
                for (Mtl mat : mtlList) {
                    if (currentMatGroup.getName().equals(mat.getName())) {
                        targetMat = mat;
                        break;
                    }
                }

                if (targetMat == null) {
                    return;
                }

                if (targetMat.getMapKd() != null && !targetMat.getMapKd().isEmpty()) {
                    // Read the texture.
                    Bitmap textureBitmap;
                    if (targetMat.getMapKd().contains("tga")) {
                        textureBitmap = readTgaToBitmap(context, targetMat.getMapKd());
                    } else {
                        textureBitmap = BitmapFactory.decodeStream(
                                context.getAssets().open(targetMat.getMapKd()));
                    }

                    GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, mTextures[i]);

                    GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D,
                            GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR_MIPMAP_LINEAR);
                    GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D,
                            GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
                    GLES20.glGenerateMipmap(GLES20.GL_TEXTURE_2D);
                    GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, 0);

                    textureBitmap.recycle();

                }
                else
                {
                    FloatTuple k = targetMat.getKd();
                    int color = Color.rgb((int) (255 * k.getX()), (int) (255 * k.getY()), (int) (255 * k.getZ()));

                    paint.setColor(color);
                    canvas.drawRect(mrect, paint);

                    GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, mTextures[i]);

                    GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D,
                            GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_LINEAR_MIPMAP_LINEAR);
                    GLES20.glTexParameteri(GLES20.GL_TEXTURE_2D,
                            GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
                    GLUtils.texImage2D(GLES20.GL_TEXTURE_2D, 0, mtextureBitmap, 0);
                    GLES20.glGenerateMipmap(GLES20.GL_TEXTURE_2D);
                    GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, 0);
                }

                ShaderUtil.checkGLError(TAG, "Texture loading");
            }

            // Convert int indices to shorts for GL ES 2.0 compatibility
            ShortBuffer indices = ByteBuffer.allocateDirect(2 * wideIndices.limit())
                    .order(ByteOrder.nativeOrder()).asShortBuffer();
            while (wideIndices.hasRemaining()) {
                indices.put((short) wideIndices.get());
            }
            indices.rewind();

            int[] buffers = new int[2];
            GLES20.glGenBuffers(2, buffers, 0);
            mVertexBufferId = buffers[0];
            mIndexBufferId = buffers[1];

            // Load vertex buffer
            mVerticesBaseAddress = 0;
            mTexCoordsBaseAddress = mVerticesBaseAddress + 4 * vertices.limit();
            mNormalsBaseAddress = mTexCoordsBaseAddress + 4 * texCoords.limit();
            final int totalBytes = mNormalsBaseAddress + 4 * normals.limit();

            //Bind VAO for this material group
            GLES30.glBindVertexArray(currentVAOId);

            //Bind VBO
            GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, mVertexBufferId);
            GLES20.glBufferData(GLES20.GL_ARRAY_BUFFER, totalBytes, null, GLES20.GL_STATIC_DRAW);
            GLES20.glBufferSubData(
                    GLES20.GL_ARRAY_BUFFER, mVerticesBaseAddress, 4 * vertices.limit(), vertices);
            GLES20.glBufferSubData(
                    GLES20.GL_ARRAY_BUFFER, mTexCoordsBaseAddress, 4 * texCoords.limit(), texCoords);
            GLES20.glBufferSubData(
                    GLES20.GL_ARRAY_BUFFER, mNormalsBaseAddress, 4 * normals.limit(), normals);

            // Bind EBO
            GLES20.glBindBuffer(GLES20.GL_ELEMENT_ARRAY_BUFFER, mIndexBufferId);
            mIndexCount = indices.limit();
            GLES20.glBufferData(
                    GLES20.GL_ELEMENT_ARRAY_BUFFER, 2 * mIndexCount, indices, GLES20.GL_STATIC_DRAW);

            ShaderUtil.checkGLError(TAG, "OBJ buffer load");

            //Compile shaders
            final int vertexShader = ShaderUtil.loadGLShader(TAG, context,
                    GLES20.GL_VERTEX_SHADER, R.raw.object_vertex);
            final int fragmentShader = ShaderUtil.loadGLShader(TAG, context,
                    GLES20.GL_FRAGMENT_SHADER, R.raw.object_fragment);

            mProgram = GLES20.glCreateProgram();
            GLES20.glAttachShader(mProgram, vertexShader);
            GLES20.glAttachShader(mProgram, fragmentShader);
            GLES20.glLinkProgram(mProgram);
            GLES20.glUseProgram(mProgram);

            ShaderUtil.checkGLError(TAG, "Program creation");

            //Get handle of vertex attributes
            mPositionAttribute = GLES20.glGetAttribLocation(mProgram, "a_Position");
            mNormalAttribute = GLES20.glGetAttribLocation(mProgram, "a_Normal");
            mTexCoordAttribute = GLES20.glGetAttribLocation(mProgram, "a_TexCoord");

            // Set the vertex attributes.
            GLES20.glVertexAttribPointer(
                    mPositionAttribute, COORDS_PER_VERTEX, GLES20.GL_FLOAT, false, 0, mVerticesBaseAddress);
            GLES20.glVertexAttribPointer(
                    mNormalAttribute, 3, GLES20.GL_FLOAT, false, 0, mNormalsBaseAddress);
            GLES20.glVertexAttribPointer(
                    mTexCoordAttribute, 2, GLES20.GL_FLOAT, false, 0, mTexCoordsBaseAddress);

            // Enable vertex arrays
            GLES20.glEnableVertexAttribArray(mPositionAttribute);
            GLES20.glEnableVertexAttribArray(mNormalAttribute);
            GLES20.glEnableVertexAttribArray(mTexCoordAttribute);

            //Unbind VAO,VBO and EBO
            GLES30.glBindVertexArray(0);
            GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, 0);
            GLES20.glBindBuffer(GLES20.GL_ELEMENT_ARRAY_BUFFER, 0);

            //Get handle of other shader inputs
            mModelViewUniform = GLES20.glGetUniformLocation(mProgram, "u_ModelView");
            mModelViewProjectionUniform =
                    GLES20.glGetUniformLocation(mProgram, "u_ModelViewProjection");

            mTextureUniform = GLES20.glGetUniformLocation(mProgram, "u_Texture");

            mLightingParametersUniform = GLES20.glGetUniformLocation(mProgram, "u_LightingParameters");
            mMaterialParametersUniform = GLES20.glGetUniformLocation(mProgram, "u_MaterialParameters");

            ShaderUtil.checkGLError(TAG, "Program parameters");

            Matrix.setIdentityM(mModelMatrix, 0);
        }

    }

    /**
     * Taken from https://github.com/JohnLXiang/arcore-sandbox
     */
    private static IntBuffer createDirectIntBuffer(int size) {
        return ByteBuffer.allocateDirect(size * 4)
                .order(ByteOrder.nativeOrder())
                .asIntBuffer();
    }

    /**
     * Taken from https://github.com/JohnLXiang/arcore-sandbox
     */
    private Bitmap readTgaToBitmap(Context context, String path) throws IOException {
        InputStream is = context.getAssets().open(path);
        byte[] buffer = new byte[is.available()];
        is.read(buffer);
        is.close();

        int[] pixels = TGAReader.read(buffer, TGAReader.ARGB);
        int width = TGAReader.getWidth(buffer);
        int height = TGAReader.getHeight(buffer);

        return Bitmap.createBitmap(pixels, 0, width, width, height,
                Bitmap.Config.ARGB_8888);
    }

    /**
     * Selects the blending mode for rendering.
     *
     * @param blendMode The blending mode.  Null indicates no blending (opaque rendering).
     */
    public void setBlendMode(BlendMode blendMode) {
        mBlendMode = blendMode;
    }

    /**
     * Updates the object model matrix and applies scaling.
     *
     * @param modelMatrix A 4x4 model-to-world transformation matrix, stored in column-major order.
     * @param scaleFactor A separate scaling factor to apply before the {@code modelMatrix}.
     * @see android.opengl.Matrix
     */
    public void updateModelMatrix(float[] modelMatrix, float scaleFactor) {
        float[] scaleMatrix = new float[16];
        Matrix.setIdentityM(scaleMatrix, 0);
        scaleMatrix[0] = scaleFactor;
        scaleMatrix[5] = scaleFactor;
        scaleMatrix[10] = scaleFactor;
        Matrix.multiplyMM(mModelMatrix, 0, modelMatrix, 0, scaleMatrix, 0);
    }

    /**
     * Sets the surface characteristics of the rendered model.
     *
     * @param ambient  Intensity of non-directional surface illumination.
     * @param diffuse  Diffuse (matte) surface reflectivity.
     * @param specular  Specular (shiny) surface reflectivity.
     * @param specularPower  Surface shininess.  Larger values result in a smaller, sharper
     *     specular highlight.
     */
    public void setMaterialProperties(
            float ambient, float diffuse, float specular, float specularPower) {
        mAmbient = ambient;
        mDiffuse = diffuse;
        mSpecular = specular;
        mSpecularPower = specularPower;
    }

    /**
     * Draws the model.
     *
     * @param cameraView  A 4x4 view matrix, in column-major order.
     * @param cameraPerspective  A 4x4 projection matrix, in column-major order.
     * @param lightIntensity  Illumination intensity.  Combined with diffuse and specular material
     *     properties.
     * @see #setBlendMode(BlendMode)
     * @see #updateModelMatrix(float[], float)
     * @see #setMaterialProperties(float, float, float, float)
     * @see android.opengl.Matrix
     */
    public void draw(float[] cameraView, float[] cameraPerspective, float lightIntensity) {

        ShaderUtil.checkGLError(TAG, "Before draw");

        // Build the ModelView and ModelViewProjection matrices
        // for calculating object position and light.
        Matrix.multiplyMM(mModelViewMatrix, 0, cameraView, 0, mModelMatrix, 0);
        Matrix.multiplyMM(mModelViewProjectionMatrix, 0, cameraPerspective, 0, mModelViewMatrix, 0);

        GLES20.glUseProgram(mProgram);

        // Set the lighting environment properties.
        Matrix.multiplyMV(mViewLightDirection, 0, mModelViewMatrix, 0, LIGHT_DIRECTION, 0);
        normalizeVec3(mViewLightDirection);
        GLES20.glUniform4f(mLightingParametersUniform,
            mViewLightDirection[0], mViewLightDirection[1], mViewLightDirection[2], lightIntensity);

        // Set the object material properties.
        GLES20.glUniform4f(mMaterialParametersUniform, mAmbient, mDiffuse, mSpecular,
            mSpecularPower);

        // Attach the object texture.
        GLES20.glActiveTexture(GLES20.GL_TEXTURE0);
        GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, mTextures[0]);
        GLES20.glUniform1i(mTextureUniform, 0);

        // Set the vertex attributes.
        GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, mVertexBufferId);

        GLES20.glVertexAttribPointer(
            mPositionAttribute, COORDS_PER_VERTEX, GLES20.GL_FLOAT, false, 0, mVerticesBaseAddress);
        GLES20.glVertexAttribPointer(
            mNormalAttribute, 3, GLES20.GL_FLOAT, false, 0, mNormalsBaseAddress);
        GLES20.glVertexAttribPointer(
            mTexCoordAttribute, 2, GLES20.GL_FLOAT, false, 0, mTexCoordsBaseAddress);

        GLES20.glBindBuffer(GLES20.GL_ARRAY_BUFFER, 0);

        // Set the ModelViewProjection matrix in the shader.
        GLES20.glUniformMatrix4fv(
            mModelViewUniform, 1, false, mModelViewMatrix, 0);
        GLES20.glUniformMatrix4fv(
            mModelViewProjectionUniform, 1, false, mModelViewProjectionMatrix, 0);

        // Enable vertex arrays
        GLES20.glEnableVertexAttribArray(mPositionAttribute);
        GLES20.glEnableVertexAttribArray(mNormalAttribute);
        GLES20.glEnableVertexAttribArray(mTexCoordAttribute);

        if (mBlendMode != null) {
            GLES20.glDepthMask(false);
            GLES20.glEnable(GLES20.GL_BLEND);
            switch (mBlendMode) {
                case Shadow:
                    // Multiplicative blending function for Shadow.
                    GLES20.glBlendFunc(GLES20.GL_ZERO, GLES20.GL_ONE_MINUS_SRC_ALPHA);
                    break;
                case Grid:
                    // Grid, additive blending function.
                    GLES20.glBlendFunc(GLES20.GL_SRC_ALPHA, GLES20.GL_ONE_MINUS_SRC_ALPHA);
                    break;
            }
        }

        //Start drawing data from each VAO
        for (int i = 0; i < mObj.getNumMaterialGroups(); i++) {
            // Attach the object texture.
            GLES20.glUniform1i(mTextureUniform, 0);
            GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, mTextures[i]);

            GLES30.glBindVertexArray(vectorArrayObjectIds[i]);
            GLES20.glDrawElements(GLES20.GL_TRIANGLES, mObj.getMaterialGroup(i).getNumFaces() * 3,
                    GLES20.GL_UNSIGNED_SHORT, 0);
            GLES30.glBindVertexArray(0);

            //Unbind texture
            GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, 0);
        }

        //GLES20.glBindBuffer(GLES20.GL_ELEMENT_ARRAY_BUFFER, mIndexBufferId);
        //GLES20.glDrawElements(GLES20.GL_TRIANGLES, mIndexCount, GLES20.GL_UNSIGNED_SHORT, 0);
       // GLES20.glBindBuffer(GLES20.GL_ELEMENT_ARRAY_BUFFER, 0);

        if (mBlendMode != null) {
            GLES20.glDisable(GLES20.GL_BLEND);
            GLES20.glDepthMask(true);
        }

        // Disable vertex arrays
       // GLES20.glDisableVertexAttribArray(mPositionAttribute);
        //GLES20.glDisableVertexAttribArray(mNormalAttribute);
       // GLES20.glDisableVertexAttribArray(mTexCoordAttribute);

        //GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, 0);

        ShaderUtil.checkGLError(TAG, "After draw");
    }

    private static void normalizeVec3(float[] v) {
        float reciprocalLength = 1.0f / (float) Math.sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        v[0] *= reciprocalLength;
        v[1] *= reciprocalLength;
        v[2] *= reciprocalLength;
    }
}
