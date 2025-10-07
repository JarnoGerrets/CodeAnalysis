public abstract class GuiFactory {
    public abstract Button CreateButton();
}

public abstract class WindowsGuiFactory : GuiFactory {
    // may provide some default or extra setup
}

public class WindowsDialogFactory : WindowsGuiFactory {
    public override Button CreateButton() => new WindowsButton();
}
