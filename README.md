## Design & Implementation of a Rendering Engine, Lecture @ [TU WIEN](https://www.cg.tuwien.ac.at/courses/RendEng/VU/2024W)

### Lecturers (in alphabetical order)

Daniel Cornel, Georg Haaser, Christian Luksch, Andreas Walch, Stefan Maierhofer, Harald Steinlechner, Attila Szabo

### Learning outcomes
After successful completion of the course, students are able to plan, implement, test and evaluate the design and programming of a rendering engine. The scene description, graphical APIs and optimization techniques play an important role.

### Subject of course
In this course we will focus on the following topics:

Requirements for the design of rendering engines
Hardware and Graphics APIs (OpenGL, Direct3D, Vulkan,..)
Scene Representation (Scene graphs, display lists, command buffers,...)
Static and Dynamic Data (Incremental Update Techniques)
Optimizations (Caching, Culling, Level of Detail, Bounding Volume Hierarchies, Just-In-Time Optimization)
Resource Management 
Domain Specific Languages (HLSL, Spark, FShade, Semantic Scene Graph,..)
Reusable Components/Design for Rendering Engines

### Lecture material

- [Motivation](./lecture/01-Introduction_Motivation.pdf)
- [Graphics APIs and Insights for Rendering Engines](./lecture/02-GraphicsHardwareAndInsights.pdf)
- [Scene Representation for Rendering Engines](./lecture/03-Scene-Representation.pdf)

    Accompanaying papers/talks:
    - [Paper: Semantic Scene Graph](./lecture/papers/Paper-Semantic-Scenegraph.pdf)
    - [Conference Slides: Semantic Scene Graph](./lecture/papers/Slides-Semantic-SceneGraph.pdf)
    - [Paper: Lazy Incremental Computation for Efficient Scene Graph Rendering](./lecture/papers/Paper-Lazy-Incremental-Computation.pdf)
    - [Conference Slides: Lazy Incremental Computation for Efficient Scene Graph Rendering](./lecture/papers/Slides-Lazy-Incremental-Computation.pdf)
- [Case Study + Aardvark Introduction/Lessons learned](./lecture/04-Introduction-Aardvark.pdf)

    Accompanaying papers/talks:
    - [Paper: An Incremental Rendering VM](./lecture/papers/Paper-An-Incremental-Rendering-VM.pdf)
- [Rendering Engine Architectures & ECS](./lecture/05-Rendering%20Engine%20Architectures%20&%20ECS.pdf)
- [DSLs and Shader Systems](./lecture/06-DSLs%20and%20Shader%20Systems.pdf)

    Accompanaying papers/talks:
    - [Paper: Language Integrated Shaders](./lecture/papers/paper-LINS.pdf)
- [Dataflow in Scenarify](./lecture/07-Data-flow%20in%20Scenarify.pdf)
- [Materials & Lights](./lecture/08-Materials%20&%20Lights%20Slides.pdf)

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
