using System;
using System.Threading;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StatusHud;

public class StatusHudRoomElement : StatusHudElement
{
    public const string Name = "room";
    private readonly StatusHudRoomRenderer renderer;

    private readonly IClientWorldAccessor world;
    private readonly RoomRegistry roomRegistry;
    private readonly Thread roomUpdateThread;
    private volatile bool isRunning;

    private BlockPos currentPosition;

    // Thread-safe room state flags using volatile for atomic reads/writes
    private volatile bool _inRoom;
    private volatile bool _isSmallRoom;
    private volatile bool _isGreenhouse;

    internal bool InRoom => _inRoom;
    internal bool IsSmallRoom => _isSmallRoom;
    internal bool IsGreenhouse => _isGreenhouse;

    public StatusHudRoomElement(StatusHudSystem system) : base(system)
    {
        renderer = new StatusHudRoomRenderer(system, this);
        this.system.capi.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
        world = system.capi.World;
        roomRegistry = world.Api.ModLoader.GetModSystem<RoomRegistry>();

        // Start background thread for room updates
        isRunning = true;
        roomUpdateThread = new Thread(RoomUpdateLoop)
        {
            Name = "StatusHud-RoomUpdate",
            IsBackground = true
        };
        roomUpdateThread.Start();
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

        // Just update the position to check
        currentPosition = world.Player.Entity.Pos.AsBlockPos;
    }

    private void RoomUpdateLoop()
    {
        try
        {
            while (isRunning)
            {
                BlockPos pos = currentPosition;
                if (pos != null)
                {
                    Room room = roomRegistry.GetRoomForPosition(pos);

                    // Atomic writes via volatile fields
                    _inRoom = room?.ExitCount == 0;
                    _isSmallRoom = room?.IsSmallRoom == true;
                    _isGreenhouse = room?.SkylightCount > room?.NonSkylightCount;
                }

                // Update every 100ms
                Thread.Sleep(100);
            }
        }
        catch (ThreadInterruptedException)
        {
            // Thread was interrupted, exit gracefully
        }
        catch (Exception ex)
        {
            system.capi.Logger.Error($"[StatusHud] Room update thread error: {ex}");
        }
    }

    public override void Dispose()
    {
        isRunning = false;

        // Wait for thread to stop (with timeout)
        if (roomUpdateThread?.IsAlive == true)
        {
            roomUpdateThread.Interrupt();
            if (!roomUpdateThread.Join(TimeSpan.FromSeconds(2)))
            {
                system.capi.Logger.Warning("[StatusHud] Room update thread did not stop in time");
            }
        }

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
        if (element.InRoom)
        {
            // Inside.
            system.capi.Render.RenderTexture(
                element.IsSmallRoom ? system.textures.TexturesDict["room_cellar"].TextureId : system.textures.TexturesDict["room_room"].TextureId, x,
                y, w, h);

            // No room flag available, based on FruitTreeRootBH.
            if (element.IsGreenhouse)
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