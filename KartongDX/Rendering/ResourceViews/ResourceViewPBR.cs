using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D3D11 = SharpDX.Direct3D11;

namespace KartongDX.Rendering.ResourceViews
{
    public class ResourceViewPBR
    {
        public D3D11.ShaderResourceView albedo;
        public D3D11.ShaderResourceView normal;
        public D3D11.ShaderResourceView roughness;
        public D3D11.ShaderResourceView metallic;

        public D3D11.ShaderResourceView sky;
        public D3D11.ShaderResourceView irradiance;

        public ResourceViewPBR()
        {

        }
    }

    public class ShaderResourceViewCollection
    {
        private Dictionary<int, D3D11.ShaderResourceView> resourceViews;

        public List<int> Slots { get; private set; }

        public ShaderResourceViewCollection()
        {
            resourceViews = new Dictionary<int, D3D11.ShaderResourceView>();
        }

        public void AddSRVT(int slot, D3D11.ShaderResourceView srvt)
        {
            resourceViews.Add(slot, srvt);
            Slots = resourceViews.Keys.ToList();
        }

        public D3D11.ShaderResourceView GetSRVT(int slot)
        {
            return resourceViews[slot];
        }

        public bool HasSRVT(int slot)
        {
            return resourceViews.Keys.Contains(slot);
        }
    }
}
