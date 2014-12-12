using System;
using System.Diagnostics;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace opentk
{
    class MyApplication
    {
        private static GameWindow game;
        private static float x = 0;
        private static float y = 0;
        private static float z = 0;

        private static int r = 0;
        private static int g = 0;
        private static int b = 0;

        public static void Main()
        {
            using (game = new GameWindow())
            {
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;
                };

                game.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, game.Width, game.Height);
                };

                game.MouseMove += (sender, args) =>
                {
                    x = -6.25f + ((args.X / (float)game.Width) * 12.5f);
                    y = 5.0f - (args.Y / (float)game.Height) * 10.0f;

                    r = (int)(255 * (args.X / (float)game.Width) * 0.4f);
                    g = (int)(255 * (args.Y / (float)game.Height) * 0.85f);
                    b = (int)(((255 * (args.X / (float)game.Width)) + (255 * (args.Y / (float)game.Height))) / 2.0f);
                };

                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();

                    Matrix4d blar = Matrix4d.CreateOrthographic((game.Width / (double)game.Height) * 10, 1.0d * 10, 0.0d, 4.0d);

                    GL.MultMatrix(ref blar);

                    GL.Translate(x, y, z);

                    GL.Begin(PrimitiveType.Quads);

                    GL.Color3(Color.FromArgb(r % 255, g % 255, b % 255));
                    GL.Vertex2(-0.5f, 0.5f);
                    GL.Color3(Color.FromArgb(r % 255, g % 255, b % 255));
                    GL.Vertex2(0.5f, 0.5f);
                    GL.Color3(Color.FromArgb(r % 255, g % 255, b % 255));
                    GL.Vertex2(0.5f, -0.5f);
                    GL.Color3(Color.FromArgb(r % 255, g % 255, b % 255));
                    GL.Vertex2(-0.5f, -0.5f);

                    GL.End();

                    game.SwapBuffers();
                };

                // Run the game at 60 updates per second
                game.Run(60.0);
            }
        }
    }
}