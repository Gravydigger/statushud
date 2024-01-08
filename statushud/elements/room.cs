using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudRoomElement : StatusHudElement
    {
        public new const string name = "room";
        public new const string desc = "The 'room' element displays a house icon when inside a room or a cabin icon when inside a small room (cellar), and a sun icon when inside a greenhouse. Otherwise, it is hidden.";
        protected const string textKey = "shud-room";

        public override string elementName => name;

        public bool inside;
        public bool cellar;
        public bool greenhouse;

        protected StatusHudRoomRenderer renderer;

        public StatusHudRoomElement(StatusHudSystem system, int slot) : base(system, slot)
        {
            this.renderer = new StatusHudRoomRenderer(system, slot, this);
            this.system.capi.Event.RegisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }

        public override StatusHudRenderer getRenderer()
        {
            return this.renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
        }

        public override void Tick()
        {
            EntityPlayer entity = this.system.capi.World.Player.Entity;
            if (entity == null)
            {
                this.inside = false;
                this.cellar = false;
                this.greenhouse = false;
                return;
            }

            Room room = entity.World.Api.ModLoader.GetModSystem<RoomRegistry>().GetRoomForPosition(entity.Pos.AsBlockPos);
            if (room == null)
            {
                this.inside = false;
                this.cellar = false;
                this.greenhouse = false;
                return;
            }

            if (room.ExitCount == 0)
            {
                // Inside.
                this.inside = true;
                this.cellar = room.IsSmallRoom;
                this.greenhouse = room.SkylightCount > room.NonSkylightCount;   // No room flag avaiable, based on FruitTreeRootBH.
            }
            else
            {
                // Outside.
                this.inside = false;
                this.cellar = false;
                this.greenhouse = false;
            }
        }

        public override void Dispose()
        {
            this.renderer.Dispose();
            this.system.capi.Event.UnregisterRenderer(this.renderer, EnumRenderStage.Ortho);
        }
    }

    public class StatusHudRoomRenderer : StatusHudRenderer
    {
        protected StatusHudRoomElement element;

        protected float ghy;

        public StatusHudRoomRenderer(StatusHudSystem system, int slot, StatusHudRoomElement element) : base(system, slot)
        {
            this.element = element;
        }

        public override void Reload(StatusHudTextConfig config) { }

        protected override void render()
        {
            if (!this.element.inside)
            {
                if (this.system.showHidden)
                {
                    this.renderHidden(this.system.textures.roomRoom.TextureId);
                }
                return;
            }

            this.system.capi.Render.RenderTexture(this.element.cellar ? this.system.textures.roomCellar.TextureId : this.system.textures.roomRoom.TextureId, this.x, this.y, this.w, this.h);

            if (this.element.greenhouse)
            {
                this.system.capi.Render.RenderTexture(this.system.textures.roomGreenhouse.TextureId, this.x, this.ghy, this.w, this.h);
            }
        }

        protected override void update()
        {
            base.update();

            this.ghy = (float)(this.y - GuiElement.scaled(this.system.textures.size));
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}