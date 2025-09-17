using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StatusHud;

public class StatusHudTextures
{
    private readonly ICoreClientAPI capi;

    public StatusHudTextures(ICoreClientAPI capi, float size)
    {
        this.capi = capi;
        int size1 = (int)size;

        TexturesDict = new Dictionary<string, LoadedTexture>();

        // Generate empty texture.
        LoadedTexture empty = new(this.capi);
        ImageSurface surface = new(Format.Argb32, size1, size1);

        this.capi.Gui.LoadOrUpdateCairoTexture(surface, true, ref empty);
        surface.Dispose();

        TexturesDict.Add("empty", empty);

        // Generate ping texture.
        LoadedTexture ping = new(this.capi);
        surface = new ImageSurface(Format.Argb32, size1, size1);
        Context context = new(surface)
        {
            LineWidth = 2
        };

        context.SetSourceRGBA(0, 0, 0, 0.5);
        context.Rectangle(0, 0, size1, size1);
        context.Stroke();

        context.SetSourceRGBA(1, 1, 1, 1);
        context.Rectangle(context.LineWidth, context.LineWidth, size1 - context.LineWidth * 2, size1 - context.LineWidth * 2);
        context.Stroke();

        this.capi.Gui.LoadOrUpdateCairoTexture(surface, true, ref ping);
        context.Dispose();
        surface.Dispose();

        TexturesDict.Add("ping", ping);

        // Load Texture files
        LoadAllTextures();
    }

    public Dictionary<string, LoadedTexture> TexturesDict { get; }

    internal void Dispose()
    {
        foreach (var texture in TexturesDict)
        {
            texture.Value.Dispose();
        }
        TexturesDict.Clear();
    }

    internal void LoadAllTextures()
    {
        var assetLocations = capi.Assets.GetLocations("textures/", StatusHudSystem.Domain);

        foreach (AssetLocation asset in assetLocations)
        {
            // Get asset name without file extension
            string name = asset.GetName().Split('.')[0];

            if (TexturesDict.ContainsKey(name)) continue;

            LoadedTexture texture = new(capi);
            capi.Render.GetOrLoadTexture(asset, ref texture);
            TexturesDict.Add(name, texture);
        }
        assetLocations.Clear();
    }
}