# Xamarin.Mac.Sdk bundling the wrong assemblies

This solution contains two projects:

* **WitnessMe**, a traditional Xamarin.Mac project that is only notable in that
  it explicitly references .NET 4.6.1.
* **SurpriseWitness**, a `Xamarin.Mac.Sdk` project that is otherwise meant to be
  identical to **WitnessMe**.

## Building and Running

Test the sample like so:

```bash
msbuild /r /bl &&  find . -name System.Memory.dll | xargs md5
```

Notice that SurpriseWitness bundles a different version of `System.Memory.dll`
than WitnessMe. Your output will look something like this:

```bash
MD5 (./WitnessMe/bin/Debug/WitnessMe.app/Contents/MonoBundle/System.Memory.dll) = 89ec6e101de3a70ed140c62c2980f24e
MD5 (./WitnessMe/bin/Debug/System.Memory.dll) = 89ec6e101de3a70ed140c62c2980f24e
MD5 (./SurpriseWitness/bin/Debug/net472/SurpriseWitness.app/Contents/MonoBundle/System.Memory.dll) = e1ce1dc0f9f08765f3ea59a24d0d905e
MD5 (./SurpriseWitness/bin/Debug/net472/System.Memory.dll) = 89ec6e101de3a70ed140c62c2980f24e
```

SurpriseWitness is bundling the `ref` assembly of `System.Memory.dll`. You can
verify like so:

```bash
find ~/.nuget/packages/system.memory/4.5.1 -name System.Memory.dll | xargs md5
```

```bash
MD5 (/Users/sandy/.nuget/packages/system.memory/4.5.1/ref/netstandard1.1/System.Memory.dll) = 8e35651c648c424af199e6dc3ec71676
MD5 (/Users/sandy/.nuget/packages/system.memory/4.5.1/ref/netstandard2.0/System.Memory.dll) = e1ce1dc0f9f08765f3ea59a24d0d905e
MD5 (/Users/sandy/.nuget/packages/system.memory/4.5.1/lib/netstandard1.1/System.Memory.dll) = a8771302c97226729d2b3c0b849c2969
MD5 (/Users/sandy/.nuget/packages/system.memory/4.5.1/lib/netstandard2.0/System.Memory.dll) = 89ec6e101de3a70ed140c62c2980f24e
```

## What's Going On?

Read https://github.com/xamarin/xamarin-macios/blob/d7c2a45ca9351106f83fcf7f08c1b09a8a5fb563/msbuild/Xamarin.Mac.Tasks/Xamarin.Mac.Common.targets#L504-L508, then look at your log.

It seems that the `_CompileToNative` target depends on values (like
`_ReferencesFromNuGetPackages`) being set from earlier NuGet targets (like
`ResolveNuGetPackageAssetes`). Those targets run for **WitnessMe** but not
**SurpriseWitness**, presumably because `Xamarin.Mac.Sdk` inherits behavior from
`Microsoft.NET.Sdk` that differs from the old NuGet targets.

`_ReferencesFromNuGetPackages` contains `ref` assemblies, apparently.
`_CompileToNative` removes those from the list of references sent to MMP...but
in our case that does not happen, so the `ref` version of `System.Memory.dll`
remains in the list, and gets chosen instead of the `lib` version (which is also
in the list).

The result is an app bundle with the wrong assemblies, and runtime issues, as
seen in this sample (launch `SurpriseWitness.app` and it will crash).

This same problem exists for other packages, such as
`System.Threading.Tasks.Extensions`.