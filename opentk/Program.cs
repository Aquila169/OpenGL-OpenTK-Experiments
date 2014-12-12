using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace opentk
{
    internal class MyApplication
    {
        public static void Main()
        {
            using (Game game = new Game())
            {
                game.Run();
            }
        }
    }
}