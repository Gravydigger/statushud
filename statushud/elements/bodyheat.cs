using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StatusHud
{
    public class StatusHudBodyheatElement : StatusHudElement
    {
        public new const string name = "bodyheat";
        public new const string desc = "The 'bodyheat' element displays the player's body heat (in %). If at maximum, it is hidden.";
        protected const string textKey = "shud-bodyheat";

        public override string elementName => name;

        protected const float cfratio = (9f / 5f);

        public const float tempIdeal = 37;

        public bool active;
        public int textureId;

        protected StatusHudBodyheatRenderer renderer;
        protected StatusHudConfig config;


        public StatusHudBodyheatElement(StatusHudSystem system, int slot, StatusHudConfig config) : base(system, slot)
        {
            renderer = new StatusHudBodyheatRenderer(this.system, this.slot, this, config.text);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            textureId = this.system.textures.texturesDict["empty"].TextureId;
            this.config = config;

            active = false;
        }

        public override StatusHudRenderer getRenderer()
        {
            return renderer;
        }

        public virtual string getTextKey()
        {
            return textKey;
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

            // Heatstroke doesn't exists yet, only consider cold tempatures
            if (tempDiff <= -0.5f)
            {
                string textRender;
                switch (config.options.temperatureScale)
                {
                    case 'F':
                        textRender = string.Format("{0:N1}", tempDiff * cfratio) + "�F";
                        break;
                    case 'K':
                        textRender = string.Format("{0:N1}", tempDiff) + "�K";
                        break;
                    case 'C':
                    default:
                        textRender = string.Format("{0:N1}", tempDiff) + "�C";
                        break;
                }

                active = true;
                renderer.setText(textRender);
            }
            else
            {
                if (active)
                {
                    renderer.setText("");
                }

                active = false;
            }
            updateTexture(tempDiff);
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }

        protected void updateTexture(float tempDiff)
        {
            // If body temp ~33C, the player will start freezing
            if (tempDiff > -4)
            {
                textureId = system.textures.texturesDict["bodyheat"].TextureId;
            }
            else
            {
                textureId = system.textures.texturesDict["bodyheat_cold"].TextureId;
            }
        }
    }

    public class StatusHudBodyheatRenderer : StatusHudRenderer
    {
        protected StatusHudBodyheatElement element;

        protected StatusHudText text;

        public StatusHudBodyheatRenderer(StatusHudSystem system, int slot, StatusHudBodyheatElement element, StatusHudTextConfig config) : base(system, slot)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.slot, this.element.getTextKey(), config, this.system.textures.size);
        }

        public override void Reload(StatusHudTextConfig config)
        {
            text.ReloadText(config, pos);
        }

        public void setText(string value)
        {
            text.Set(value);
        }

        protected override void update()
        {
            base.update();
            text.Pos(pos);
        }

        protected override void render()
        {
            if (!element.active)
            {
                if (system.showHidden)
                {
                    renderHidden(system.textures.texturesDict["bodyheat"].TextureId);
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
}