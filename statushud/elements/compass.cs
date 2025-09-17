using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace StatusHud;

public class StatusHudCompassElement : StatusHudElement
{
    public const string Name = "compass";
    private const string textKey = "shud-compass";
    private readonly StatusHudCompassRenderer renderer;
    private string compassBearing;

    public StatusHudCompassElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudCompassRenderer(this.system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);

        compassBearing = "relative";

        // Config error checking
        if (!ElementOptionList.Any(str => str.Contains(compassBearing)))
        {
            system.capi.Logger.Warning(StatusHudSystem.PrintModName("[{0}] {1} is not a valid value for temperatureFormat. Defaulting to relative"), textKey,
                compassBearing);
        }
    }

    public sealed override string[] ElementOptionList => ["relative", "absolute"];
    public override string ElementName => Name;
    public override string ElementOption => compassBearing;

    public override StatusHudRenderer GetRenderer()
    {
        return renderer;
    }

    public static string GetTextKey()
    {
        return textKey;
    }

    public override void ConfigOptions(string value)
    {
        foreach (string option in ElementOptionList)
        {
            if (option == value)
            {
                compassBearing = value;
            }
        }
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

public class StatusHudCompassRenderer : StatusHudRenderer
{
    private readonly StatusHudCompassElement element;

    public StatusHudCompassRenderer(StatusHudSystem system, StatusHudCompassElement element) : base(system)
    {
        this.element = element;
        text = new StatusHudText(this.system.capi, StatusHudCompassElement.GetTextKey(), system.Config);
    }

    public override void Reload()
    {
        text.ReloadText(pos);
    }

    protected override void Update()
    {
        base.Update();
        text.SetPos(pos);
    }

    protected override void Render()
    {
        int direction = (Modulo((int)Math.Round(-system.capi.World.Player.CameraYaw * GameMath.RAD2DEG), 360) + 180) % 360;
        text.Set(direction + "°");

        system.capi.Render.RenderTexture(system.textures.TexturesDict["compass"].TextureId, x, y, w, h);

        IShaderProgram prog = system.capi.Render.GetEngineShader(EnumShaderProgram.Gui);
        prog.Uniform("rgbaIn", ColorUtil.WhiteArgbVec);
        prog.Uniform("extraGlow", 0);
        prog.Uniform("applyColor", 0);
        prog.Uniform("noTexture", 0f);
        prog.BindTexture2D("tex2d", system.textures.TexturesDict["compass_needle"].TextureId, 0);

        float angle = system.capi.World.Player.CameraYaw;

        if (element.ElementOption == "absolute")
        {
            // Show player's absolute direction instead of relation to north.
            angle = GameMath.PIHALF - angle;
        }
        else
        {
            angle += GameMath.PIHALF;
        }

        // Use hidden matrix and mesh because this element is never hidden.
        hiddenMatrix.Set(system.capi.Render.CurrentModelviewMatrix)
            .Translate(x + w / 2f, y + h / 2f, 50)
            .Scale(w, h, 0)
            .Scale(0.5f, 0.5f, 0)
            .RotateZ(angle);

        prog.UniformMatrix("projectionMatrix", system.capi.Render.CurrentProjectionMatrix);
        prog.UniformMatrix("modelViewMatrix", hiddenMatrix.Values);

        system.capi.Render.RenderMesh(hiddenMesh);
    }

    private static int Modulo(int n, int m)
    {
        return (n % m + m) % m;
    }

    public override void Dispose()
    {
        base.Dispose();
        text.Dispose();
    }
}