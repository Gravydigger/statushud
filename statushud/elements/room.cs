using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudRoomElement : StatusHudElement
{
    public const string Name = "room";
    private readonly StatusHudRoomRenderer renderer;

    private readonly IClientWorldAccessor world;
    internal Room room;

    public StatusHudRoomElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudRoomRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
        world = system.capi.World;
    }

    public override string ElementName => Name;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public override void Tick()
    {
        if (world.Player == null)
        {
            return;
        }
        room = world.Api.ModLoader.GetModSystem<RoomRegistry>().GetRoomForPosition(world.Player.Entity.Pos.AsBlockPos);
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
        // Intentionally left blank.
    }

    protected override void Render()
    {
        if (element.room is { ExitCount: 0 })
        {
            // Inside.
            Room room = element.room;

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