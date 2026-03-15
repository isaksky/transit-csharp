namespace Transit.Impl.WriteHandlers;

internal abstract class AbstractWriteHandler : IWriteHandler
{
    public abstract string Tag(object obj);
    public abstract object Representation(object obj);
    public virtual string? StringRepresentation(object obj) => null;
    public virtual IWriteHandler? GetVerboseHandler() => null;
}
