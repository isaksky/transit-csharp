namespace Transit.Impl;

/// <summary>
/// Internal emitter interface.
/// </summary>
internal interface IEmitter
{
    void Emit(object obj, bool asDictionaryKey, WriteCache cache);
}
