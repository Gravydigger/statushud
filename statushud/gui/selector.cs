using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.API.Config;
using StatusHud;

public class GuiDialogMoveable : GuiDialog
{
    public override string ToggleKeyCombinationCode => "statushudconfigselector";

    private bool moving = false;
    private Vec2i Pos = new();
    private ElementBounds bounds;
    private StatusHudElement selectedEelement;

    public GuiDialogMoveable(ICoreClientAPI capi) : base(capi)
    {
        bounds = ElementBounds.Fixed(0, 0, 20, 20);

        SingleComposer = capi.Gui.CreateCompo("statushudconfigselector", bounds)
            .AddShadedDialogBG(ElementBounds.Fill, false)
            .Compose()
        ;
    }

    public void UpdateSelectedElement(StatusHudElement element)
    {
        selectedEelement = element;
        Pos.Set((int)selectedEelement.GetRenderer().X, (int)selectedEelement.GetRenderer().Y);
        bounds.WithFixedPosition(Pos.X, Pos.Y);
        SingleComposer.ReCompose();
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
            bounds.fixedX += (args.X - Pos.X) / RuntimeEnv.GUIScale;
            bounds.fixedY += (args.Y - Pos.Y) / RuntimeEnv.GUIScale;
            Pos.Set(args.X, args.Y);
            bounds.CalcWorldBounds();

            selectedEelement.pos.x = Pos.X;
            selectedEelement.pos.y = Pos.Y;
            selectedEelement.Pos();
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