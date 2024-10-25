open Aardvark.Base
open System.Threading
open FSharp.Data.Adaptive
open Aardvark.Application
open Aardvark.Rendering
open Aardvark.Application.Slim
open Microsoft.AspNetCore
open Aardvark.Dom
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Giraffe
open Aardvark.Dom.Remote
open Interactions
open Aardium

type OrbitController = SimpleOrbitController of OrbitState

type RenderControlBuilder with
    member x.Yield(SimpleOrbitController state) =
        let mutable state = state
        let astate = AdaptiveOrbitState state

        let coll = new System.Collections.Concurrent.BlockingCollection<_>()

        let env = 
            { new Env<OrbitMessage> with
                member this.Emit(messages: OrbitMessage seq): unit = 
                    coll.Add messages
                member this.Run(js: string, arg1: (System.Text.Json.JsonElement -> unit) option): unit = 
                    raise (System.NotImplementedException())
                member this.RunModal(modal: System.IDisposable -> DomNode): System.IDisposable = 
                    raise (System.NotImplementedException())
                member this.Runtime: IRuntime = 
                    failwith ""
                member this.StartWorker(): System.Threading.Tasks.Task<WorkerInstance<'b,'a>> = 
                    raise (System.NotImplementedException())
            }
        let runner =
            startThread <| fun () ->
                for msgs in coll.GetConsumingEnumerable() do
                    for msg in msgs do
                        state <- OrbitController.update env state msg
                        transact (fun () -> astate.Update state)

        x.Combine(
            x.Combine(
                x.Yield(OrbitController.getAttributes env), 
                x.Yield(RenderControl.OnRendered (fun _ -> env.Emit [OrbitMessage.Rendered]))
            ),
            
            x.Combine(
                x.Yield(Sg.View (astate.view |> AVal.map CameraView.viewTrafo)),
                x.Yield(Sg.OnDoubleTap(fun e -> env.Emit [OrbitMessage.SetTargetCenter(true, AnimationKind.Tanh, e.WorldPosition)]; false))
            )
        )

type ScenePointerEvent with
    member e.Ray =
        let tc = e.Pixel / V2d e.ViewportSize
        let ndc = V2d(2.0 * tc.X - 1.0, 1.0 - 2.0 * tc.Y)
        
        let p0 = e.ViewProjTrafo.Backward.TransformPosProj ndc.XYN
        let p1 = e.ViewProjTrafo.Backward.TransformPosProj ndc.XYO
        
        let ray = Ray3d(p0, Vec.normalize (p1 - p0))
        ray
        
module Shader =
    open FShade

    let shadow =
        sampler2dShadow {
            texture uniform?ShadowMap
            addressU WrapMode.Wrap
            addressV WrapMode.Wrap
            filter Filter.MinMagLinear
            comparison ComparisonFunction.Less
        }

    type UniformScope with
        member x.ShadowView : M44d = uniform?ShadowView
        member x.ShadowProj : M44d = uniform?ShadowProj

    let shadowLookup (v : Effects.Vertex) =
        fragment {
            let wp = uniform.ViewProjTrafoInv * v.pos
            let shadowPos4 = uniform.ShadowProj * (uniform.ShadowView * wp)

            let mutable shadowValue = 1.0
            if shadowPos4.X >= -shadowPos4.W && shadowPos4.X <= shadowPos4.W && shadowPos4.Y >= -shadowPos4.W && shadowPos4.Y <= shadowPos4.W then
                let shadowPos = shadowPos4.XYZ / shadowPos4.W

                let ccx = 0.5 * shadowPos.X + 0.5
                let ccy = 0.5 * shadowPos.Y + 0.5
                let ccz = 0.5 * shadowPos.Z + 0.5

                let sv = shadow.SampleLevel(V2d(ccx, ccy), ccz * 0.9999, 0.0)
                shadowValue <- sv

            return V4d(v.c.XYZ * shadowValue, 1.0)
        }




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


    let scene =
        sg {
            sg {
                // the default box is centered around (0,0,0) so we shift it up in the z direction by half its size
                Sg.Translate(0.0, 0.0, 0.5)
                Primitives.Box(V3d.III, C4b.Red)
            }    
            sg {
                // we use a XY-Quad and scale it by a factor of 10 here to get a (-10, -10) to (10, 10) quad at z=0
                Sg.Scale 10.0
                Sg.VertexAttribute("Colors", AVal.constant V4f.OOII)
                Primitives.ScreenQuad 0.0

            }
        }

    let casterScene =
        sg {
            Sg.View lightView
            Sg.Proj lightProj
            Sg.Shader {
                DefaultSurfaces.trafo
            }
            scene
        }

    let depth =
        let signature = runtime.CreateFramebufferSignature [DefaultSemantic.DepthStencil, TextureFormat.Depth24Stencil8]
        let task = app.Runtime.CompileRender(signature, casterScene.GetRenderObjects (TraversalState.empty runtime))
        task |> RenderTask.renderToDepthWithClear (AVal.constant shadowMapSize) (clear { depth 1.0; stencil 0})






    let node =
        body {
            h1 { "hello" }

            renderControl {
                Style [
                    Position "fixed"; Top "0px"; Left "0px"
                    Width "100%"; Height "100%"; Background "black"
                ]

                SimpleOrbitController (OrbitState.create V3d.Zero 1.0 0.5 3.0 Button.Left Button.Middle)

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
                    Sg.Uniform("ShadowMap", depth)

                    
                    // we apply a shader doing basic transformation and a simple lighting
                    Sg.Shader {
                        DefaultSurfaces.trafo
                        DefaultSurfaces.simpleLighting
                        Shader.shadowLookup
                    }

                    scene
                }

                sg {
                    Sg.Shader {
                        DefaultSurfaces.trafo
                        DefaultSurfaces.simpleLighting
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
                            let ray = e.Ray
                            let plane = Plane3d(V3d.OOI, V3d(0,0,3))
                            
                            let mutable t = 0.0
                            if e.Ray.Intersects(plane, &t) && t > 0.0 then
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
    let cancel = new CancellationTokenSource()
    let task = Server.start cancel.Token app.Runtime node
    if args |> Array.exists (fun a -> a = "--server") then
        task.Wait()
    else
        Aardium.run {
            url "http://localhost:5000/"
        }
        cancel.Cancel()
    0