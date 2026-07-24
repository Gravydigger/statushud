using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace StatusHud;

public class StatusHudDistanceElement : StatusHudElement
{
    public const string Name = "distance";

    private readonly StatusHudDistanceRenderer renderer;

    public bool active;

    public StatusHudDistanceElement(StatusHudSystem system) : base(system, true)
    {
        renderer = new StatusHudDistanceRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
        IClientWorldAccessor world = system.capi.World;
        EntityPlayer playerEntity = world.Player.Entity;

        BlockSelection blockSelection = new();
        EntitySelection entitySelection = new();

        // Same as creative: https://github.com/anegostudios/vscreativemod/blob/master/Core.cs#L64
        const float pickingRange = 100f;

        // We can't use `Player.CurrentBlockSelection` as we'd be restricted to 4.5 blocks in survival mode.
        // Instead, use the same code to calculate it.
        system.capi.World.RayTraceForSelection(playerEntity.Pos.XYZ.Add(playerEntity.LocalEyePos),
            playerEntity.Pos.Pitch, playerEntity.Pos.Yaw, pickingRange, ref blockSelection, ref entitySelection);

        if (blockSelection != null || entitySelection != null)
        {
            // Prefer block over entity.
            BlockPos target = blockSelection == null ? entitySelection.Position.AsBlockPos : blockSelection.Position;
            BlockPos distance = playerEntity.Pos.AsBlockPos - target;

            renderer.SetText($"{Math.Abs(distance.X)}/{Math.Abs(distance.Y)}/{Math.Abs(distance.Z)}");
            active = true;
        }
        else
        {
            if (!active) return;
            renderer.SetText("");
            active = false;
        }
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudDistanceRenderer : StatusHudRenderer
{
    private const string textKey = "shud-distance";
    private readonly StatusHudDistanceElement element;

    public StatusHudDistanceRenderer(StatusHudSystem system, StatusHudDistanceElement element) : base(system)
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
                RenderHidden(system.textures.TexturesDict["distance"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(system.textures.TexturesDict["distance"].TextureId, x, y, w, h);
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}