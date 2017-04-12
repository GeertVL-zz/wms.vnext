## Set dotnet watch for your asp.net project

Add the following to the csproj file

```
<ItemGroup>
 <DotNetCliToolReference Include="Microsoft.DotNet.Watcher.Tools" Version="1.0.0" />
</ItemGroup>
```

run "dotnet restore".


## How to add a nuget package to your project

