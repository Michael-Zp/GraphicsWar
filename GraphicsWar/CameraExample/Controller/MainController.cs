using GraphicsWar.Model;
using GraphicsWar.View;
using System;
using System.Numerics;
using Zenseless.Base;
using Zenseless.ExampleFramework;
using Zenseless.OpenGL;

namespace GraphicsWar.Controller
{
    public class MainController
    {
        [STAThread]
        private static void Main()
        {
            GameTime gameTime = new GameTime();
            var window = new ExampleWindow();


            var orbit = window.GameWindow.CreateOrbitingCameraController(15f, 90, 0.1f, 500f);
            orbit.View.Azimuth = 250;
            orbit.View.Elevation = 40;
            orbit.View.TargetY = 10;
            var visual = new MainView(window.RenderContext.RenderState, window.ContentLoader);
            var model = new MainModel();

            Voronoi voronoi = new Voronoi(30, 30);
            visual.SetMesh(Shared.Enums.EntityType.Voronoi, voronoi.Mesh);
            foreach(var key in voronoi.CrystalPositions.Keys)
            {
                model.AddEntities(key, voronoi.CrystalPositions[key], Vector3.Zero, 1);
            }

            window.Update += (period) => model.Update(gameTime.DeltaTime);
            window.Render += () => visual.Render(model.Entities.ToViewEntities(), gameTime.AbsoluteTime, orbit);
            window.Resize += visual.Resize;
            window.GameWindow.KeyDown += (sender, e) =>
            {
                if (e.Key == OpenTK.Input.Key.Space)
                {
                    visual.Bloom = !visual.Bloom;
                }
            };
            window.Run();

        }
    }
}