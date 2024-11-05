### Build

requires [DotNet SDK 8.0.403](https://dotnet.microsoft.com/en-us/download)

1. `dotnet tool restore`
2. `dotnet paket restore`
3. `dotnet build`

#### For the WebAssembly project
`dotnet workload install wasm-tools`


### Running

* use Visual Studio / JetBrains Rider / VSCode
* in the project directories (`src/Interactions` / `src/SimpleSceneGraph`/ `src/CSharpScene`) run `dotnet run`

Note that the Interactions example will download our custom-built electron on first startup which will take some time.
