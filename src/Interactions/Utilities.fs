namespace Interactions

open Aardvark.Base
open FSharp.Data.Adaptive
open Aardvark.Rendering
open Aardvark.Application.Slim
open Aardvark.Dom
open Aardvark.Dom.Utilities
open Aardium

module Sg =
    module Shader =
        open FShade
        let simpleLighting (v : Effects.Vertex) =
            fragment {
                return V4d.IIII
            }
            
    let slideStuff() =
        sg {
            Sg.Shader { Shader.simpleLighting }
            Sg.BlendMode BlendMode.Add
            
            sg {
                Sg.OnClick (fun e ->
                    printfn "clicked sphere: %A" e.WorldPosition
                )
                Sg.Translate(10.0, 0.0, 0.0)
                Primitives.Sphere(radius = 1.0, color = C4b.Red)
            }
            sg {
                Sg.Scale 10.0
                Primitives.Box(size = V3d(2,2,2))
            }
        }
        
    
    let mapShaders (mapEffect : FShade.Effect -> FShade.Effect) (scene : ISceneNode) =
        let mapping (o : IRenderObject) =
            match o with
            | :? RenderObject as o ->
                match o.Surface with
                | Surface.Effect e ->
                    let o2 = RenderObject.Clone(o)
                    match RenderObject.traversalStates.TryGetValue o with
                    | (true, s) -> RenderObject.traversalStates.Add(o2, s)
                    | _ -> ()
                    o2.Surface <- Surface.Effect (mapEffect e)
                    o2 :> IRenderObject
                | _ ->
                    o :> IRenderObject
            | _ -> o
        { new ISceneNode with
            member x.GetRenderObjects(state) =
                scene.GetRenderObjects(state) |> ASet.map mapping
            member x.GetObjects(state) =
                let a,b = scene.GetObjects(state)
                ASet.map mapping a, b
        }


