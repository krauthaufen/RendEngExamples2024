open Aardvark.Base
open FSharp.Data.Adaptive
open Aardvark.Rendering
open Aardvark.Application.Slim
open Aardvark.Dom
open Aardvark.Dom.Utilities
open Aardium
open Interactions

[<EntryPoint>]
let main args =
    Aardvark.Init()
    Aardium.init()
    let app = new OpenGlApplication(false)
    let runtime = app.Runtime

    let shadowMapSize = V2i(2048, 2048)
    let lightPosition = cval (V3d(5,0,3))

    let lightView =
        lightPosition |> AVal.map (fun position ->
            let center = V3d.Zero
            let fw = Vec.normalize (center - position)

            let sky =
                if abs fw.Z > 0.99 then V3d.OIO
                else V3d.OOI

            CameraView.lookAt position V3d.Zero sky
            |> CameraView.viewTrafo
        )

    let lightProj =
        Frustum.perspective 90.0 0.1 100.0 1.0
        |> Frustum.projTrafo
        
    // texture will be loaded from a file when needed
    let grassTexture =
        FileTexture(Path.combine [__SOURCE_DIRECTORY__; ".."; ".."; "data"; "grass.jpg"], true) :> ITexture


    let scene =
        sg {
            Sg.Shader {
                DefaultSurfaces.trafo
            }
            sg {
                // the default box is centered around (0,0,0) so we shift it up in the z direction by half its size
                Sg.Translate(0.0, 0.0, 0.5)
                Primitives.Box(V3d.III, C4b.Red)
            }
            
            sg {
                Sg.Scale 2.0
                Sg.Translate(1.5, 0.0, 0.0)
                Primitives.Teapot(C4b.Green)
            }
            
            
            sg {
                Sg.Translate(0.0, 1.5, 0.0)
                Primitives.Octahedron(C4b.VRVisGreen)
            }
            
            sg {
                // we use a XY-Quad and scale it by a factor of 10 here to get a (-10, -10) to (10, 10) quad at z=0
                Sg.Shader {
                    DefaultSurfaces.trafo
                    DefaultSurfaces.diffuseTexture
                }
                // we pass the texture as "DiffuseColorTexture" which our "diffuseTexture" shader expects as a texture-name
                Sg.Uniform("DiffuseColorTexture", AVal.constant grassTexture)
                Sg.Scale 10.0
                Sg.Intersectable (Intersectable.planeXY (Box2d(V2d(-1, -1), V2d(1, 1))))
                Primitives.ScreenQuad 0.0
            }
        }

    let casterScene =
        sg {
            Sg.View lightView
            Sg.Proj lightProj
            let white = FShade.Effect.ofFunction (DefaultSurfaces.constantColor C4f.White)
            scene |> Sg.mapShaders (fun e -> FShade.Effect.compose [e; white])
        }

    let depthTexture =
        let signature = runtime.CreateFramebufferSignature [DefaultSemantic.DepthStencil, TextureFormat.Depth24Stencil8]
        let objects = casterScene.GetRenderObjects (TraversalState.empty runtime)
        let task = app.Runtime.CompileRender(signature, objects)
        task |> RenderTask.renderToDepthWithClear (AVal.constant shadowMapSize) (clear { depth 1.0; stencil 0})

    let node =
        body {
            renderControl {
                Style [
                    Position "fixed"; Top "0px"; Left "0px"
                    Width "100%"; Height "100%"; Background "black"
                ]

                SimpleOrbitController {
                    Location = V3d(7,6,4)
                    Center = V3d.Zero
                    RotateButton = Button.Left
                    PanButton = Button.Middle 
                }

                let! info = RenderControl.Info
                Sg.Proj(
                    info.ViewportSize 
                    |> AVal.map (fun size -> Frustum.perspective 90.0 0.1 100.0 (float size.X / float size.Y))
                    |> AVal.map Frustum.projTrafo
                )

                Sg.Uniform("LightLocation", lightPosition)

                sg {
                    Sg.Uniform("ShadowView", lightView)
                    Sg.Uniform("ShadowProj", lightProj)
                    Sg.Uniform("ShadowMap", depthTexture)

                    let shadowLight = FShade.Effect.ofFunction Shader.shadowLight
                    scene |> Sg.mapShaders (fun e ->
                        FShade.Effect.compose [e; shadowLight]
                    )
                }

                sg {
                    Sg.Shader {
                        DefaultSurfaces.trafo
                    }
                    Sg.Cursor "hand"
                    
                    let mutable down = false
                    Sg.OnPointerDown((fun e ->
                        down <- true
                        e.Context.SetPointerCapture(e.Target, e.PointerId)
                        false
                    ))
                    
                    Sg.OnPointerMove(fun e ->
                        if down then
                            let ray = e.Location.WorldPickRay
                            let plane = Plane3d(V3d.OOI, V3d(0,0,3))
                            
                            let mutable t = 0.0
                            if ray.Intersects(plane, &t) && t > 0.0 then
                                let pt = ray.GetPointOnRay t
                                transact (fun () -> lightPosition.Value <- pt)
                            false
                        else
                            true
                    )
                    
                    Sg.OnPointerUp(fun e ->
                        down <- false
                        e.Context.ReleasePointerCapture(e.Target, e.PointerId)
                        false
                            
                    )
                    Sg.Translate lightPosition
                    Primitives.Sphere(0.5)
                }

            }

        }

    // start a server and view the page
    let task = Server.Start(app.Runtime, node)
    if args |> Array.exists (fun a -> a = "--server") then
        task.Wait()
    else
        Aardium.run {
            url "http://localhost:5000/"
        }
    0