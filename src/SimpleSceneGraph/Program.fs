open System
open FSharp.Data.Adaptive
open Aardvark.Base
open Aardvark.Rendering
open Aardvark.Dom
open Aardvark.Application
open Aardvark.Application.Slim
open SimpleSceneGraph

let rand = RandomSystem()

[<EntryPoint>]
let main _args =
    // initialize aardvark (this takes care of registering image-loaders, etc.)
    Aardvark.Init()

    // texture will be loaded from a file when needed
    let grassTexture =
        FileTexture(Path.combine [__SOURCE_DIRECTORY__; ".."; ".."; "data"; "grass.jpg"], true) :> ITexture

    let brickTexture =
        FileTexture(Path.combine [__SOURCE_DIRECTORY__; ".."; ".."; "data"; "brick.jpg"], true) :> ITexture

    // a currently empty array of points
    let points = 
        cval Array.empty<V3d>

    // create an "Application" which allows us to create windows/etc. 
    // here we use "Aardvark.Application.Slim" which creates GLFW windows underneath
    let app = new OpenGlApplication(false)

    // create a simple window for rendering
    let win = app.CreateGameWindow(4)


    // add minimal key-based interaction for changing the "points" array
    win.Keyboard.DownWithRepeats.Values.Add (fun k ->
        let bounds = Box3d(V3d(-10, -10, 0), V3d(10, 10, 5))
        match k with
        | Keys.Space -> 
            // pressing space fills the array with 400 new random points in the scene
            transact (fun () -> points.Value <- Array.init 400 (fun _ -> rand.UniformV3d bounds))
        | Keys.Delete | Keys.Escape ->
            // pressing Delete or Escape resets the array to empty
            transact (fun () -> points.Value <- Array.empty)
        | _ ->
            ()
    )

    // in order to see something we need some camera matrices.
    // here we use our DefaultCameraController which takes an initial camera
    // and the RenderWindow's keyboard and mouse to control the camera using WSAD and look-around.
    // win.Time here is a special adaptive value provided by the window that changes whenever a frame has been rendered
    // which the CameraController uses internally to control animations.
    // NOTE that the whole CameraController simply returns an `aval<CameraView>` that changes appropriately
    let view =
        CameraView.lookAt (V3d(4,3,4)) V3d.Zero V3d.OOI
        |> DefaultCameraController.controlWithSpeed (cval 8.0) win.Mouse win.Keyboard win.Time

    // for our perspective projection we need to know the window's aspect-ratio
    // therefore our `proj` adaptively depends on the window-size and determines its aspect.
    // NOTE that this computation will only be evaluated whenever the window is resized.
    let proj =
        win.Sizes |> AVal.map (fun size ->
            Frustum.perspective 90.0 0.1 100.0 (float size.X / float size.Y)
        )

    // finally we put together our simple scene
    let scene =
        sg {
            // we start by applying our two camera-matrices from above
            // Note that `CameraView` and `Frustum` are not directly passable
            // to the SceneGraph here, so we need to convert them to `Trafo3d`
            // using the appropriate functions. This does not represent something
            // deep or fundamental, the SceneGraph internally simply uses `Trafo3d`
            // Trafo3d is our representation of a simple 4x4 matrix with some additional information.
            Sg.View (AVal.map (fun v -> CameraView.viewTrafo(v)) view)
            Sg.Proj (AVal.map (fun f -> Frustum.projTrafo(f)) proj)
            
            // let t0 = DateTime.Now
            // Sg.Trafo(win.Time |> AVal.map (fun t -> Trafo3d.RotationZ((t - t0).TotalSeconds * 0.5)))
            //
            // here we render a unit-sized cube
            sg {
                // we apply a shader doing basic transformation and a simple lighting
                Sg.Shader {
                    DefaultSurfaces.trafo
                    DefaultSurfaces.simpleLighting
                }
                // the default box is centered around (0,0,0) so we shift it up in the z direction by half its size
                Sg.Translate(0.0, 0.0, 0.5)
                Primitives.Box(V3d.III, C4b.Red)
            }
       

            // and a ground-plane with our simple grass-texture from above
            sg {
                // we apply a shader doing basic transformation, simple texturing and lighting
                Sg.Shader {
                    DefaultSurfaces.trafo
                    DefaultSurfaces.diffuseTexture
                    DefaultSurfaces.simpleLighting
                }
                // we pass the texture as "DiffuseColorTexture" which our "diffuseTexture" shader expects as a texture-name
                Sg.Uniform("DiffuseColorTexture", AVal.constant grassTexture)

                // we use a XY-Quad and scale it by a factor of 10 here to get a (-10, -10) to (10, 10) quad at z=0
                Sg.Scale 10.0
                Primitives.ScreenQuad 0.0

            }

            // here we create something dynamic: the points from the beginning of the program should
            // be rendered as point-sprites with random colors.
            sg {
                // we start by creating an array of colors with the same length as the `points` array
                // where each point is assigned a random color.
                let colors = 
                    points |> AVal.map (fun pts -> 
                        // whenever `points` changes this function will be evaluated
                        pts |> Array.map (fun _ -> rand.UniformC3f().ToC4b())
                    )
                
                // a shader for creating point-sprites
                Sg.Shader {
                    DefaultSurfaces.trafo
                    Shader.pointSize
                    Shader.circularPoint
                }

                // the point-size (as expected by the pointSprite shader) should be 10px
                Sg.Uniform("PointSize", 10.0)

                // we set the mode and use our two arrays as vertex-buffers for rendering the points
                Sg.Mode(IndexedGeometryMode.PointList)
                Sg.VertexAttribute("Positions", points |> AVal.map (Array.map V3f))
                Sg.VertexAttribute("Colors", colors)

                // render here gets the "FaceVertexCount" needed for drawing.
                // in our case simply the adaptive number of points.
                Sg.Render(points |> AVal.map Array.length)

            }
            //
            // sg {
            //     Sg.Translate(3.0, 0.0, 0.5)
            //     Sg.Shader {
            //         DefaultSurfaces.trafo
            //         DefaultSurfaces.diffuseTexture
            //         DefaultSurfaces.simpleLighting
            //     }
            //     view |> AVal.map (fun v ->
            //         let distance = Vec.distance v.Location (V3d(3.0, 0.0, 0.5))
            //         
            //         if distance > 5.0 then
            //             sg {
            //                 Sg.Uniform("DiffuseColorTexture", AVal.constant grassTexture)
            //                 Primitives.Box(V3d.III, C4b.Red)
            //             }
            //         else 
            //             sg {   
            //                 Sg.Uniform("DiffuseColorTexture", AVal.constant brickTexture)
            //                 Primitives.Sphere(0.5, C4b.Aqua)
            //             }
            //         
            //     )
            // }
        
        


        }

    // here we get the adaptive set of RenderObjects from the scene
    let renderObjects = 
        scene.GetRenderObjects(TraversalState.empty(app.Runtime))

    // we can now "compile" them into a RenderTask which can in turn be fed into the Window for rendering
    let renderTask =
        app.Runtime.CompileRender(win.FramebufferSignature, renderObjects)

    win.RenderTask <- renderTask

    // finally we show the window and run the application-loop
    win.Run()

    0













