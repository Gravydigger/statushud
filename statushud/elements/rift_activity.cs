using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudRiftActivityElement : StatusHudElement
{
    public const string Name = "riftactivity";
    private const string harmonyId = "shud-riftactivity";

    private static CurrentPattern _riftActivityData;
    public readonly bool active;
    private readonly Harmony harmony;
    private readonly StatusHudRiftActivityRenderer renderer;

    private readonly ModSystemRiftWeather riftSystem;
    private bool firstLoad;
    private string showRiftChange;

    public int textureId;

    public StatusHudRiftActivityElement(StatusHudSystem system) : base(system)
    {
        riftSystem = this.system.capi.ModLoader.GetModSystem<ModSystemRiftWeather>();

        renderer = new StatusHudRiftActivityRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        // When a player first activates this element when already in a world, the element hasn't gotten the rift data yet.
        // Until the player gets rift data, show the element with the unknown icon
        textureId = system.textures.TexturesDict["rift_unknown"].TextureId;

        active = this.system.capi.World.Config.GetString("temporalRifts") != "off";

        showRiftChange = "false";
        firstLoad = true;

        // World has to be reloaded for changes to apply
        harmony = new Harmony(harmonyId);
        harmony.Patch(typeof(ModSystemRiftWeather).GetMethod("onPacket", BindingFlags.Instance | BindingFlags.NonPublic),
            postfix: new HarmonyMethod(typeof(StatusHudRiftActivityElement).GetMethod(nameof(ReceiveData))));

        if (!active)
        {
            Dispose();
        }
    }

    public sealed override string[] ElementOptionList => ["true", "false"];
    public override string ElementName => Name;
    public override string ElementOption => showRiftChange;

    public static void ReceiveData(SpawnPatternPacket msg)
    {
        _riftActivityData = msg.Pattern;
    }

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void ConfigOptions(string value)
    {
        showRiftChange = value.ToBool() ? "true" : "false";
    }

    public override void Tick()
    {
        if (!active)
        {
            return;
        }

        if (riftSystem == null || _riftActivityData == null)
        {
            if (!firstLoad) return;

            string langName = Lang.Get("statushudcont:riftactivity-name");
            system.capi.ShowChatMessage(StatusHudSystem.PrintModName(Lang.Get("statushudcont:harmony-nodata", langName, langName.ToLower())));
            firstLoad = false;
            return;
        }

        if (showRiftChange.ToLower().ToBool())
        {
            double hours = system.capi.World.Calendar.TotalHours;
            double nextRiftChange = Math.Max(_riftActivityData.UntilTotalHours - hours, 0);

            TimeSpan ts = TimeSpan.FromHours(nextRiftChange);
            string text = (int)nextRiftChange + ":" + ts.ToString("mm");

            renderer.SetText(text);
        }
        else
        {
            renderer.SetText("");
        }

        UpdateTexture(_riftActivityData.Code);
    }

    public sealed override void Dispose()
    {
        harmony.UnpatchAll(harmonyId);

        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    private void UpdateTexture(string activity)
    {
        try
        {
            textureId = system.textures.TexturesDict["rift_" + activity].TextureId;
        }
        catch (KeyNotFoundException)
        {
            system.capi.Logger.Error("For {0} element, texture rift_{1} is not valid", Name, activity);
            throw;
        }
    }
}

public class StatusHudRiftActivityRenderer : StatusHudRenderer
{
    private const string textKey = "shud-riftactivity";
    private readonly StatusHudRiftActivityElement element;

    public StatusHudRiftActivityRenderer(StatusHudSystem system, StatusHudRiftActivityElement element) : base(system)
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
        if (element.active)
        {
            system.capi.Render.RenderTexture(element.textureId, x, y, w, h);
        }
        else if (showHidden)
        {
            RenderHidden(system.textures.TexturesDict["rift_calm"].TextureId);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}