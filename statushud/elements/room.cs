using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudRoomElement : StatusHudElement
{
    public const string Name = "room";
    private readonly StatusHudRoomRenderer renderer;

    public StatusHudRoomElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudRoomRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudRoomRenderer : StatusHudRenderer
{
    private float ghy;

    public StatusHudRoomRenderer(StatusHudSystem system, StatusHudRoomElement element) : base(system)
    {
    }

    public override void Reload()
    {
    }

    protected override void Render()
    {
        Room room;
        try
        {
            EntityPlayer entity = system.capi.World.Player.Entity;
            room = entity.World.Api.ModLoader.GetModSystem<RoomRegistry>().GetRoomForPosition(entity.Pos.AsBlockPos);
        }
        catch (Exception e)
        {
            if (showHidden)
            {
                RenderHidden(system.textures.TexturesDict["room_room"].TextureId);
            }
            return;
        }
        
        if (room.ExitCount == 0)
        {
            // Inside.
            system.capi.Render.RenderTexture(
                room.IsSmallRoom ? system.textures.TexturesDict["room_cellar"].TextureId : system.textures.TexturesDict["room_room"].TextureId, x,
                y, w, h);
            
            // No room flag available, based on FruitTreeRootBH.
            if (room.SkylightCount > room.NonSkylightCount)
            {
                system.capi.Render.RenderTexture(system.textures.TexturesDict["room_greenhouse"].TextureId, x, ghy, w, h);
            } 
            
        }
        else
        {
            // Outside.
            if (showHidden)
            {
                RenderHidden(system.textures.TexturesDict["room_room"].TextureId);
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        ghy = (float)(y - GuiElement.scaled(StatusHudSystem.IconSize * system.Config.elementScale));
    }
}