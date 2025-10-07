public abstract class Dialog
{
    public abstract Button CreateButton();
}

public class WindowsDialog : Dialog
{
    public override Button CreateButton() => new WindowsButton();
}

public class WebDialog : Dialog
{
    public override Button CreateButton() => new HtmlButton();
}

public interface Button { }
public class WindowsButton : Button { }
public class HtmlButton : Button { }
