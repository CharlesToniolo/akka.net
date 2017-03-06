#I @"tools/FAKE/tools"
#r "FakeLib.dll"

open System
open System.IO
open System.Text

open Fake
open Fake.DotNetCli

// Variables
let configuration = "Release"

// Directories
let output = __SOURCE_DIRECTORY__  @@ "build"
let outputTests = output @@ "TestResults"
let outputPerfTests = output @@ "perf"
let outputBinaries = output @@ "binaries"
let outputNuGet = output @@ "nuget"
let outputBinariesNet45 = outputBinaries @@ "net45"
let outputBinariesNetStandard = outputBinaries @@ "netstandard1.6"

Target "Clean" (fun _ ->
    CleanDir output
    CleanDir outputTests
    CleanDir outputPerfTests
    CleanDir outputBinaries
    CleanDir outputNuGet
    CleanDir outputBinariesNet45
    CleanDir outputBinariesNetStandard

    CleanDirs !! "./**/bin"
    CleanDirs !! "./**/obj"
)

Target "RestorePackages" (fun _ ->
    DotNetCli.Restore
        (fun p -> 
            { p with
                Project = "./src/Akka.sln"
                NoCache = false })
)

Target "Build" (fun _ ->
    if (isWindows) then
        let projects = !! "./**/core/**/*.csproj"
                       ++ "./**/core/**/*.fsproj"
                       ++ "./**/contrib/**/*.csproj"
                       -- "./**/Akka.MultiNodeTestRunner.Shared.Tests.csproj"
                       -- "./**/Akka.FSharp.Tests.fsproj"
                       -- "./**/serializers/**/*Wire*.csproj"
                       -- "./**/Akka.TestKit.Xunit.csproj"
                       -- "./**/transports/**/*.csproj"

        let runSingleProject project =
            DotNetCli.Build
                (fun p -> 
                    { p with
                        Project = project
                        Configuration = configuration })

        projects |> Seq.iter (runSingleProject)
    else
        let projects = !! "./**/core/**/*.csproj"
                       ++ "./**/contrib/cluster/**/*.csproj"
                       ++ "./**/contrib/persistence/**/*.csproj"
                       ++ "./**/contrib/**/Akka.TestKit.Xunit2.csproj"
                       -- "./**/*MultiNode*.csproj"
                       -- "./**/Akka.NodeTestRunner.csproj"
                       -- "./**/Akka.Streams.Tests.TCK.csproj"
                       -- "./**/Akka.API.Tests.csproj"
                       -- "./**/Akka.API.Tests.csproj"
                       -- "./**/Akka.Cluster.TestKit.csproj"
                       -- "./**/Akka.Remote.Tests.csproj"
                       -- "./**/Akka.Remote.TestKit.csproj"
                       -- "./**/Akka.Remote.TestKit.Tests.csproj"
                       -- "./**/Akka.DistributedData.csproj"
                       -- "./**/Akka.DistributedData.Tests.csproj"
                       -- "./**/*.Performance.csproj"
                       -- "./**/Akka.Persistence.Sqlite.csproj"
                       -- "./**/Akka.Persistence.Sqlite.Tests.csproj"

        let runSingleProject project =
            DotNetCli.Build
                (fun p -> 
                    { p with
                        Project = project
                        Configuration = configuration })

        projects |> Seq.iter (runSingleProject)
)

Target "RunTests" (fun _ ->
    if (isWindows) then
        let projects = !! "./**/core/**/*.csproj"
                       ++ "./**/contrib/**/*.csproj"
                       -- "./**/Akka.Remote.Tests.csproj"
                       -- "./**/Akka.Remote.TestKit.Tests.csproj"
                       -- "./**/Akka.Streams.Tests.csproj"
                       -- "./**/Akka.Persistence.Sqlite.Tests.csproj"

        let runSingleProject project =
            DotNetCli.Test
                (fun p -> 
                    { p with
                        Project = project
                        Configuration = configuration })

        projects |> Seq.iter (runSingleProject)
    else
        let projects = !! "./**/core/*.Tests.csproj"
                       ++ "./**/contrib/cluster/**/*.Tests.csproj"
                       ++ "./**/contrib/persistence/**/*.Tests.csproj"
                       ++ "./**/contrib/testkits/**/*.Tests.csproj"
                       -- "./**/Akka.Persistence.Tests.csproj"
                       -- "./**/Akka.Remote.TestKit.Tests.csproj"
                       -- "./**/Akka.Remote.Tests.csproj"
                       -- "./**/Akka.Streams.Tests.csproj"
                       -- "./**/Akka.Persistence.Sqlite.Tests.csproj"
                       -- "./**/Akka.DistributedData.Tests.csproj"

        let runSingleProject project =
            DotNetCli.Test
                (fun p -> 
                    { p with
                        Project = project
                        Configuration = configuration })

        projects |> Seq.iter (runSingleProject)
)

//--------------------------------------------------------------------------------
// Nuget targets 
//--------------------------------------------------------------------------------

Target "CreateNuget" (fun _ ->
    let versionSuffix = getBuildParamOrDefault "versionsuffix" ""

    let projects = !! "src/**/Akka.csproj"
                   ++ "src/**/Akka.Cluster.csproj"
                   ++ "src/**/Akka.Cluster.TestKit.csproj"
                   ++ "src/**/Akka.Cluster.Tools.csproj"
                   ++ "src/**/Akka.Cluster.Sharding.csproj"
                   ++ "src/**/Akka.DistributedData.csproj"
                   ++ "src/**/Akka.Persistence.csproj"
                   ++ "src/**/Akka.Persistence.Query.csproj"
                   ++ "src/**/Akka.Persistence.TestKit.csproj"
                   ++ "src/**/Akka.Persistence.Query.Sql.csproj"
                   ++ "src/**/Akka.Persistence.Sql.Common.csproj"
                   ++ "src/**/Akka.Remote.csproj"
                   ++ "src/**/Akka.Remote.TestKit.csproj"
                   ++ "src/**/Akka.Streams.csproj"
                   ++ "src/**/Akka.Streams.TestKit.csproj"
                   ++ "src/**/Akka.TestKit.csproj"
                   ++ "src/**/Akka.TestKit.Xunit2.csproj"
                   ++ "src/**/Akka.DI.Core.csproj"
                   ++ "src/**/Akka.DI.TestKit.csproj"
                   ++ "src/**/Akka.Serialization.Hyperion.csproj"
                   ++ "src/**/Akka.Serialization.TestKit.csproj"
                   ++ "src/**/Akka.FSharp.fsproj"

    let runSingleProject project =
        DotNetCli.Pack
            (fun p -> 
                { p with
                    Project = project
                    Configuration = configuration
                    AdditionalArgs = ["--include-symbols"]
                    VersionSuffix = versionSuffix
                    OutputPath = outputNuGet })

    projects |> Seq.iter (runSingleProject)
)

Target "PublishNuget" DoNothing

//--------------------------------------------------------------------------------
// Help 
//--------------------------------------------------------------------------------

Target "Help" <| fun _ ->
    List.iter printfn [
      "usage:"
      "/build [target]"
      ""
      " Targets for building:"
      " * Build      Builds"
      " * Nuget      Create and optionally publish nugets packages"
      " * RunTests   Runs tests"
      " * All        Builds, run tests, creates and optionally publish nuget packages"
      ""
      " Other Targets"
      " * Help       Display this help" 
      ""]

//--------------------------------------------------------------------------------
//  Target dependencies
//--------------------------------------------------------------------------------

Target "BuildRelease" DoNothing
Target "All" DoNothing
Target "Nuget" DoNothing

// build dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "BuildRelease"

// tests dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "RunTests"

// nuget dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "CreateNuget"
"CreateNuget" ==> "PublishNuget"
"PublishNuget" ==> "Nuget"

// all
"BuildRelease" ==> "All"

RunTargetOrDefault "Help"