namespace SimpleSceneGraph

open Aardvark.Base
open Aardvark.Rendering

module Shader =
    open FShade

    type Vertex =
        {
            [<Position>] pos : V4d
            [<PointSize>] s : float
            [<PointCoord>] c : V2d
            [<Color>] col : V4d
        }
    
    let pointSize (v : Vertex) =
        vertex {
            return { v with s = uniform?PointSize }
        }
    let circularPoint (v : Vertex) =
        fragment {
            let ndc = v.c * 2.0 - 1.0
            let r = Vec.length ndc
            if r >= 1.0 then discard()
            
            return v.col
        }