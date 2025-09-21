using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudTempstormElement : StatusHudElement
{
    public const string Name = "tempstorm";
    private const string harmonyId = "shud-tempstorm";

    // Hard-coded values from SystemTemporalStability.
    private const double approachingThreshold = 0.35;
    // private const double imminentThreshold = 0.02;
    // private const double waningThreshold = 0.02;

    private static TemporalStormRunTimeData _data;
    private readonly Harmony harmony;
    private readonly StatusHudTempstormRenderer renderer;

    private readonly SystemTemporalStability stabilitySystem;

    public bool active;
    private bool firstLoad;
    public int textureId;

    public StatusHudTempstormElement(StatusHudSystem system) : base(system)
    {
        stabilitySystem = this.system.capi.ModLoader.GetModSystem<SystemTemporalStability>();

        renderer = new StatusHudTempstormRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        active = false;
        firstLoad = true;
        textureId = system.textures.TexturesDict["empty"].TextureId;

        if (stabilitySystem == null) return;

        harmony = new Harmony(harmonyId);

        harmony.Patch(typeof(SystemTemporalStability).GetMethod("onServerData", BindingFlags.Instance | BindingFlags.NonPublic),
            postfix: new HarmonyMethod(typeof(StatusHudTempstormElement).GetMethod(nameof(ReceiveData))));
    }

    public override string ElementName => Name;

    public static void ReceiveData(TemporalStormRunTimeData data)
    {
        _data = data;
    }

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
        if (stabilitySystem == null)
        {
            return;
        }

        if (_data == null)
        {
            if (!firstLoad) return;

            string langName = Lang.Get("statushudcont:tempstorm-name");
            system.capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get("statushudcont:harmony-nodata", langName, langName.ToLower())));
            firstLoad = false;
            return;
        }

        double nextStormDaysLeft = _data.nextStormTotalDays - system.capi.World.Calendar.TotalDays;

        if (nextStormDaysLeft is > 0 and < approachingThreshold)
        {
            // Preparing.
            float hoursLeft = (float)((_data.nextStormTotalDays - system.capi.World.Calendar.TotalDays) * system.capi.World.Calendar.HoursPerDay);

            active = true;
            textureId = system.textures.TexturesDict["tempstorm_incoming"].TextureId;

            TimeSpan ts = TimeSpan.FromHours(Math.Max(hoursLeft, 0));
            renderer.SetText(ts.ToString("h':'mm"));
        }
        else
        {
            // In progress.
            if (_data.nowStormActive)
            {
                // Active.
                double hoursLeft = (_data.stormActiveTotalDays - system.capi.World.Calendar.TotalDays) * system.capi.World.Calendar.HoursPerDay;

                active = true;
                textureId = system.textures.TexturesDict["tempstorm_duration"].TextureId;

                TimeSpan ts = TimeSpan.FromHours(Math.Max(hoursLeft, 0));
                renderer.SetText(ts.ToString("h':'mm"));
            }
            else if (active)
            {
                // Ending.
                active = false;
                textureId = system.textures.TexturesDict["empty"].TextureId;

                renderer.SetText("");
            }
        }
    }

    public override void Dispose()
    {
        harmony.UnpatchAll(harmonyId);

        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudTempstormRenderer : StatusHudRenderer
{
    private const string textKey = "shud-tempstorm";
    private readonly StatusHudTempstormElement element;

    public StatusHudTempstormRenderer(StatusHudSystem system, StatusHudTempstormElement element) : base(system)
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
                RenderHidden(system.textures.TexturesDict["tempstorm_incoming"].TextureId);
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