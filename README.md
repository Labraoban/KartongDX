# DX11 Renderer - KartongDX

## Summary
This is a hobby project that I use to learn more about 3D Rendering. I have been working on this on and off for a while now.

I was interested in trying to make a renderer with PBR support to get a better grasp on what is going on under the hood. It is written in C# using a wrapper called SharpDX. I choose to use C# instead of C++ for this project because it was a while since I used C# when I started this project.

The project is very unoptimized and unorganized at the moment as it is very much a case of "learn as you go". But it succeeds in its purpose to be a way to learn the DX rendering pipeline as I now have many ideas on how to refactor and implement portions in a better way. Also I need to fix a lot of needed disposes of resources since the idea was mainly to worry about that later, and get things going quick.

## NuGet Packages needed
### Assimpnet
https://bitbucket.org/Starnick/assimpnet
### ini-parser
https://github.com/rickyah/ini-parser
### SharpDX
http://sharpdx.org/

SharpDX.D3DCompiler
SharpDX.Desktop
SharpDX.Direct2D1
SharpDX.Direct3D11
SharpDX.DirectInput
SharpDX.DXGI
SharpDX.Mathematics