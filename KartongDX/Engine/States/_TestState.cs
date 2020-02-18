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

        private GameObject groundPlane;

        private List<Skybox> skyboxes;
        private int currentSkybox = 0;

        private GameObject cameraHandle;
        public _TestState(Accessors accessors) : base("TestState", accessors)
        {
            testObject = new GameObject("testObject", 0);
            testObject.SetPosition(new SharpDX.Vector3(0, 0, -4));

            sphere = new GameObject("sphere", 0);
            sphere.SetPosition(new Vector3(0, 0, 0));
            sphere.Name = "Sphere";

            testUI = new GameObject("testUI", 0);


            groundPlane = new GameObject("groundPlane", 0);
            stateScene.AddObjectAsRootChild(groundPlane);
            groundPlane.SetPosition(new Vector3(0, -1, 0));
            groundPlane.SetEulerRotation(new Vector3(0, 0, 90));
            groundPlane.SetScale(new Vector3(10, 10, 10));



            //stateScene.AddObjectAsRootChild(testObject);
            stateScene.AddObjectAsRootChild(sphere);
            uiScene.AddObjectAsRootChild(testUI);

            cameraHandle = new GameObject("cameraHandle", 0);
            cameraHandle.AddChild(stateScene.Camera);
            cameraHandle.Components.Create<ComponentSystem._FPSCameraComponent>();
            cameraHandle.SetPosition(new Vector3(0, 0, -7));

            stateScene.AddObjectAsRootChild(cameraHandle);

            sphere.AddChild(testObject);
            //testObject.SetParent(sphere);
        }

        // TODO: States should not access renderer.
        // inject device in resource manager state if needed?
        // ResourceManager should be accessed by Accessor
        protected override void LoadResources(ResourceManager resourceManager, SharpDX.Direct3D11.Device device)
        {
            resourceManager.Textures.Load(new Texture2DResourceDesc("sky/misty_pines_4k.png", "SkyPines", device));
            resourceManager.Textures.Load(new Texture2DResourceDesc("sky/output_sky.png", "SkyFac", device));
            resourceManager.Textures.Load(new Texture2DResourceDesc("sky/hdri.png", "SkyGrass", device));
            resourceManager.Textures.Load(new Texture2DResourceDesc("sky/fireplace_4k.png", "SkyFire", device));

            resourceManager.Shaders.Load(new ShaderResourceDesc("skybox.hlsl", "Sky", device));


            // TODO: Remove this. Placed here for quick testing purposes.
            Rendering.CubemapRenderer cubemapRenderer = new CubemapRenderer(device, resourceManager);

            var pineCubemap = cubemapRenderer.GenerateFromEquirectangular(resourceManager.Textures.GetFromAlias("SkyPines").Texture.Texture2D);
            var factoryCubemap = cubemapRenderer.GenerateFromEquirectangular(resourceManager.Textures.GetFromAlias("SkyFac").Texture.Texture2D);
            var grassCubemap = cubemapRenderer.GenerateFromEquirectangular(resourceManager.Textures.GetFromAlias("SkyGrass").Texture.Texture2D);
            var testCubemap = cubemapRenderer.GenerateFromEquirectangular(resourceManager.Textures.GetFromAlias("SkyFire").Texture.Texture2D);

            Mesh cubeMesh = resourceManager.Meshes.GetFromAlias("KDX_CUBE_IN").Mesh;
            Shader skyShader = resourceManager.Shaders.GetFromAlias("Sky").Shader;
            
            Skybox pineSkybox = new Skybox(pineCubemap, cubeMesh, skyShader);
            Skybox grassSkybox = new Skybox(grassCubemap, cubeMesh, skyShader);
            Skybox indoorSkybox = new Skybox(factoryCubemap, cubeMesh, skyShader);
            Skybox testSkybox = new Skybox(testCubemap, cubeMesh, skyShader);



            // TODO: Remove later. This is only for switching skyboxes at the fly for testing.
            skyboxes = new List<Skybox>();
            skyboxes.Add(pineSkybox);
            skyboxes.Add(testSkybox);
            skyboxes.Add(indoorSkybox);
            skyboxes.Add(grassSkybox);

            stateScene.SetSkybox(skyboxes[0]);

            resourceManager.Textures.Load(new Texture2DResourceDesc("test/RedOctFloor_basecolor.png", "Diffuse", device));
            resourceManager.Textures.Load(new Texture2DResourceDesc("test/RedOctFloor_normal.png", "Normal", device));
            resourceManager.Textures.Load(new Texture2DResourceDesc("test/RedOctFloor_roughness.png", "Roughness", device));
            resourceManager.Textures.Load(new Texture2DResourceDesc("test/RedOctFloor_metallic.png", "Metallic", device));
            resourceManager.Textures.Load(new Texture2DResourceDesc("plain_normal.png", "PlainNormal", device));


            resourceManager.Meshes.Load(new MeshResourceDesc("plane.fbx", "Plane"));
            resourceManager.Meshes.Load(new MeshResourceDesc("monkey_high.fbx", "MonkeyHigh"));
            resourceManager.Meshes.Load(new MeshResourceDesc("monkey_smooth.fbx", "Monkey"));
            resourceManager.Meshes.Load(new MeshResourceDesc("barrel.fbx", "Barrel"));
            resourceManager.Meshes.Load(new MeshResourceDesc("sphere_smooth.fbx", "Sphere"));

            resourceManager.Shaders.Load(new ShaderResourceDesc("testShader_PBR.hlsl", "PBR", device));
            resourceManager.Shaders.Load(new ShaderResourceDesc("ui.hlsl", "UI", device));


            resourceManager.Textures.Load(new Texture2DResourceDesc("StandardCubeMap.png", "Sky", device));
            resourceManager.Textures.Load(new Texture2DResourceDesc("hdri.png", "SkyHDRI", device));

            resourceManager.Meshes.Load(new MeshResourceDesc("skybox.fbx", "Sky"));

            resourceManager.Shaders.Load(new ShaderResourceDesc("testsky.hlsl", "TestSky", device));
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
            Rendering.Model planeModel = new Rendering.Model(resourceManager.Meshes.GetFromAlias("Plane").Mesh, pbrMat);


            sphere.Components.Create<ComponentSystem._ModelComponent>();
            sphere.Components.Get<ComponentSystem._ModelComponent>().Model = sphereModel;

            testObject.Components.Create<ComponentSystem._ModelComponent>();
            testObject.Components.Get<ComponentSystem._ModelComponent>().Model = monkeyModel;

            testUI.Components.Create<ComponentSystem._ModelComponent>();
            testUI.Components.Get<ComponentSystem._ModelComponent>().Model = new Rendering.UI.UIRect(new RectangleF(3, 0, 256, 256), uiMat);

            groundPlane.Components.Create<ComponentSystem._ModelComponent>();
            groundPlane.Components.Get<ComponentSystem._ModelComponent>().Model = planeModel;
        }

        public override void Update()
        {
            base.Update();

            float speed = 50.0f;

            float angle = (speed * Accessors.GameTime.Time) % 360.0f;

            //testObject.SetEulerRotation(new Vector3(angle, angle, angle));
            sphere.SetEulerRotation(new Vector3(0, angle, 0));

            if (Accessors.Input.GetKeyPressed(SharpDX.DirectInput.Key.O))
            {
                currentSkybox++;
                int index = currentSkybox % skyboxes.Count;
                stateScene.SetSkybox(skyboxes[index]);
            }
        }
    }
}
