using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartongDX.Resources;
using D3D11 = SharpDX.Direct3D11;
using KartongDX.Resources.Types;
using KartongDX.Rendering;
using SharpDX;

namespace KartongDX.Engine.States
{
    class _TestState : GameState
    {
        private GameObject testObject;
        private GameObject sphere;
        private GameObject testUI;

        private GameObject skyMeshTest;

        private GameObject skyPlane;

        private List<Skybox> skyboxes;
        private int currentSkybox = 0;

        public _TestState() : base("TestState")
        {
            testObject = new GameObject();
            testObject.SetPosition(new SharpDX.Vector3(-5, 0, -2));

            sphere = new GameObject();
            sphere.SetPosition(new Vector3(5, 0, -2));
            sphere.Name = "Sphere";

            testUI = new GameObject();

            //skyMeshTest = new GameObject();
            //skyPlane = new GameObject();
            //skyPlane.SetPosition(new Vector3(2, 0, 0));

            stateScene.AddObjectAsRootChild(testObject);
            stateScene.AddObjectAsRootChild(sphere);
            uiScene.AddObjectAsRootChild(testUI);

            stateScene.AddObjectAsRootChild(skyMeshTest);
            stateScene.AddObjectAsRootChild(skyPlane);
        }

        protected override void LoadResources(ResourceManager resourceManager, SharpDX.Direct3D11.Device device)
        {
            skyboxes = new List<Skybox>();

            resourceManager.Textures.Load(new Texture2DResourceDescription("data/textures/sky/fireplace_4k.png", "SkyFac", device));
            resourceManager.Textures.Load(new Texture2DResourceDescription("data/textures/sky/noon_grass_4k.png", "SkyGrass", device));

            resourceManager.Shaders.Load(new ShaderResourceDescription("data/shaders/skybox.hlsl", "Sky", device));

            Rendering.CubemapRenderer cubemapRenderer = new CubemapRenderer(device, resourceManager);
            var factoryCubemap = cubemapRenderer.GenerateFromEquirectangular(resourceManager.Textures.GetFromAlias("SkyFac").Texture.Texture2D);
            var grassCubemap = cubemapRenderer.GenerateFromEquirectangular(resourceManager.Textures.GetFromAlias("SkyGrass").Texture.Texture2D);

            Skybox factorySkybox = new Skybox(factoryCubemap, resourceManager.Meshes.GetFromAlias("KDX_CUBE_IN").Mesh, resourceManager.Shaders.GetFromAlias("Sky").Shader);
            Skybox grassSkybox = new Skybox(grassCubemap, resourceManager.Meshes.GetFromAlias("KDX_CUBE_IN").Mesh, resourceManager.Shaders.GetFromAlias("Sky").Shader);

            skyboxes.Add(factorySkybox);
            skyboxes.Add(grassSkybox);

            stateScene.Skybox = skyboxes[0];

            resourceManager.Textures.Load(new Texture2DResourceDescription("data/textures/test/RedOctFloor_basecolor.png", "Diffuse", device));
            resourceManager.Textures.Load(new Texture2DResourceDescription("data/textures/test/RedOctFloor_normal.png", "Normal", device));
            resourceManager.Textures.Load(new Texture2DResourceDescription("data/textures/test/RedOctFloor_roughness.png", "Roughness", device));
            resourceManager.Textures.Load(new Texture2DResourceDescription("data/textures/test/RedOctFloor_metallic.png", "Metallic", device));
            resourceManager.Textures.Load(new Texture2DResourceDescription("data/textures/plain_normal.png", "PlainNormal", device));


            resourceManager.Meshes.Load(new MeshResourceDescription("data/models/plane.fbx", "Plane"));
            resourceManager.Meshes.Load(new MeshResourceDescription("data/models/monkey_high.fbx", "MonkeyHigh"));
            resourceManager.Meshes.Load(new MeshResourceDescription("data/models/monkey_smooth.fbx", "Monkey"));
            resourceManager.Meshes.Load(new MeshResourceDescription("data/models/barrel.fbx", "Barrel"));
            resourceManager.Meshes.Load(new MeshResourceDescription("data/models/sphere_smooth.fbx", "Sphere"));

            resourceManager.Shaders.Load(new ShaderResourceDescription("data/shaders/testShader_PBR.hlsl", "PBR", device));
            resourceManager.Shaders.Load(new ShaderResourceDescription("data/shaders/ui.hlsl", "UI", device));


            resourceManager.Textures.Load(new Texture2DResourceDescription("data/textures/StandardCubeMap.png", "Sky", device));
            resourceManager.Textures.Load(new Texture2DResourceDescription("data/textures/hdri.png", "SkyHDRI", device));

            resourceManager.Meshes.Load(new MeshResourceDescription("data/models/skybox.fbx", "Sky"));

            resourceManager.Shaders.Load(new ShaderResourceDescription("data/shaders/testsky.hlsl", "TestSky", device));
        }


        public override void Init(ResourceManager resourceManager)
        {
            Material pbrMat = new Rendering.Material(resourceManager.Shaders.GetFromAlias("PBR").Shader);
            Material uiMat = new Rendering.Material(resourceManager.Shaders.GetFromAlias("UI").Shader);

            pbrMat.AddTexture((int)Rendering.TextureMap.Albedo, resourceManager.Textures.GetFromAlias("Diffuse").Texture);
            pbrMat.AddTexture((int)Rendering.TextureMap.Normal, resourceManager.Textures.GetFromAlias("Normal").Texture);
            pbrMat.AddTexture((int)Rendering.TextureMap.Roughness, resourceManager.Textures.GetFromAlias("Roughness").Texture);
            pbrMat.AddTexture((int)Rendering.TextureMap.Metallic, resourceManager.Textures.GetFromAlias("Metallic").Texture);

            uiMat.AddTexture((int)Rendering.TextureMap.Normal, resourceManager.Textures.GetFromAlias("Normal").Texture);
            uiMat.AddTexture((int)Rendering.TextureMap.Albedo, resourceManager.Textures.GetFromAlias("Diffuse").Texture);
            uiMat.AddTexture((int)Rendering.TextureMap.Roughness, resourceManager.Textures.GetFromAlias("Roughness").Texture);
            uiMat.AddTexture((int)Rendering.TextureMap.Metallic, resourceManager.Textures.GetFromAlias("Metallic").Texture);


            Rendering.Model monkeyModel = new Rendering.Model(resourceManager.Meshes.GetFromAlias("Barrel").Mesh, pbrMat);
            Rendering.Model sphereModel = new Rendering.Model(resourceManager.Meshes.GetFromAlias("Sphere").Mesh, pbrMat);


            sphere.Components.Create<ComponentSystem._ModelComponent>();
            sphere.Components.Get<ComponentSystem._ModelComponent>().Model = sphereModel;

            testObject.Components.Create<ComponentSystem._ModelComponent>();
            testObject.Components.Get<ComponentSystem._ModelComponent>().Model = monkeyModel;

            testUI.Components.Create<ComponentSystem._ModelComponent>();
            testUI.Components.Get<ComponentSystem._ModelComponent>().Model = new Rendering.UI.UIRect(new RectangleF(3, 0, 256, 256), uiMat);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float speed = 50.0f;

            float angle = (speed * gameTime.Time) % 360.0f;

            testObject.SetEulerRotation(new Vector3(angle, angle, angle));
            sphere.SetEulerRotation(new Vector3(angle, angle, angle));

            if (Input.instance.GetKeyPressed(SharpDX.DirectInput.Key.O))
            {
                currentSkybox++;
                int index = currentSkybox % skyboxes.Count;
                stateScene.Skybox = skyboxes[index];
            }
        }
    }
}
