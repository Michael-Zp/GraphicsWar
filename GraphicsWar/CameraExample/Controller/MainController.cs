using GraphicsWar.Model;
using GraphicsWar.View;
using System;
using System.Collections.Generic;
using System.Numerics;
using Zenseless.Base;
using Zenseless.ExampleFramework;
using Zenseless.Geometry;
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

            Voronoi voronoi = new Voronoi(30, 30, new Vector3(10, 5, 10));

            var additionalMeshes = new Dictionary<Shared.Enums.EntityType, Tuple<DefaultMesh, Vector4>>();
            additionalMeshes.Add(Shared.Enums.EntityType.Voronoi, Tuple.Create(voronoi.Mesh, new Vector4(.1f, 1, 0.5f, 0)));

            var visual = new MainView(window.RenderContext.RenderState, window.ContentLoader, additionalMeshes);
            var model = new MainModel();

            foreach(var key in voronoi.Crystals.Keys)
            {
                foreach(var crystal in voronoi.Crystals[key])
                {
                    model.AddEntity(key, crystal.Position, Vector3.UnitY * crystal.RotationFactor, crystal.ScaleFactor);
                }
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