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
    
    // Here we create an OpenGlApplication which allows us to interact with the GL backend
    let app = new OpenGlApplication(false)
    let runtime = app.Runtime

    let shadowMapSize = V2i(2048, 2048)
    let lightPosition = cval (V3d(5,0,3))
    let hoverPosition = cval<option<V3d>> None
    let points = clist<V3d> [||]
    let hoverPoint = cval<option<Index>> None
    let shadowRenderCount = cval 0
    let mainRenderCount = cval 0
    
    // We adaptively calculate a view matrix for the light source
    // s.t. it alwyas looks at the center of the scene
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

    // a 90 degree perspective projection for the light source
    let lightProj =
        Frustum.perspective 90.0 0.1 100.0 1.0
        |> Frustum.projTrafo
        
    // texture will be loaded from a file when needed
    let grassTexture =
        FileTexture(Path.combine [__SOURCE_DIRECTORY__; ".."; ".."; "data"; "grass.jpg"], true) :> ITexture

    // here we specify our scene to render by not applying any lighting/shadow-lookup shaders.
    // we will apply lighting later when we render the scene with the appropriate shadow-map
    let scene =
        sg {
            Sg.Shader {
                DefaultSurfaces.trafo
            }
            
            // a box in the center
            sg {
                Sg.Translate(0.0, 0.0, 0.5)
                Primitives.Box(V3d.III, C4b.Red)
            }
            
            // a teapot next to it
            sg {
                Sg.Scale 2.0
                Sg.Translate(1.5, 0.0, 0.0)
                Primitives.Teapot(C4b.Green)
            }
            
            // an octahedron behind it
            sg {
                Sg.Translate(0.0, 1.5, 0.0)
                Primitives.Octahedron(C4b.VRVisGreen)
            }
            
            // a ground plane with the grass texture
            sg {
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
            
            Sg.Shader {
                DefaultSurfaces.trafo
            }
            points |> AList.toASetIndexed |> ASet.map (fun (i, (point)) ->
                sg {
                    // use a crosshair cursor when hovering the point
                    Sg.Cursor "crosshair"
                    
                    // whenever the cursor enters the point we set the hoverPoint to the current index
                    Sg.OnPointerEnter (fun _ -> transact (fun () -> hoverPoint.Value <- Some i))
                    Sg.OnPointerLeave (fun _ -> transact (fun () -> hoverPoint.Value <- None))
                    
                    // clicking the point removes it
                    Sg.OnTap (fun e ->
                        transact (fun () -> points.Remove i |> ignore)
                        false
                    )
                    
                    // if this point is the hovered one we render it in yellow. otherwise it's blue
                    let color =
                        hoverPoint |> AVal.map (fun v ->
                            match v with
                            | Some idx when idx = i -> C4b.Yellow
                            | _ -> C4b.Blue
                        )
                    
                    Sg.Translate(point)
                    Primitives.Sphere(0.3, color)
                }    
            )
            
        }

    // for rendering the shadow-map we create a new scene with the same objects
    // but with a short-cut shader that only applies a white color to the objects (mapShaders)
    let casterScene =
        sg {
            // first we apply the view/proj matrices for rendering the shadow-map
            Sg.View lightView
            Sg.Proj lightProj
            
            // here FShade's composition takes care of removing all texturing/etc. for our caster-scene
            let white = FShade.Effect.ofFunction (DefaultSurfaces.constantColor C4f.White)
            scene |> Sg.mapShaders (fun e -> FShade.Effect.compose [e; white])
        }

    
    // here we adaptively render our caster-scene to a texture and get a `IAdaptiveResource<IBackendTexture>` back.
    // This `IAdaptiveResource` is basically an `aval` that also carries information on how to destroy the resource
    // once it is no longer needed.
    let depthTexture =
        let signature = runtime.CreateFramebufferSignature [DefaultSemantic.DepthStencil, TextureFormat.Depth24Stencil8]
        let objects = casterScene.GetRenderObjects (TraversalState.empty runtime)
        let task = app.Runtime.CompileRender(signature, objects)
        
        // we sneak in a RenderTask that counts its executions (for counting the number of shadow-passes)
        let counterTask =
            RenderTask.custom (fun _ ->
                transact (fun () -> shadowRenderCount.Value <- shadowRenderCount.Value + 1)
            )
        
        RenderTask.ofList [task; counterTask]
        |> RenderTask.renderToDepthWithClear (AVal.constant shadowMapSize) (clear { depth 1.0; stencil 0})
    

    
    // we define a simple HTML-UI consisting of a single `RenderControl` that renders our scene
    let ui =
        body {
            div {
                Style [
                    Position "fixed"
                    Padding "10px"
                    Top "0px"
                    Left "0px"
                    FontFamily "monospace"
                    FontSize "30pt"
                    Color "white"
                    UserSelect "none"
                    ZIndex 999
                ]
                
                h3 {
                    // Hover Position
                    hoverPosition |> AVal.map (function
                        | Some v -> sprintf "Hover: (%.3f, %.3f, %.3f)" v.X v.Y v.Z
                        | None -> "Hover: None"
                    )
                }
                
                table {
                    Style [
                        Color "white"
                        FontSize "20pt"
                    ]
                    tr {
                        td { "shadow passes" }
                        td { AVal.map string shadowRenderCount }
                    }
                    tr {
                        td { "main passes" }
                        td { AVal.map string mainRenderCount }
                    }
                }
                
                
                
                // List of all current points
                ul {
                    points |> AList.mapi (fun i p ->
                        li {
                            // use a crosshair cursor when hovering the list-entry
                            Style [Css.Cursor "crosshair"]
                            
                            // make hovered entry yellow
                            hoverPoint |> AVal.map (fun v ->
                                match v with
                                | Some idx when idx = i -> Style [Color "yellow"]
                                | _ -> Style []
                            )
                            
                            // use the coords as label
                            sprintf "(%.3f, %.3f, %.3f)" p.X p.Y p.Z
                           
                            // whenever the cursor enters the list-item we set the hoverPoint to the current index
                            Dom.OnMouseEnter(fun _ -> transact (fun () -> hoverPoint.Value <- Some i))
                            Dom.OnMouseLeave(fun _ -> transact (fun () -> hoverPoint.Value <- None))
                            
                            // clicking the item removes the point
                            Dom.OnClick(fun _ -> transact (fun () -> points.Remove i |> ignore))
                        }    
                    )
                }
                
            }
            renderControl {
                Style [
                    Position "fixed"
                    Top "0px"
                    Left "0px"
                    Width "100%"
                    Height "100%"
                    Background "black"
                ]
                
                RenderControl.OnRendered (fun _ ->
                    transact (fun () -> mainRenderCount.Value <- mainRenderCount.Value + 1)    
                )
                
                // the standard 90 degree projection 
                let! info = RenderControl.Info
                Sg.Proj(
                    info.ViewportSize 
                    |> AVal.map (fun size -> Frustum.perspective 90.0 0.1 100.0 (float size.X / float size.Y))
                    |> AVal.map Frustum.projTrafo
                )
                
                // Aardvark.Dom allows us to apply a simple WSAD controller with just a few lines.
                SimpleFreeFlyController {
                    Location = V3d(7,6,4)
                    LookAt = V3d.Zero
                    Sky = V3d.OOI
                }
                
                // we could also use an orbit controller
                // SimpleOrbitController {
                //     Location = V3d(7,6,4)
                //     Center = V3d.Zero
                //     RotateButton = Button.Left
                //     PanButton = Button.Middle 
                // }
                
                // whenever the pointer moves over the scene
                Sg.OnPointerMove(true, fun e ->
                    transact (fun () -> hoverPosition.Value <- Some e.WorldPosition)
                )
                
                // when you shift+click anywhere on the scene we add a new point there.
                Sg.OnTap(fun e ->
                    if e.Shift || e.Button = Button.Right then
                        transact (fun () -> points.Add e.WorldPosition |> ignore)
                )


                // Light location and the shadow-map uniforms (as requested by the shadowLight shader)
                Sg.Uniform("LightLocation", lightPosition)
                Sg.Uniform("ShadowView", lightView)
                Sg.Uniform("ShadowProj", lightProj)
                Sg.Uniform("ShadowMap", depthTexture)

                
                // render our scene by composing our `shadowLight` shader to whatever shader the objects use
                // for rendering the scene.
                sg {
                    let shadowLight = FShade.Effect.ofFunction Shader.shadowLight
                    scene |> Sg.mapShaders (fun e ->
                        FShade.Effect.compose [e; shadowLight]
                    )
                }

                // the light-source as a sphere with drag interaction
                sg {
                    Sg.Cursor "hand"
                    
                    Sg.Shader {
                        DefaultSurfaces.trafo
                    }
                    
                    let mutable down = false
                    Sg.OnPointerDown((fun e ->
                        down <- true
                        e.Context.SetPointerCapture(e.Target, e.PointerId)
                        false
                    ))
                    
                    Sg.OnPointerMove(fun e ->
                        if down then
                            let ray = e.WorldPickRay
                            let plane = Plane3d(V3d.OOI, V3d(0,0,3))
                            let mutable t = 0.0
                            
                            if ray.Intersects(plane, &t) && t > 0.05 && t < 30.0 then
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
    let task = Server.Start(app.Runtime, ui)
    if args |> Array.exists (fun a -> a = "--server") then
        task.Wait()
    else
        Aardium.run {
            url "http://localhost:5000/"
        }
    0