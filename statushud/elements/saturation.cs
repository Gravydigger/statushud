using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudSaturationElement : StatusHudElement
{
    public const string Name = "saturation";
    private const string harmonyId = "shud-saturation";

    private static float _satLossMultiplier;
    private readonly Harmony harmony;
    private readonly StatusHudSaturationRenderer renderer;

    public bool active;
    private float prevSat;
    private float satDelaySec;

    public StatusHudSaturationElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudSaturationRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        active = false;

        harmony = new Harmony(harmonyId);

        harmony.Patch(
            typeof(EntityBehaviorHunger).GetMethod("ReduceSaturation", BindingFlags.Instance | BindingFlags.NonPublic),
            postfix: new HarmonyMethod(typeof(StatusHudSaturationElement).GetMethod(nameof(GetSatLoss))));
    }

    public override string ElementName => Name;

    public static void GetSatLoss(float satLossMultiplier)
    {
        _satLossMultiplier = satLossMultiplier;
    }

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
        ITreeAttribute hungerTree = system.capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("hunger");

        if (hungerTree == null) return;

        float sat = 0;

        float? satLossFruit = hungerTree.TryGetFloat("saturationlossdelayfruit");
        if (satLossFruit != null)
        {
            sat = (float)satLossFruit;
        }

        float? satLossVeg = hungerTree.TryGetFloat("saturationlossdelayvegetable");
        if (satLossVeg != null)
        {
            sat = Math.Max(sat, (float)satLossVeg);
        }

        float? satLossProtein = hungerTree.TryGetFloat("saturationlossdelayprotein");
        if (satLossProtein != null)
        {
            sat = Math.Max(sat, (float)satLossProtein);
        }

        float? satLossGrain = hungerTree.TryGetFloat("saturationlossdelaygrain");
        if (satLossGrain != null)
        {
            sat = Math.Max(sat, (float)satLossGrain);
        }

        float? satLossDairy = hungerTree.TryGetFloat("saturationlossdelaydairy");
        if (satLossDairy != null)
        {
            sat = Math.Max(sat, (float)satLossDairy);
        }

        if (sat != prevSat)
        {
            satDelaySec = sat;
            prevSat = sat;
        }

        if (sat > 0 && satDelaySec > 0)
        {
            satDelaySec -= _satLossMultiplier;
            active = true;

            renderer.SetText($"{Math.Round(satDelaySec, 1)}s");
        }
        else
        {
            renderer.SetText("");
            active = false;
        }
    }

    public override void Dispose()
    {
        harmony.UnpatchAll(harmonyId);

        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudSaturationRenderer : StatusHudRenderer
{
    private const string textKey = "shud-saturation";
    private readonly StatusHudSaturationElement element;

    public StatusHudSaturationRenderer(StatusHudSystem system, StatusHudSaturationElement element) : base(system)
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
                RenderHidden(system.textures.TexturesDict["light"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(system.textures.TexturesDict["light"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}