using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Config;

public class GuiDialogMoveable : GuiDialog
{
    public override string ToggleKeyCombinationCode => "statushudconfigselector";

    private bool moving = false;
    private Vec2i movingStartPos = new();
    private ElementBounds bounds;

    public GuiDialogMoveable(ICoreClientAPI capi) : base(capi)
    {
        bounds = ElementBounds.Fixed(30, 30, 20, 20);

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
            movingStartPos.Set(args.X, args.Y);
        }
    }

    public override void OnMouseMove(MouseEvent args)
    {
        base.OnMouseDown(args);
        if (moving)
        {
            bounds.fixedX += (float)(args.X - movingStartPos.X) / RuntimeEnv.GUIScale;
            bounds.fixedY += (float)(args.Y - movingStartPos.Y) / RuntimeEnv.GUIScale;
            movingStartPos.Set(args.X, args.Y);
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