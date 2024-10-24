using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Common;

namespace StatusHud
{
    public class StatusHudTextures
    {
        private int size;
        protected ICoreClientAPI capi;
        public Dictionary<string, LoadedTexture> texturesDict;

        public StatusHudTextures(ICoreClientAPI capi, int size)
        {
            this.capi = capi;
            this.size = size;

            texturesDict = new Dictionary<string, LoadedTexture>();

            ImageSurface surface;
            Context context;

            // Generate empty texture.
            LoadedTexture empty = new LoadedTexture(this.capi);
            surface = new ImageSurface(Format.Argb32, this.size, this.size);

            this.capi.Gui.LoadOrUpdateCairoTexture(surface, true, ref empty);
            surface.Dispose();
            
            texturesDict.Add("empty", empty);

            // Generate ping texture.
            LoadedTexture ping = new LoadedTexture(this.capi);
            surface = new ImageSurface(Format.Argb32, this.size, this.size);
            context = new Context(surface);

            context.LineWidth = 2;

            context.SetSourceRGBA(0, 0, 0, 0.5);
            context.Rectangle(0, 0, this.size, this.size);
            context.Stroke();

            context.SetSourceRGBA(1, 1, 1, 1);
            context.Rectangle(context.LineWidth, context.LineWidth, this.size - (context.LineWidth * 2), this.size - (context.LineWidth * 2));
            context.Stroke();

            this.capi.Gui.LoadOrUpdateCairoTexture(surface, true, ref ping);
            context.Dispose();
            surface.Dispose();

            texturesDict.Add("ping", ping);

            // Load Texture files
            LoadAllTextures();
        }

        public void Dispose()
        {
            foreach (var texture in texturesDict)
            {
                texture.Value.Dispose();
            }
            texturesDict.Clear();
        }

        protected void LoadAllTextures()
        {
            List<AssetLocation> assetLocations = capi.Assets.GetLocations("textures/", StatusHudSystem.domain);

            foreach (var asset in assetLocations)
            {
                LoadedTexture texture = new LoadedTexture(capi);

                // Get asset name without file extension
                string name = asset.GetName().Split('.')[0];

                capi.Render.GetOrLoadTexture(asset, ref texture);
                texturesDict.Add(name, texture);
            }
            assetLocations.Clear();
        }
    }
}