using System;
using System.IO;
using System.Collections.Generic;

using D3D11 = SharpDX.Direct3D11;
using SharpDX.D3DCompiler;



namespace KartongDX.Resources.Types
{
    class ShaderResourceDesc : ResourceDesc
    {
        public D3D11.Device Device { get; set; }
        public List<string> Defines { get; set; }

        public ShaderResourceDesc()
            : base() { }

        public ShaderResourceDesc(string fileName, string alias, D3D11.Device device, List<string> defines = null)
            : base(fileName, alias)
        {
            Device = device;
            Defines = defines;
        }
    }

    class ShaderResource : Resource
    {
        public ShaderResource(ShaderResourceDesc resourceDescription)
            : base(resourceDescription)
        { }

        public D3D11.VertexShader VertexShader { get; private set; }
        public D3D11.PixelShader PixelShader { get; private set; }

        public Rendering.Shader Shader { get; private set; }
        public ShaderSignature InputSignature { get; private set; }
        public D3D11.InputElement[] InputElements { get; private set; }

        public override void Dispose()
        {
            VertexShader.Dispose();
            InputSignature.Dispose();
            PrintDisposeLog();
        }

        protected override void Load(ResourceDesc resourceDescription)
        {
            ShaderResourceDesc desc = (ShaderResourceDesc)resourceDescription;

            string file = System.IO.File.ReadAllText(resourceDescription.FileName);
            InputElements = Rendering.ShaderPreprocessor.Preprocess(file);

            // Creates a string with specified Defines
            string defines = "";
            if (desc.Defines != null)
            {
                foreach (string str in desc.Defines)
                {
                    defines += String.Format("#define {0}\n", str);
                }
                file = defines + file;
            }

            ShaderInclude shaderInclude = new ShaderInclude("data/shaders/");
            ShaderBytecode vsShaderByteCode = ShaderBytecode.Compile(file, "vs_main", "vs_5_0", ShaderFlags.Debug, EffectFlags.None, null, shaderInclude);
            ShaderBytecode psShaderByteCode = ShaderBytecode.Compile(file, "ps_main", "ps_5_0", ShaderFlags.Debug, EffectFlags.None, null, shaderInclude);
            shaderInclude.Dispose();

            InputSignature = ShaderSignature.GetInputSignature(vsShaderByteCode);
            VertexShader = new D3D11.VertexShader(desc.Device, vsShaderByteCode);
            PixelShader = new D3D11.PixelShader(desc.Device, psShaderByteCode);

            Shader = new Rendering.Shader(desc.Device, VertexShader, PixelShader, InputSignature, InputElements, resourceDescription.Alias);
        }
    }

    public class ShaderInclude : Include
    {

        private string includeDirectory;
        public IDisposable Shadow { get; set; }

        public ShaderInclude(string includeDirectory)
        {
            this.includeDirectory = includeDirectory;
        }

        public void Close(Stream stream)
        {
            stream.Close();
            stream.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            return new FileStream(includeDirectory + fileName, FileMode.Open);
        }

        public void Dispose()
        {
            if(Shadow != null)
            {
                Shadow.Dispose();
            }
        }
    }

}
