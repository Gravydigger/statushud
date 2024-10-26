using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Config;

public class GuiDialogMoveable : GuiDialog
{
    public override string ToggleKeyCombinationCode => "statushudconfigselector";

    private bool moving = false;
    public Vec2i Pos = new();
    private ElementBounds bounds;

    public GuiDialogMoveable(ICoreClientAPI capi) : base(capi)
    {
        bounds = ElementBounds.Fixed(0, 0, 20, 20);

        SingleComposer = capi.Gui.CreateCompo("statushudconfigselector", bounds)
            .AddShadedDialogBG(ElementBounds.Fill, false)
            .Compose()
        ;
    }

    public override void OnMouseDown(MouseEvent args)
    {
        base.OnMouseDown(args);
        if (!args.Handled)
        {
            moving = true;
            Pos.Set(args.X, args.Y);
        }
    }

    public override void OnMouseMove(MouseEvent args)
    {
        base.OnMouseDown(args);
        if (moving)
        {
            bounds.fixedX += (float)(args.X - Pos.X) / RuntimeEnv.GUIScale;
            bounds.fixedY += (float)(args.Y - Pos.Y) / RuntimeEnv.GUIScale;
            Pos.Set(args.X, args.Y);
            bounds.CalcWorldBounds();
        }
    }

    public override void OnMouseUp(MouseEvent args)
    {
        base.OnMouseUp(args);
        if (!args.Handled)
        {
            moving = false;
        }
    }
}