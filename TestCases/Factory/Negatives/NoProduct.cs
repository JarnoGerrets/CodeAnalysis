public abstract class ConnectionFactory
{
    public abstract string GetName();
}

public class SqlConnectionFactory : ConnectionFactory
{
    public override string GetName() => "SQL";
}
