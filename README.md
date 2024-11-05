Code for [Lecture](./Lecture.md)

### Build

requires [DotNet SDK 8.0.403](https://dotnet.microsoft.com/en-us/download)

1. `dotnet tool restore`
2. `dotnet paket restore`
3. `dotnet build`

#### For the WebAssembly project
`dotnet workload install wasm-tools`


### Running

[Live Demo](https://georg.haaser.me/WASM/shadowmap/)

[<img width="643" alt="image" src="https://github.com/user-attachments/assets/0fb3825f-0d18-4ac7-9659-92ff58350b06">](https://georg.haaser.me/WASM/shadowmap/)


* use Visual Studio / JetBrains Rider / VSCode
* in the project directories (`src/Interactions` / `src/SimpleSceneGraph`/ `src/CSharpScene`) run `dotnet run`

Note that the Interactions example will download our custom-built electron on first startup which will take some time.
