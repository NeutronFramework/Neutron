namespace neutroncli.Scripts.DataStructures;

public enum DotnetVersion
{
    net5,
    net6,
    net7,
    net8,
}

public enum FrontendFramework
{
    vanilla, 
    vanilla_ts, 
    vue, 
    vue_ts, 
    react, 
    react_ts, 
    react_swc, 
    react_swc_ts, 
    preact, 
    preact_ts, 
    lit, 
    lit_ts, 
    svelte, 
    svelte_ts, 
    solid, 
    solid_ts, 
    qwik, 
    qwik_ts
}

public enum BuildMode
{
    debug,
    release
}

public enum TargetPlatform
{
    win_x64,
    linux_x64
}