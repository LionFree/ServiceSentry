using System.ComponentModel.Composition;

namespace ServiceSentry.Extensibility.Interfaces
{
    /// <summary>
    ///     Provides a generalized mechanism for exporting an extension.
    /// </summary>
    public interface IExtensionClass : IPartImportsSatisfiedNotification
    {
        /// <summary>
        ///     Gets the name of this imported control.
        /// </summary>
        string ExtensionName { get; }

        /// <summary>
        ///     A boolean value that determines whether the extension will be loaded.
        /// </summary>
        bool CanExecute { get; }
    }
}