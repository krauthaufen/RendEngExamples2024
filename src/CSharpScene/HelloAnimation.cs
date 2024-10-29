using Aardvark.Base;
using FSharp.Data.Adaptive;
using CSharp.Data.Adaptive;
using Aardvark.Rendering;
using Aardvark.SceneGraph;
using Aardvark.SceneGraph.CSharp;
using Aardvark.Application.Slim;
using Effects = Aardvark.Rendering.Effects;

namespace PlainAardvarkRendering_NetFramework
{
    class HelloAnimation
    {
        public static void Run()
        {
            using (var app = /*new VulkanApplication() */ new OpenGlApplication())
            {
                var win = app.CreateGameWindow(samples: 8);
                
                // build object from indexgeometry primitives
                var cone = 
                    IndexedGeometryPrimitives.Cone.solidCone(V3d.OOO, V3d.OOI, 1.0, 0.2, 48, C4b.Red).ToSg();
                
                // or directly using scene graph API
                var cube = 
                    SgPrimitives.Sg.box(
                        new ConstantValue<C4b>(C4b.Blue), 
                        new ConstantValue<Box3d>(Box3d.FromCenterAndSize(V3d.Zero, V3d.III))
                    );
                
                // initial camera
                var initialViewTrafo = CameraView.LookAt(V3d.III * 3.0, V3d.OOO, V3d.OOI);
                
                // camera controller WSAD
                var controlledViewTrafo = 
                    Aardvark.Application.DefaultCameraController.control(
                        win.Mouse, win.Keyboard, win.Time, 
                        initialViewTrafo
                    );
                
                var frustum = 
                    win.Sizes.Map(size => 
                        FrustumModule.perspective(60.0, 0.1, 10.0, (double)size.X / (double)size.Y)
                    );

                var t0 = DateTime.Now;
                var angle = win.Time.Map(t => (t - t0).TotalSeconds);
                var rotatingTrafo = angle.Map(a => Trafo3d.RotationZ(a));

                var scene =
                    new[] {
                        cone.Trafo(AValModule.constant(Trafo3d.Translation(1.0,1.0,0.0))),
                        cube.Trafo(rotatingTrafo)
                    }
                    .ToSg()
                    .WithEffects(new[]
                    {
                        Effects.Trafo.Effect, 
                        Aardvark.Rendering.Effects.SimpleLighting.Effect
                    })
                    .ViewTrafo(controlledViewTrafo.Map(c => c.ViewTrafo))
                    .ProjTrafo(frustum.Map(f => f.ProjTrafo()));

                win.RenderTask = 
                    app.Runtime.CompileRender(win.FramebufferSignature, scene);
                win.Run();
            }
        }
    }
}
