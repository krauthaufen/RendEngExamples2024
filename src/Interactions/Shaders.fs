namespace Interactions

open Aardvark.Base
open Aardvark.Rendering

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

    let samples32 = 
        [|
            V2d(0.0, 0.0) 
            V2d(-0.31875235511755723, -0.9449421061331551) 
            V2d(0.8873850913061684, 0.4596938710160174) 
            V2d(-0.9788250446705007, -0.1831111727831261) 
            V2d(0.005537750804412887, 0.9974571276106907) 
            V2d(0.8206399087445753, -0.5705805591897731) 
            V2d(-0.7414371503082338, 0.6202421677502155) 
            V2d(0.2168352886856725, -0.6534870950991977) 
            V2d(-0.4363688974163705, -0.39277773518261627) 
            V2d(0.5761431232992206, -0.09098619331581685) 
            V2d(0.27503756543061164, 0.4721663800876495) 
            V2d(-0.6043717347842896, 0.12596923451633443) 
            V2d(-0.2345988297176348, 0.5478495239228365) 
            V2d(0.4308666736714582, 0.8327643134966602) 
            V2d(0.9819876966573231, -0.15505172400391917) 
            V2d(-0.7825428490870162, -0.5337904740606889) 
            V2d(0.578034553138591, -0.8077971566927116) 
            V2d(0.24943457649794393, -0.23523511540977873) 
            V2d(-0.9665233680525819, 0.23360453377325477) 
            V2d(0.58695303753926, 0.2828890245499231) 
            V2d(-0.3820004327164163, 0.8943356333923626) 
            V2d(-0.09471927996029245, -0.5347210133858383) 
            V2d(-0.3261320726207562, -0.04224633287134217) 
            V2d(0.04245245984441612, -0.9052189889240561) 
            V2d(0.46603115774770676, -0.45385000171683715) 
            V2d(0.32604987699767757, 0.09760151251073435) 
            V2d(0.0479937119233933, 0.2985788668532307) 
            V2d(0.8872146466966282, 0.14060079703839684) 
            V2d(-0.4573233925844178, -0.6858206089508533) 
            V2d(0.6238920337016496, 0.5968574752482825) 
            V2d(-0.48980017066546505, 0.4176096007886444) 
            V2d(-0.6984946482175952, -0.24999220737512617) 
        |]
    let internal shadowLight (v : Effects.Vertex) =
        fragment {
            let n = v.n |> Vec.normalize
            let c = uniform.LightLocation - v.wp.XYZ |> Vec.normalize

            let diffuse = Vec.dot c n |> max 0.0

            
            let shadowPos4 = uniform.ShadowProj * (uniform.ShadowView * v.wp)

            let mutable shadowValue = 1.0
            if shadowPos4.X >= -shadowPos4.W && shadowPos4.X <= shadowPos4.W && shadowPos4.Y >= -shadowPos4.W && shadowPos4.Y <= shadowPos4.W then
                let shadowPos = shadowPos4.XYZ / shadowPos4.W

                let ccx = 0.5 * shadowPos.X + 0.5
                let ccy = 0.5 * shadowPos.Y + 0.5
                let ccz = 0.5 * shadowPos.Z + 0.5

                let mutable sv = 0.0
                let scale = 10.0 / V2d shadow.Size
                for i in 0 .. 31 do
                    let off = samples32.[i]
                    let svv = shadow.SampleLevel(V2d(ccx, ccy) + off*scale, ccz * 0.9995, 0.0)
                    sv <- sv + svv
                
                //let sv = shadow.SampleLevel(V2d(ccx, ccy), ccz * 0.9999, 0.0)
                shadowValue <- sv / 32.0

            
            
            return V4d(v.c.XYZ * (0.8 * shadowValue * diffuse + 0.2), v.c.W)
        }
