using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;

namespace StatusHud;

public class StatusHudBodyHeatElement : StatusHudElement
{
    public const string Name = "bodyheat";

    private const float cfratio = 9f / 5f;
    private const float tempIdeal = 37;

    private readonly StatusHudBodyHeatRenderer renderer;

    public bool active;
    private string tempScale;
    public int textureId;

    public StatusHudBodyHeatElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudBodyHeatRenderer(this.system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        textureId = system.textures.TexturesDict["empty"].TextureId;

        tempScale = "C";
        active = false;

        // Config error checking
        if (!ElementOptionList.Any(str => str.Contains(tempScale)))
        {
            system.capi.Logger.Warning(StatusHudSystem.PrintModName("[{0}] {1} is not a valid value for temperatureFormat. Defaulting to C"), Name,
                tempScale);
        }
    }

    public sealed override string[] ElementOptionList => ["C", "F"];
    public override string ElementOption => tempScale;
    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void ConfigOptions(string value)
    {
        foreach (string words in ElementOptionList)
        {
            if (words == value)
            {
                tempScale = value;
            }
        }
    }

    public override void Tick()
    {
        ITreeAttribute tempTree = system.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("bodyTemp");

        if (tempTree == null)
        {
            return;
        }

        float temp = tempTree.GetFloat("bodytemp");
        float tempDiff = temp - tempIdeal;

        // Heatstroke doesn't exist yet, only consider cold temperatures
        if (tempDiff <= -0.5f)
        {
            string textRender = tempScale switch
            {
                "F" => $"{tempDiff * cfratio:N1}" + "°F",
                _ => $"{tempDiff:N1}" + "°C"
            };

            active = true;
            renderer.SetText(textRender);
        }
        else
        {
            if (active)
            {
                renderer.SetText("");
            }

            active = false;
        }
        UpdateTexture(tempDiff);
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    private void UpdateTexture(float tempDiff)
    {
        // If body temp ~33C, the player will start freezing
        textureId = tempDiff > -4
            ? system.textures.TexturesDict["bodyheat"].TextureId
            : system.textures.TexturesDict["bodyheat_cold"].TextureId;
    }
}

public class StatusHudBodyHeatRenderer : StatusHudRenderer
{
    private const string textKey = "shud-bodyheat";
    private readonly StatusHudBodyHeatElement element;

    public StatusHudBodyHeatRenderer(StatusHudSystem system, StatusHudBodyHeatElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, textKey, system.Config);
    }

    public override void Reload()
    {
        text.ReloadText(pos);
    }

    public void SetText(string value)
    {
        text.Set(value);
    }

    protected override void Update()
    {
        base.Update();
        text.SetPos(pos);
    }

    protected override void Render()
    {
        if (!element.active)
        {
            if (showHidden)
            {
                RenderHidden(system.textures.TexturesDict["bodyheat"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(element.textureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}