using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudRoomElement : StatusHudElement
    {
        public new const string name = "room";
        protected const string textKey = "shud-room";

        public override string ElementName => name;

        public bool inside;
        public bool cellar;
        public bool greenhouse;

        protected StatusHudRoomRenderer renderer;

        public StatusHudRoomElement(StatusHudSystem system) : base(system)
        {
            renderer = new StatusHudRoomRenderer(system, this);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
        }

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
                greenhouse = room.SkylightCount > room.NonSkylightCount;   // No room flag avaiable, based on FruitTreeRootBH.
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
        protected StatusHudRoomElement element;

        protected float ghy;

        public StatusHudRoomRenderer(StatusHudSystem system, StatusHudRoomElement element) : base(system)
        {
            this.element = element;
        }

        public override void Reload() { }

        protected override void Render()
        {
            if (!element.inside)
            {
                if (system.ShowHidden)
                {
                    this.RenderHidden(system.textures.texturesDict["room_room"].TextureId);
                }
                return;
            }

            system.capi.Render.RenderTexture(element.cellar ? system.textures.texturesDict["room_cellar"].TextureId : system.textures.texturesDict["room_room"].TextureId, x, y, w, h);

            if (element.greenhouse)
            {
                system.capi.Render.RenderTexture(system.textures.texturesDict["room_greenhouse"].TextureId, x, ghy, w, h);
            }
        }

        protected override void Update()
        {
            base.Update();

            ghy = (float)(y - GuiElement.scaled(system.Config.iconSize));
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}