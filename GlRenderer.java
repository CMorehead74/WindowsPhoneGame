package com.example.riseofthedefender;

import java.util.ArrayList;

import javax.microedition.khronos.egl.EGLConfig;
import javax.microedition.khronos.opengles.GL10;

import android.content.Context;
import android.opengl.GLSurfaceView.Renderer;
import android.opengl.GLU;

public class GlRenderer implements Renderer{

	private Context context;
	private ArrayList<Square> squares;
	
	public GlRenderer(Context context) {
		this.context = context;
		squares = new ArrayList<Square>();
		for(int i = 0; i < 300; ++i)
			squares.add(new Square());
		for(int i = 0; i < squares.size(); ++i)
			squares.get(i).x = (float) (Math.random() * 30) - 15;
		for(int i = 0; i < squares.size(); ++i)
			squares.get(i).y = (float) (Math.random() * 20) - 10;
		

	}

	public void onDrawFrame(GL10 gl) {
		gl.glClear(GL10.GL_COLOR_BUFFER_BIT | GL10.GL_DEPTH_BUFFER_BIT);
		gl.glEnableClientState(GL10.GL_VERTEX_ARRAY);
		gl.glEnableClientState(GL10.GL_TEXTURE_COORD_ARRAY);

		for(int i = 0; i < squares.size(); ++i)
			squares.get(i).update();

		for(int i = 0; i < squares.size(); ++i)
			squares.get(i).draw(gl);

		gl.glDisableClientState(GL10.GL_VERTEX_ARRAY);
		gl.glDisableClientState(GL10.GL_TEXTURE_COORD_ARRAY);
	}

	public void onSurfaceChanged(GL10 gl, int width, int height) {
		if (height == 0) { // Prevent A Divide By Zero By
			height = 1; // Making Height Equal One
		}

		gl.glViewport(0, 0, width, height); // Reset The Current Viewport
		gl.glMatrixMode(GL10.GL_PROJECTION); // Select The Projection Matrix
		gl.glLoadIdentity(); // Reset The Projection Matrix

		// Calculate The Aspect Ratio Of The Window
		GLU.gluPerspective(gl, 45.0f, (float) width / (float) height, 0.01f,200.0f);

		gl.glMatrixMode(GL10.GL_MODELVIEW); // Select The Modelview Matrix
		gl.glLoadIdentity(); // Reset The Modelview Matrix
	}

	public void onSurfaceCreated(GL10 gl, EGLConfig config) {
		TextureManager.LoadEverything(gl, context);
		gl.glEnable(GL10.GL_TEXTURE_2D); // Enable Texture Mapping ( NEW )
		gl.glShadeModel(GL10.GL_SMOOTH); // Enable Smooth Shading
		gl.glClearColor(100.0f, 0.0f, 0.0f, 255f); // Black Background
		gl.glClearDepthf(1.0f); // Depth Buffer Setup
		//gl.glEnable(GL10.GL_DEPTH_TEST); // Enables Depth Testing
		//gl.glEnable(GL10.GL_ALPHA_TEST); // Enables Depth Testing
		gl.glDepthFunc(GL10.GL_LESS); // The Type Of Depth Testing To Do
		//gl.glAlphaFunc(GL10.GL_GREATER, 0.4f);
		gl.glEnable(GL10.GL_DITHER);
		gl.glEnable(GL10.GL_BLEND);
		gl.glBlendFunc(GL10.GL_SRC_ALPHA, GL10.GL_ONE_MINUS_SRC_ALPHA);
		// gl.glBlendFunc(GL10.GL_SRC_ALPHA, GL10.GL_ONE_MINUS_SRC_ALPHA);
		gl.glEnable(GL10.GL_ALPHA_BITS);
		// Really Nice Perspective Calculations
		gl.glHint(GL10.GL_PERSPECTIVE_CORRECTION_HINT, GL10.GL_NICEST);
		
		gl.glFrontFace(GL10.GL_CW);
		gl.glVertexPointer(3, GL10.GL_FLOAT, 0, Square.vertexBuffer);
		gl.glTexCoordPointer(2, GL10.GL_FLOAT, 0, Square.textureBuffer);
	}

}
