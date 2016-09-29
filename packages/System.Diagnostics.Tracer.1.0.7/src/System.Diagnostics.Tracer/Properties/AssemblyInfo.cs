using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("System.Diagnostics.Tracer")]
[assembly: AssemblyProduct("System.Diagnostics.Tracer")]
[assembly: AssemblyCopyright("Copyright © 2015")]

[assembly: InternalsVisibleTo("System.Diagnostics.Tracer.Tests, PublicKey=002400000480000094000000060200000024000052534131000400000100010027dae7b9c059832ce5c6555903c9ca7b39acb047f61d7515b2eee5ccd68b27bf4301b29c2c01e12f0a1a98dd3608a1d5591944bfb831f5e3a89b94bd81eda2fe15a8731bcf1a675052be7f116183f253822f8760fb8c487c38d2dc0dd8e68aeeeb5b6ca739b883a0b443897a39e79e6abab4789852e9f25fa02069f741c364f0")]

[assembly: AssemblyVersion(ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch)]
[assembly: AssemblyFileVersion (ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch)]
[assembly: AssemblyInformationalVersion(ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch + "-" + ThisAssembly.Git.Branch + "+" + ThisAssembly.Git.Commit)]