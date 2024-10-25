namespace Interactions

open Aardvark.Base
open Aardvark.Application.Slim
open Microsoft.AspNetCore
open Aardvark.Dom
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Giraffe
open Aardvark.Dom.Remote
open Aardvark.Rendering
open System.Threading

module Server =

    let start (ct : CancellationToken) (rt : IRuntime) (content : DomNode) =
        let run (ctx : DomContext) = 
            content, Disposable.empty


        let host = 
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(
                    fun webHostBuilder ->
                        webHostBuilder
                            .UseSockets()
                            .Configure(fun b -> b.UseWebSockets().UseGiraffe (DomNode.toRoute rt run))
                            .ConfigureServices(fun s -> s.AddGiraffe() |> ignore)
                            |> ignore
                )
                .Build()

        host.Start()
        host.WaitForShutdownAsync(ct)

