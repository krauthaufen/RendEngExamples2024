## Introduction

### Abstract
In this lecture we discuss different characteristics of rendering engines (e.g. game engine vs. movie rendering engine,.. ), give an overview on the structure of the lecture and identify tasks and challenges of rendering engine.
As a key challenge engine needs to bridge the gap between easy application programming abstractions and high-performance rendering (just like a compiler). 
We identify the degree of dynamism (how can the rendered scene change over time?) as a central parameter which influences the design of the rendering engine throughout each module. 
Next, we give an overview on the structure of the remaining lectures.

### Outlook into the next lecture
To tackle the main challenge of closing the abstraction gap between application programming and high-performance rendering it is necessary to understand the underlying machine and efficient usage patterns. To this end, in the next lecture we look at graphics APIs in more depth.

### Rendering Engine takeaways
Tasks, challenges and overview of techniques used in rendering engines. 

### General learnings 
Throughout the lecture, we show connect rendering-engine problems to concepts in compiler technology, runtime-systems and software engineering.

## Graphics APIs and Graphics Hardware Insights

### Abstract
This lecture gives an overview on the historical development of different graphics APIs (DirectX, OpenGL, Vulkan, WebGPU planned). We identify sources of overheads induced by the design of the API (communication, validation, state management). By looking at graphics API features and their development we identify efficient usage patterns.
Next we look at abstraction mechanisms and how a rendering engine could support multiple rendering backends (e.g. GL, WebGPU etc.).

### Outlook into the next lecture
This lecture showed how we *need* to work with graphics hardware. In the next lecture we look at options on how we *want* to expose scenes and logics to application programmers, i.e. the other end of the abstraction gap.

### Rendering Engine takeaways 
Abstraction mechanisms for graphics APIs, the higher-level rendering engine modules need to be built on.

### General computer science learnings 
Efficient usage patterns for graphics APIs, specialized hardware-features for high-performance rendering, API design choices for graphics APIs.

## Scene Representation

### Abstract 
Input for rendering engines is typically a scene description (in games for example a game world/level). In this lecture we look at scene graphs, a renderable object abstraction and how to convert between them (planned for next year: also mention data-oriented approaches such as entity/component representations).
We show the tradeoffs of scene graph implementation techniques such as interfaces, visitors and attribute grammars.

### Outlook into the next lecture
Having scene descriptions and graphics hardware understanding at hand, the translation unit needs to be designed (bridging the scene description -> graphics hardware gap). In the next lecture we look at techniques on rendering scene descriptions efficiently. 

### Related research papers
- When expressing more complex logics in scene graphs, state-handling becomes challenging. The paper [Semantic Scene Graph](./papers/Paper-Semantic-Scenegraph.pdf) elaborates on this topic. 
- Rendering a scene graph naively comes with a performance penalty. The paper [Lazy Incremental Computation](./papers/Paper-Lazy-Incremental-Computation.pdf) gives background on this.

### Rendering Engine takeaways 
There is a ease-of-use/performance tradeoff for scene descriptions. 

### General computer science learnings 
Implementation techniques for tree traversals (object-oriented, functional and rule-based implementation). Extensibility, the expression problem and its possible solutions.


## Rendering Scene Representations (as held 2017-2023, not 2024)

### Abstract 
In this lecture we show implementation techniques for executing abstract graphics commands (render objects) efficiently (called rendering vm). We introduce *adaptive compilation* to deal with changes (new objects, changed parameters).
Next we analyze performance of hardware-features such as multi-draw-indirect and put it in perspective with the previously introduced rendering vm.

### Outlook into the next lecture
The approach presented in this lecture scales well when changes in the scene are small compared to the complete scene description. In the next lecture, we look at techniques on how to utilize parallel execution for graphics command submission and data-oriented representations to tackle the other end of the dynamism spectrum.

### Related research papers
This lecture is based on the paper [An Incremental Rendering VM](./papers/Paper-An-Incremental-Rendering-VM.pdf).

### Rendering Engine takeaways 
Peak performance implementation techniques for optimizing stateful graphics APIs.

### General computer science learnings 
Virtual Machine implementation techniques, Adaptive Compilation and Just-in-Time-Compilation, Specialized Graphics Hardware features


## Rendering Engine Architectures and Entity Component Systems  (as held 2017-2023, not 2024)

### Abstract 
In the last lecture we looked at adaptive compilation to deal with small changes to large scenes. In this lecture we focus on massively dynamic scenes.
Firstly, we discuss the multi-threaded graphics command system in the unreal engine. 
Next we look at ECS data-representation optimizing for large, massively dynamic scenes.

### Outlook into the next lecture
We managed to bridge the scene description - rendering abstraction gap. Further climbing up the abstractions towards materials, shaders and global illumination? The basis here is a shader module for the rendering engine, which we will handle in the next lecture.

### Rendering Engine takeaways 
Parallel execution and data-oriented design for rendering engines.

### General computer science learnings 
Data-oriented design, Efficient array processing, Parallel Processing.

## Domain Specific Languages and Shader Systems for Rendering Engines

### Abstract 
In this lecture we look at the design space of shader systems (capability vs effort to use) and how to deal with the explosion of variants. 
To gain understanding of the underlying principles, we look at it from a language design and implementation perspective and investigate domain-specific-languages and apply the theory to shader languages.
Next we look at the design of pipeline shaders and show a the concrete FShade implementation as used by the aardvark rendering engine.

### Outlook into the next lecture
In the next lecture, we go from shaders to materials and lights from a rendering-engine perspective.

## Materials and Lights for Rendering Engines

## Shading System and Global Illumination

## Data and Rendering Engines

## Abstract 
TODO

## Dataflow in Scenarify

## Abstract
TODO
