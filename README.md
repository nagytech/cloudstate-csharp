# Purpose

This repo is a csharp dotnet implementation of the cloudstate.io
user service provider protocol.  

> Note: This README is a bit dated, so I plan to update it shortly.

# Prerequisites

- docker
- dotnetcore 2.2 SDK
- grpc_cli (for testing the gRPC endpoint - use your own if you prefer)
- mac/linux (currently limited by build, setup scripts - but should be
  cross platform eventually)

# Running the sample

Check out the examples/ShoppingCart folder for detailed instructions.

# Discussion

## gRPC Library

There are two common gRPC implementations for dotnet.  

- C Core Wrapper - https://github.com/grpc/grpc/tree/master/src/core
- Native C# - https://github.com/grpc/grpc-dotnet

The native C# implementation requires services to be registered by 
providing a concrete class to the route builder.  And at first glance
it did not seem possible to implement the cloudstate protocol this way.
However, I reached that conclusion in the early stages of writing this
code so it may actually be possible now given that I understand the 
framework a lot better now.

Additionally, the Native C# implementation is only supported by netcore
3.0 which is still in early days of adoption.  Comparatively, the C core
implementation is supported by netcore 2.0.  Adopting the C core library
allows us a wider array of adoption, but we may be able to deliniate 
user-function code away from the service code in a way that allows 
the user-functions to be written in any netstandard 1.0 compatible library.

Based on the above, I've chosen the C Core implementation of gRPC for
now and may change that later on.

## Protobuf Compilation

Currently there's an issue with OmniSharp (VSCode plugin that helps
orchestrate Roslyn and Intellisense) when adding a protobuf directive
to a netcore 3.0 csproj.  Additionally, when adding the Grpc.Tools library
to the project for build integration, we create a dependency on the 
Google.Protobuf library which does not fully support what we are setting 
out to achieve (_see 'Custom Libs' below).  So, protobuf compilation is 
currently provided by submodule repo.  Additionally, the examples/Template
project has been set up with a custom NuGet version of protobuf.

## Custom Libs

Currently, there's some limitations in the protobuf csharp implementation.
Primarily, the descriptor proto that is used to send a file descriptor set
on discovery is internal.  (I raised an issue on github, but at this point
don't have enough info about the whole system to justify the request).

As a result, I've downloaded the protobuf binaries and have customised the
field so that we can carry on with this implementation.

Will need to investigate the blockers caused by protobuf in order to ensure
that we can go ahead without customizing the protobuf library each time.
