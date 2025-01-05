using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;

namespace StatusHud
{
    public class StatusHudBodyheatElement : StatusHudElement
    {
        public new const string name = "bodyheat";
        protected const string textKey = "shud-bodyheat";

        protected const float cfratio = 9f / 5f;
        public const float tempIdeal = 37;

        public static readonly string[] tempFormatWords = { "C", "F" };
        private string tempScale;

        public override string ElementOption => tempScale;
        public override string ElementName => name;

        public bool active;
        public int textureId;

        protected StatusHudBodyheatRenderer renderer;
        protected StatusHudConfig config;


        public StatusHudBodyheatElement(StatusHudSystem system, StatusHudConfig config) : base(system)
        {
            renderer = new StatusHudBodyheatRenderer(this.system, this, config);
            this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

            textureId = this.system.textures.texturesDict["empty"].TextureId;
            this.config = config;

            tempScale = "C";
            active = false;
        }

        public override StatusHudRenderer GetRenderer()
        {
            return renderer;
        }

        public virtual string GetTextKey()
        {
            return textKey;
        }

        public override void ConfigOptions(string value)
        {
            foreach (var words in tempFormatWords)
            {
                if (words == value)
                {
                    tempScale = value;
                }
            }
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
                string textRender = tempScale switch
                {
                    "F" => string.Format("{0:N1}", tempDiff * cfratio) + "°F",
                    _ => string.Format("{0:N1}", tempDiff) + "°C",
                };

                active = true;
                renderer.SetText(textRender);
            }
            else
            {
                if (active)
                {
                    renderer.SetText("");
                }

                active = false;
            }
            UpdateTexture(tempDiff);
        }

        public override void Dispose()
        {
            renderer.Dispose();
            system.capi.Event.UnregisterRenderer(renderer, EnumRenderStage.Ortho);
        }

        protected void UpdateTexture(float tempDiff)
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

        public StatusHudBodyheatRenderer(StatusHudSystem system, StatusHudBodyheatElement element, StatusHudConfig config) : base(system)
        {
            this.element = element;

            text = new StatusHudText(this.system.capi, this.element.GetTextKey(), config);
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
            text.Pos(pos);
        }

        protected override void Render()
        {
            if (!element.active)
            {
                if (system.ShowHidden)
                {
                    RenderHidden(system.textures.texturesDict["bodyheat"].TextureId);
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