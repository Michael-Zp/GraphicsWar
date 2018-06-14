﻿using GraphicsWar.Model;
using GraphicsWar.View;
using System;
using Zenseless.ExampleFramework;
using Zenseless.OpenGL;
using Zenseless.Patterns;

namespace GraphicsWar.Controller
{
    public class MainController
    {
        [STAThread]
        private static void Main()
        {
            GameTime gameTime = new GameTime();
            var window = new ExampleWindow();

            
            var orbit = window.GameWindow.CreateOrbitingCameraController(10f, 90, 0.1f, 500f);
            var visual = new MainView(window.RenderContext.RenderState, window.ContentLoader);
            var model = new MainModel();
            window.Update += (period) => model.Update(gameTime.DeltaTime);
            window.Render += () => visual.Render(model.Entities.ToViewEntities(), gameTime.AbsoluteTime, orbit);
            window.Resize += visual.Resize;
            window.Run();
        }
    }
}