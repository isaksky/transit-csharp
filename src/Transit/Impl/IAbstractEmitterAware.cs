namespace Transit.Net.Impl;

/// <summary>
/// Interface for emitter-aware write handlers.
/// </summary>
internal interface IAbstractEmitterAware
{
    void SetEmitter(AbstractEmitter abstractEmitter);
}
