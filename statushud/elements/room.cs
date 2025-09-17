using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudRoomElement : StatusHudElement
{
    public const string Name = "room";
    private const string textKey = "shud-room";

    private readonly StatusHudRoomRenderer renderer;
    public bool cellar;
    public bool greenhouse;

    public bool inside;

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

    public virtual string GetTextKey()
    {
        return textKey;
    }

    public override void Tick()
    {
        EntityPlayer entity = system.capi.World.Player.Entity;
        if (entity == null)
        {
            inside = false;
            cellar = false;
            greenhouse = false;
            return;
        }

        Room room = entity.World.Api.ModLoader.GetModSystem<RoomRegistry>().GetRoomForPosition(entity.Pos.AsBlockPos);
        if (room == null)
        {
            inside = false;
            cellar = false;
            greenhouse = false;
            return;
        }

        if (room.ExitCount == 0)
        {
            // Inside.
            inside = true;
            cellar = room.IsSmallRoom;
            greenhouse = room.SkylightCount > room.NonSkylightCount; // No room flag avaiable, based on FruitTreeRootBH.
        }
        else
        {
            // Outside.
            inside = false;
            cellar = false;
            greenhouse = false;
        }
    }

    public override void Dispose()
    {
        renderer.Dispose();
        system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
    }
}

public class StatusHudRoomRenderer : StatusHudRenderer
{
    private readonly StatusHudRoomElement element;

    private float ghy;

    public StatusHudRoomRenderer(StatusHudSystem system, StatusHudRoomElement element) : base(system)
    {
        this.element = element;
    }

    public override void Reload()
    {
    }

    protected override void Render()
    {
        if (!element.inside)
        {
            if (showHidden)
            {
                RenderHidden(system.textures.TexturesDict["room_room"].TextureId);
            }
            return;
        }

        system.capi.Render.RenderTexture(
            element.cellar ? system.textures.TexturesDict["room_cellar"].TextureId : system.textures.TexturesDict["room_room"].TextureId, x,
            y, w, h);

        if (element.greenhouse)
        {
            system.capi.Render.RenderTexture(system.textures.TexturesDict["room_greenhouse"].TextureId, x, ghy, w, h);
        }
    }

    protected override void Update()
    {
        base.Update();

        ghy = (float)(y - GuiElement.scaled(StatusHudSystem.IconSize * system.Config.elementScale));
    }
}