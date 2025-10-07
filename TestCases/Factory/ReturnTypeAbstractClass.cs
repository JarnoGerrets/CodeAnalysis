public abstract class GUIFactory1
{
    public abstract Button1 CreateButton();
}

public class FancyGUIFactory1 : GUIFactory1
{
    public override Button1 CreateButton() => new FancyButton1();
}

public abstract class Button1 { }
public class FancyButton1 : Button1 { }
