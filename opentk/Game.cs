using System;
using System.Diagnostics;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace opentk
{
    struct Handle
    {
        public int Identifier;

        public Handle(int id)
        {
            Identifier = id;
        }
    }

    class Game : IDisposable
    {
        private GameWindow GameWindow;

        Handle VertexShaderHandle,
                FragmentShaderHandle,
                ShaderProgramHandle,
                ModelviewMatrixLocation,
                ProjectionMatrixLocation,
                VaoHandle,
                PositionVboHandle,
                NormalVboHandle,
                EboHandle;

        private float AspectRatio;
        private Matrix4 ProjectionMatrix;
        private Matrix4 ModelViewMatrix;

        private Vector3[] Cube = 
        {
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f), 
            new Vector3( 1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f)
        };

        //These are the indices of each vertex in the Cube[] variable which define the triangles.
        private uint[] ElementIndices =
        {
            //front face
            0, 1, 2, 
            2, 3, 0,
            //top face
            3, 2, 6, 
            6, 7, 3,
            //back face
            7, 6, 5, 
            5, 4, 7,
            //left face
            4, 0, 3, 
            3, 7, 4,
            //bottom face
            0, 1, 5,
            5, 4, 0,
            //right face
            1, 5, 6, 
            6, 2, 1
        };

        public Game(int width = 1680, int height = 1050)
        {
            GameWindow = new GameWindow(width, height);
            AspectRatio = width / (float)height;
            GameWindow.Load += (sender, e) => Load();
            GameWindow.Resize += (sender, e) => Resize();
            GameWindow.RenderFrame += (sender, args) => Draw();
            GameWindow.UpdateFrame += (sender, args) =>
            {
                Matrix4 rotation = Matrix4.CreateRotationY((float)args.Time * Mouse.GetCursorState().X/width * 2);
                Matrix4.Mult(ref rotation, ref ModelViewMatrix, out ModelViewMatrix);
                GL.UniformMatrix4(ModelviewMatrixLocation.Identifier, false, ref ModelViewMatrix);
            };
        }
        public void Run()
        {
            GameWindow.Run(60.0);
        }

        private void Load()
        {
            GameWindow.VSync = VSyncMode.On;
            InitializeShaders();
            SetupProjectionMatrices();
            CreateVBOs();
            CreateVAOs();

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(System.Drawing.Color.White);
        }

        private void InitializeShaders()
        {
            //Obtain handles for both the fragment and vertex shader
            VertexShaderHandle.Identifier = GL.CreateShader(ShaderType.VertexShader);
            FragmentShaderHandle.Identifier = GL.CreateShader(ShaderType.FragmentShader);

            //Tell OpenGL where to obtain the code for the shaders
            GL.ShaderSource(VertexShaderHandle.Identifier, File.ReadAllText("vertex.glsl"));
            GL.ShaderSource(FragmentShaderHandle.Identifier, File.ReadAllText("fragment.glsl"));

            //Now compile these sources and check if there were any compilation errors
            GL.CompileShader(VertexShaderHandle.Identifier);
            GL.CompileShader(FragmentShaderHandle.Identifier);

            Debug.WriteLine(GL.GetShaderInfoLog(VertexShaderHandle.Identifier));
            Debug.WriteLine(GL.GetShaderInfoLog(FragmentShaderHandle.Identifier));

            //Create a GL program (?)
            ShaderProgramHandle.Identifier = GL.CreateProgram();

            //Now attach the shaders to the program
            GL.AttachShader(ShaderProgramHandle.Identifier, VertexShaderHandle.Identifier);
            GL.AttachShader(ShaderProgramHandle.Identifier, FragmentShaderHandle.Identifier);

            //Bind the attributes of each shader (that is, the non-uniform inputs)
            GL.BindAttribLocation(ShaderProgramHandle.Identifier, 0, "in_position");

            //Link the program (?)
            GL.LinkProgram(ShaderProgramHandle.Identifier);
            //Check debug output of program
            Debug.WriteLine(GL.GetProgramInfoLog(ShaderProgramHandle.Identifier));
            //Set the program as the active program
            GL.UseProgram(ShaderProgramHandle.Identifier);
        }

        private void SetupProjectionMatrices()
        {
            //Get a handle to the location of the uniform variable 'projection_matrix'
            ProjectionMatrixLocation.Identifier = GL.GetUniformLocation(ShaderProgramHandle.Identifier, "projection_matrix");
            ModelviewMatrixLocation.Identifier = GL.GetUniformLocation(ShaderProgramHandle.Identifier, "modelview_matrix");

            //Set up the perspective field of view matrix, and set it in the ProjectionMatrix variable
            Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, AspectRatio, 1, 100, out ProjectionMatrix);

            //Set up the model view matrix, which is kinda like a camera.
            ModelViewMatrix = Matrix4.LookAt(new Vector3(0, 3, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

            //Now tell OpenGL which matrices to use for each that we just got an identifier from.
            GL.UniformMatrix4(ProjectionMatrixLocation.Identifier, false, ref ProjectionMatrix);
            GL.UniformMatrix4(ModelviewMatrixLocation.Identifier, false, ref ModelViewMatrix);
        }

        private void CreateVBOs()
        {
            //Get a handle to an OpenGL buffer
            PositionVboHandle.Identifier = GL.GenBuffer();

            //Tell OpenGL what kind of buffer we're using for that handle.
            //This is an array buffer, which can be used to store vertices and such, along with their additional attributes
            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionVboHandle.Identifier);

            //Fill up the buffer with our vertices! Whoop-di-doo
            //First tell OpenGL what kind of buffer. Then tell OpenGL how much bytes the buffer should be able to hold, with an IntPtr
            //Also indicate to OpenGL which vertices we wish to use and how often this data will be edited/refreshed.
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(Cube.Length * Vector3.SizeInBytes), Cube, BufferUsageHint.StaticDraw);

            //Some vertices in our cube are shared. To that end, there is indexing
            //We first obtain a handle to the buffer containing the indices.
            EboHandle.Identifier = GL.GenBuffer();
            
            //The buffer can now be bound: tell OpenGL the purpose.
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EboHandle.Identifier);

            //Pass our indices to OpenGL
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * ElementIndices.Length), ElementIndices, BufferUsageHint.StaticDraw);

            //Unbind hardware buttons (?) wtf
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private void CreateVAOs()
        {
            //The VAO retains things so we need less calls

            //First grab a handle to a Vertex Array in OpenGL
            VaoHandle.Identifier = GL.GenVertexArray();

            //It needs to be bound to be able to use it.
            GL.BindVertexArray(VaoHandle.Identifier);

            //Create a vertex attribute array for the position of vertices
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, PositionVboHandle.Identifier); //Que?
            GL.VertexAttribPointer(0,3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);

            //Also make sure to bind the indices (?)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EboHandle.Identifier);

            //Clear hardwares stuff?
            GL.BindVertexArray(0);
        }

        private void Resize()
        {
            GL.Viewport(0, 0, GameWindow.Width, GameWindow.Height);
        }

        private void Draw()
        {
            GL.Viewport(0, 0, 1680, 1050);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(VaoHandle.Identifier);
            GL.DrawElements(PrimitiveType.Triangles, ElementIndices.Length,
                DrawElementsType.UnsignedInt, IntPtr.Zero);

            GameWindow.SwapBuffers();
        }

        public void Dispose()
        {
            GameWindow.Dispose();
        }
    }
}
