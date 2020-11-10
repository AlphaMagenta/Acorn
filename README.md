# Acorn Microframework

Acorn provides our games with essential nutrients to help them grow into strong and beautiful trees.

## Tips & Tricks

- To get VSCode autocompletion the best way is to copy the `Assembly-CSharp.csproj` from one of the Unity projects, and manually correct it as follows:

    - under `<ItemGroup>` replace an explicit list of compiled files with `<Compile Include="**/*.cs" />`
    - if reference to `Acorn.dll` exists, remove it

---

Made with <3 by [inca](https://github.com/inca)
