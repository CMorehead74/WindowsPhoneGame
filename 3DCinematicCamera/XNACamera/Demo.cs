//-----------------------------------------------------------------------------
// Copyright (c) 2007-2011 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

using System;
using Dhpoware;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNACamera
{
    /// <summary>
    /// This Windows XNA application demonstrates how various camera behaviors
    /// can be implemented. The following camera behaviors are implemented by
    /// this demo: first person, spectator, flight, and orbit. The first person
    /// camera behavior simulates the view from the perspective of the player.
    /// The spectator camera behavior simulates a floating camera that moves in
    /// the direction that the camera is looking. The flight camera behavior
    /// simulates the view from the cockpit of an airplane. Finally the orbit
    /// camera behavior simulates a third person view of the player and allows
    /// the camera to orbit the player.
    ///
    /// <para>All of the camera logic is contained in the camera.cs file. This
    /// file contains an interface (ICamera) and 2 implementations of this
    /// interface (Camera and CameraComponent).</para>
    /// 
    /// <para>The Camera class contains the implementation of the ICamera
    /// interface and is designed to be directly manipulated by other classes
    /// in your application. The Camera class does not contain any kind of
    /// logic to process player input. You should look at the Camera class to
    /// understand the math behind the various camera behaviors.</para>
    /// 
    /// <para>The CameraComponent class is an XNA game component. It's designed
    /// to be plugged straight into an existing XNA application and
    /// automatically provides the application with an interactive in-game
    /// camera. The CameraComponent class processes player input and
    /// manipulates the camera based on that input. The CameraComponent class
    /// contains an instance of the Camera class. The CameraComponent interacts
    /// with the XNA framework and then processes player input by delegating
    /// all camera logic to the Camera class.</para>
    /// </summary>
    public class Demo : Microsoft.Xna.Framework.Game
    {
        private static void Main()
        {
            using (Demo demo = new Demo())
            {
                demo.Run();
            }
        }

        /// <summary>
        /// A light. This light structure is the same as the one defined in
        /// the parallax_normal_mapping.fx file. The only difference is the
        /// LightType enum.
        /// </summary>
        private struct Light
        {
            public enum LightType
            {
                DirectionalLight,
                PointLight,
                SpotLight
            }

            public LightType Type;
            public Vector3 Direction;
            public Vector3 Position;
            public Color Ambient;
            public Color Diffuse;
            public Color Specular;
            public float SpotInnerConeRadians;
            public float SpotOuterConeRadians;
            public float Radius;
        }

        /// <summary>
        /// A material. This material structure is the same as the one defined
        /// in the parallax_normal_mapping.fx file. We use the Color type here
        /// instead of a four element floating point array.
        /// </summary>
        private struct Material
        {
            public Color Ambient;
            public Color Diffuse;
            public Color Emissive;
            public Color Specular;
            public float Shininess;
        }

        private const float FLOOR_WIDTH = 8.0f;
        private const float FLOOR_HEIGHT = 8.0f;
        private const float FLOOR_TILE_U = 8.0f;
        private const float FLOOR_TILE_V = 8.0f;
        private const float CAMERA_FOV = 90.0f;
        private const float CAMERA_ZNEAR = 0.01f;
        private const float CAMERA_ZFAR = 100.0f;
        private const float CAMERA_OFFSET = 0.5f;
        private const float CAMERA_BOUNDS_MIN_X = -FLOOR_WIDTH / 2.0f;
        private const float CAMERA_BOUNDS_MAX_X = FLOOR_WIDTH / 2.0f;
        private const float CAMERA_BOUNDS_MIN_Y = CAMERA_OFFSET;
        private const float CAMERA_BOUNDS_MAX_Y = 4.0f;
        private const float CAMERA_BOUNDS_MIN_Z = -FLOOR_HEIGHT / 2.0f;
        private const float CAMERA_BOUNDS_MAX_Z = FLOOR_HEIGHT / 2.0f;

        private CameraComponent camera;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private Effect effect;
        private Texture2D nullTexture;
        private Texture2D floorColorMap;
        private Texture2D floorNormalMap;
        private Texture2D floorHeightMap;
        private VertexBuffer floorVertexBuffer;
        private Vector2 scaleBias;
        private Vector2 fontPos;
        private Light light;
        private Material material;
        private Model model;
        private Quaternion modelOrientation;
        private Vector3 modelPosition;
        private Matrix[] modelTransforms;
        private KeyboardState currentKeyboardState;
        private KeyboardState prevKeyboardState;
        private int windowWidth;
        private int windowHeight;
        private int frames;
        private int framesPerSecond;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private bool displayHelp;
        private bool disableColorMap;
        private bool disableParallax;

        public Demo()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            camera = new CameraComponent(this);
            Components.Add(camera);

            Window.Title = "XNA 4.0 Camera Demo";
            IsFixedTimeStep = false;
            IsMouseVisible = false;
        }

        private void ChangeCameraBehavior(Camera.Behavior behavior)
        {
            if (camera.CurrentBehavior == behavior)
                return;

            if (behavior == Camera.Behavior.Orbit)
            {
                modelPosition = camera.Position;
                modelOrientation = Quaternion.Inverse(camera.Orientation);
            }

            camera.CurrentBehavior = behavior;

            // Position the camera behind and 30 degrees above the target.
            if (behavior == Camera.Behavior.Orbit)
                camera.Rotate(0.0f, -30.0f, 0.0f);
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Setup the window to be a quarter the size of the desktop.
            windowWidth = GraphicsDevice.DisplayMode.Width / 2;
            windowHeight = GraphicsDevice.DisplayMode.Height / 2;

            // Setup frame buffer.
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();
            
            // Setup the floor.
            InitFloor();

            // Setup the camera.
            InitCamera();

            // Initial position for text rendering.
            fontPos = new Vector2(1.0f, 1.0f);

            // Initialize light settings.
            light.Type = Light.LightType.SpotLight;
            light.Direction = Vector3.Down;
            light.Radius = Math.Max(FLOOR_WIDTH, FLOOR_HEIGHT);
            light.Position = new Vector3(0.0f, light.Radius * 0.5f, 0.0f);
            light.Ambient = Color.White;
            light.Diffuse = Color.White;
            light.Specular = Color.White;
            light.SpotInnerConeRadians = MathHelper.ToRadians(30.0f);
            light.SpotOuterConeRadians = MathHelper.ToRadians(100.0f);

            // Initialize material settings for the floor.
            material.Ambient = new Color(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            material.Diffuse = new Color(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            material.Emissive = Color.Black;
            material.Specular = Color.White;
            material.Shininess = 90.0f;

            // Parallax mapping height scale and bias values.
            scaleBias = new Vector2(0.04f, -0.03f);

            // Setup the initial input states.
            currentKeyboardState = Keyboard.GetState();
        }

        private void InitCamera()
        {
            GraphicsDevice device = graphics.GraphicsDevice;
            float aspectRatio = (float)windowWidth / (float)windowHeight;

            camera.Perspective(CAMERA_FOV, aspectRatio, CAMERA_ZNEAR, CAMERA_ZFAR);
            camera.Position = new Vector3(0.0f, CAMERA_OFFSET, 0.0f);
            camera.Acceleration = new Vector3(4.0f, 4.0f, 4.0f);
            camera.Velocity = new Vector3(1.0f, 1.0f, 1.0f);
            camera.OrbitMinZoom = 1.5f;
            camera.OrbitMaxZoom = 5.0f;
            camera.OrbitOffsetDistance = camera.OrbitMinZoom;

            ChangeCameraBehavior(Camera.Behavior.Orbit);
        }

        private void InitFloor()
        {
            NormalMappedQuad floor = new NormalMappedQuad(
                Vector3.Zero, Vector3.Up, Vector3.Forward,
                FLOOR_WIDTH, FLOOR_HEIGHT, FLOOR_TILE_U, FLOOR_TILE_V);

            floorVertexBuffer = new VertexBuffer(GraphicsDevice,
                typeof(NormalMappedVertex), floor.Vertices.Length,
                BufferUsage.WriteOnly);

            floorVertexBuffer.SetData(floor.Vertices);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the sprite font. The sprite font has a 3 pixel outer glow
            // baked into it so we need to decrease the spacing so that the
            // SpriteFont will render correctly.
            spriteFont = Content.Load<SpriteFont>(@"Fonts\DemoFont12pt");
            spriteFont.Spacing = -4.0f;

            model = Content.Load<Model>(@"Models\bigship1");

            effect = Content.Load<Effect>(@"Effects\parallax_normal_mapping");

            floorColorMap = Content.Load<Texture2D>(@"Textures\floor_color_map");
            floorNormalMap = Content.Load<Texture2D>(@"Textures\floor_normal_map");
            floorHeightMap = Content.Load<Texture2D>(@"Textures\floor_height_map");

            // Create an empty white texture. This will be bound to the
            // colorMapTexture shader parameter when the user wants to
            // disable the color map texture. This trick will allow the
            // same shader to be used for when textures are enabled and
            // disabled.

            nullTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);

            Color[] pixels = { Color.White };

            nullTexture.SetData(pixels);
        }

        protected override void UnloadContent()
        {
        }

        private void UpdateEffect()
        {
            // Set shader matrix parameters.
            effect.Parameters["worldMatrix"].SetValue(Matrix.Identity);
            effect.Parameters["worldInverseTransposeMatrix"].SetValue(Matrix.Identity);
            effect.Parameters["worldViewProjectionMatrix"].SetValue(camera.ViewProjectionMatrix);

            // Set the shader camera position parameter.
            effect.Parameters["cameraPos"].SetValue(camera.Position);

            // Set the shader global ambiance parameters.
            effect.Parameters["globalAmbient"].SetValue(0.0f);

            // Set the shader parallax scale and bias parameter.
            effect.Parameters["scaleBias"].SetValue(scaleBias);

            // Set the shader lighting parameters.
            effect.Parameters["light"].StructureMembers["dir"].SetValue(light.Direction);
            effect.Parameters["light"].StructureMembers["pos"].SetValue(light.Position);
            effect.Parameters["light"].StructureMembers["ambient"].SetValue(light.Ambient.ToVector4());
            effect.Parameters["light"].StructureMembers["diffuse"].SetValue(light.Diffuse.ToVector4());
            effect.Parameters["light"].StructureMembers["specular"].SetValue(light.Specular.ToVector4());
            effect.Parameters["light"].StructureMembers["spotInnerCone"].SetValue(light.SpotInnerConeRadians);
            effect.Parameters["light"].StructureMembers["spotOuterCone"].SetValue(light.SpotOuterConeRadians);
            effect.Parameters["light"].StructureMembers["radius"].SetValue(light.Radius);

            // Set the shader material parameters.
            effect.Parameters["material"].StructureMembers["ambient"].SetValue(material.Ambient.ToVector4());
            effect.Parameters["material"].StructureMembers["diffuse"].SetValue(material.Diffuse.ToVector4());
            effect.Parameters["material"].StructureMembers["emissive"].SetValue(material.Emissive.ToVector4());
            effect.Parameters["material"].StructureMembers["specular"].SetValue(material.Specular.ToVector4());
            effect.Parameters["material"].StructureMembers["shininess"].SetValue(material.Shininess);

            // Bind the texture maps to the shader.
            effect.Parameters["colorMapTexture"].SetValue((disableColorMap) ? nullTexture : floorColorMap);
            effect.Parameters["normalMapTexture"].SetValue(floorNormalMap);
            effect.Parameters["heightMapTexture"].SetValue(floorHeightMap);

            // Select the shader based on light type.
            switch (light.Type)
            {
            case Light.LightType.DirectionalLight:
                if (disableParallax)
                    effect.CurrentTechnique = effect.Techniques["NormalMappingDirectionalLighting"];
                else
                    effect.CurrentTechnique = effect.Techniques["ParallaxNormalMappingDirectionalLighting"];
                break;

            case Light.LightType.PointLight:
                if (disableParallax)
                    effect.CurrentTechnique = effect.Techniques["NormalMappingPointLighting"];
                else
                    effect.CurrentTechnique = effect.Techniques["ParallaxNormalMappingPointLighting"];
                break;

            case Light.LightType.SpotLight:
                if (disableParallax)
                    effect.CurrentTechnique = effect.Techniques["NormalMappingSpotLighting"];
                else
                    effect.CurrentTechnique = effect.Techniques["ParallaxNormalMappingSpotLighting"];
                break;

            default:
                break;
            }
        }

        private void PerformCameraCollisionDetection()
        {
            if (camera.CurrentBehavior != Camera.Behavior.Orbit)
            {
                Vector3 newPos = camera.Position;

                if (camera.Position.X > CAMERA_BOUNDS_MAX_X)
                    newPos.X = CAMERA_BOUNDS_MAX_X;

                if (camera.Position.X < CAMERA_BOUNDS_MIN_X)
                    newPos.X = CAMERA_BOUNDS_MIN_X;

                if (camera.Position.Y > CAMERA_BOUNDS_MAX_Y)
                    newPos.Y = CAMERA_BOUNDS_MAX_Y;

                if (camera.Position.Y < CAMERA_BOUNDS_MIN_Y)
                    newPos.Y = CAMERA_BOUNDS_MIN_Y;

                if (camera.Position.Z > CAMERA_BOUNDS_MAX_Z)
                    newPos.Z = CAMERA_BOUNDS_MAX_Z;

                if (camera.Position.Z < CAMERA_BOUNDS_MIN_Z)
                    newPos.Z = CAMERA_BOUNDS_MIN_Z;

                camera.Position = newPos;
            }
        }

        private void ToggleFullScreen()
        {
            int newWidth = 0;
            int newHeight = 0;

            graphics.IsFullScreen = !graphics.IsFullScreen;

            if (graphics.IsFullScreen)
            {
                newWidth = GraphicsDevice.DisplayMode.Width;
                newHeight = GraphicsDevice.DisplayMode.Height;
            }
            else
            {
                newWidth = windowWidth;
                newHeight = windowHeight;
            }

            graphics.PreferredBackBufferWidth = newWidth;
            graphics.PreferredBackBufferHeight = newHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            float aspectRatio = (float)newWidth / (float)newHeight;

            camera.Perspective(CAMERA_FOV, aspectRatio, CAMERA_ZNEAR, CAMERA_ZFAR);
        }

        private bool KeyJustPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key);
        }

        private void ProcessInput()
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (KeyJustPressed(Keys.Escape))
                this.Exit();

            if (KeyJustPressed(Keys.Back))
            {
                switch (camera.CurrentBehavior)
                {
                case Camera.Behavior.Flight:
                    camera.UndoRoll();
                    break;

                case Camera.Behavior.Orbit:
                    if (!camera.PreferTargetYAxisOrbiting)
                        camera.UndoRoll();
                    break;

                default:
                    break;
                }
            }

            if (KeyJustPressed(Keys.Space))
            {
                if (camera.CurrentBehavior == Camera.Behavior.Orbit)
                    camera.PreferTargetYAxisOrbiting = !camera.PreferTargetYAxisOrbiting;
            }

            if (KeyJustPressed(Keys.D1))
                ChangeCameraBehavior(Camera.Behavior.FirstPerson);

            if (KeyJustPressed(Keys.D2))
                ChangeCameraBehavior(Camera.Behavior.Spectator);

            if (KeyJustPressed(Keys.D3))
                ChangeCameraBehavior(Camera.Behavior.Flight);

            if (KeyJustPressed(Keys.D4))
                ChangeCameraBehavior(Camera.Behavior.Orbit);

            if (KeyJustPressed(Keys.D5))
            {
                timer = 0.0f;
                ChangeCameraBehavior(Camera.Behavior.Cinematic);
            }

            if (KeyJustPressed(Keys.D9))
            {
                keyFrame1 = new KeyFrame(camera.Position, camera.Orientation, 0.0f);
            }

            if (KeyJustPressed(Keys.D0))
            {
                keyFrame2 = new KeyFrame(camera.Position, camera.Orientation, 5.0f);

            }

            if (KeyJustPressed(Keys.H))
                displayHelp = !displayHelp;

            if (KeyJustPressed(Keys.P))
                disableParallax = !disableParallax;

            if (KeyJustPressed(Keys.T))
                disableColorMap = !disableColorMap;

            if (KeyJustPressed(Keys.C))
                camera.ClickAndDragMouseRotation = !camera.ClickAndDragMouseRotation;

            if (KeyJustPressed(Keys.Add))
            {
                camera.RotationSpeed += 0.01f;

                if (camera.RotationSpeed > 1.0f)
                    camera.RotationSpeed = 1.0f;
            }

            if (KeyJustPressed(Keys.Subtract))
            {
                camera.RotationSpeed -= 0.01f;

                if (camera.RotationSpeed <= 0.0f)
                    camera.RotationSpeed = 0.01f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.LeftAlt) ||
                currentKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    ToggleFullScreen();
            }
        }

        float timer = 0.0f;
        Matrix result;
        KeyFrame keyFrame1 = new KeyFrame(new Vector3(0, 1, 0), Quaternion.Identity, 0.0f);
        KeyFrame keyFrame2 = new KeyFrame(new Vector3(2, 2, 3), Quaternion.CreateFromYawPitchRoll(3.14f, 0.0f, 0.0f), 5.0f);

        private void UpdateCinematic(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            timer = Math.Min(timer, 5.0f);
            
            result = KeyFrame.Interpolate(keyFrame1, keyFrame2, timer);
        }

        protected override void Update(GameTime gameTime)
        {
            if (!this.IsActive)
                return;

            base.Update(gameTime);

            ProcessInput();

            if (camera.CurrentBehavior == Camera.Behavior.Cinematic)
                UpdateCinematic(gameTime);

            PerformCameraCollisionDetection();
            UpdateEffect();
            UpdateFrameRate(gameTime);
        }

        private void UpdateFrameRate(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                framesPerSecond = frames;
                frames = 0;
            }
        }

        private void IncrementFrameCounter()
        {
            ++frames;
        }

        private void DrawModel(Vector3 modelPosition)
        {
            if (modelTransforms == null)
            {
                modelTransforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            }

            Matrix world = Matrix.CreateFromQuaternion(modelOrientation);

            world.M41 = modelPosition.X;
            world.M42 = modelPosition.Y;
            world.M43 = modelPosition.Z;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.PreferPerPixelLighting = true;
                    effect.EnableDefaultLighting();
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                    effect.World = modelTransforms[mesh.ParentBone.Index] * world;
                }

                mesh.Draw();
            }
        }
        private void DrawModelOriented(Matrix world)
        {
            if (modelTransforms == null)
            {
                modelTransforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            }

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.PreferPerPixelLighting = true;
                    effect.EnableDefaultLighting();
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                    effect.World = modelTransforms[mesh.ParentBone.Index] * world;
                }

                mesh.Draw();
            }
        }
        private void DrawFloor()
        {
            GraphicsDevice.SetVertexBuffer(floorVertexBuffer);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        private void DrawText()
        {
            string text = null;

            if (displayHelp)
            {
                System.Text.StringBuilder buffer = new System.Text.StringBuilder();

                buffer.AppendLine("Press 1 to switch to first person behavior");
                buffer.AppendLine("Press 2 to switch to spectator behavior");
                buffer.AppendLine("Press 3 to switch to flight behavior");
                buffer.AppendLine("Press 4 to switch to orbit behavior");
                buffer.AppendLine();
                buffer.AppendLine("First Person and Spectator behaviors:");
                buffer.AppendLine("  Press W and S to move forwards and backwards");
                buffer.AppendLine("  Press A and D to strafe left and right");
                buffer.AppendLine("  Press E and Q to move up and down");
                buffer.AppendLine("  Move mouse to free look");
                buffer.AppendLine();
                buffer.AppendLine("Flight behavior:");
                buffer.AppendLine("  Press W and S to move forwards and backwards");
                buffer.AppendLine("  Press A and D to yaw left and right");
                buffer.AppendLine("  Press E and Q to move up and down");
                buffer.AppendLine("  Move mouse up and down to change pitch");
                buffer.AppendLine("  Move mouse left and right to change roll");
                buffer.AppendLine();
                buffer.AppendLine("Orbit behavior:");
                buffer.AppendLine("  Press SPACE to enable/disable target Y axis orbiting");
                buffer.AppendLine("  Move mouse to orbit the model");
                buffer.AppendLine("  Mouse wheel to zoom in and out");
                buffer.AppendLine();
                buffer.AppendLine("Press T to enable/disable floor textures");
                buffer.AppendLine("Press P to toggle between parallax normal mapping and normal mapping");
                buffer.AppendLine("Press C to toggle mouse click-and-drag camera rotation");
                buffer.AppendLine("Press BACKSPACE to level camera");
                buffer.AppendLine("Press NUMPAD +/- to change camera rotation speed");
                buffer.AppendLine("Press ALT + ENTER to toggle full screen");
                buffer.AppendLine("Press H to hide help");

                text = buffer.ToString();
            }
            else
            {
                System.Text.StringBuilder buffer = new System.Text.StringBuilder();

                buffer.AppendFormat("FPS: {0}\n", framesPerSecond);
                buffer.AppendFormat("Floor Technique: {0}\n\n",
                    (disableParallax ? "Normal mapping" : "Parallax normal mapping"));
                buffer.Append("Camera:\n");
                buffer.AppendFormat("  Behavior: {0}\n", camera.CurrentBehavior);
                buffer.AppendFormat("  Position: x:{0} y:{1} z:{2}\n",
                    camera.Position.X.ToString("#0.00"),
                    camera.Position.Y.ToString("#0.00"),
                    camera.Position.Z.ToString("#0.00"));
                buffer.AppendFormat("  Velocity: x:{0} y:{1} z:{2}\n",
                    camera.CurrentVelocity.X.ToString("#0.00"),
                    camera.CurrentVelocity.Y.ToString("#0.00"),
                    camera.CurrentVelocity.Z.ToString("#0.00"));
                buffer.AppendFormat("  Rotation speed: {0}\n",
                    camera.RotationSpeed.ToString("#0.00"));

                if (camera.PreferTargetYAxisOrbiting)
                    buffer.Append("  Target Y axis orbiting\n\n");
                else
                    buffer.Append("  Free orbiting\n\n");

                buffer.Append("Press H to display help");

                text = buffer.ToString();
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawString(spriteFont, text, fontPos, Color.Yellow);
            spriteBatch.End();
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!this.IsActive)
                return;

            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            if (camera.CurrentBehavior == Camera.Behavior.Orbit)
                DrawModel(modelPosition);
            else //if (camera.CurrentBehavior == Camera.Behavior.Cinematic)
                DrawModelOriented(result);

            DrawFloor();
            DrawText();

            base.Draw(gameTime);
            IncrementFrameCounter();
        }
    }
}
