namespace Transit;

/// <summary>
/// Specifies the settings to use with <see cref="TransitConvert"/>.
/// </summary>
public class TransitSerializerSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether to use keywords for dictionary keys when serializing objects.
    /// Default is false (uses strings).
    /// </summary>
    public bool UseKeywordKeys { get; set; } = false;

    /// <summary>
    /// Gets or sets the custom write handlers.
    /// </summary>
    public IDictionary<Type, IWriteHandler>? WriteHandlers { get; set; }

    /// <summary>
    /// Gets or sets the custom read handlers.
    /// </summary>
    public IDictionary<string, IReadHandler>? ReadHandlers { get; set; }

    /// <summary>
    /// Gets or sets the default write handler.
    /// </summary>
    public IWriteHandler? DefaultWriteHandler { get; set; }

    /// <summary>
    /// Gets or sets the custom default read handler.
    /// </summary>
    public IDefaultReadHandler<object>? DefaultReadHandler { get; set; }
}
