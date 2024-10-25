namespace Interactions

open Aardvark.Base
open FSharp.Data.Adaptive
open Aardvark.Rendering
open Aardvark.Application.Slim
open Aardvark.Dom
open Aardvark.Dom.Utilities
open Aardium

module Sg =
    let mapShaders (mapEffect : FShade.Effect -> FShade.Effect) (scene : ISceneNode) =
        let mapping (o : IRenderObject) =
            match o with
            | :? RenderObject as o ->
                match o.Surface with
                | Surface.Effect e ->
                    let o2 = RenderObject.Clone(o)
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


