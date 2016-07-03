using System.Collections.Generic;
using ServiceSentry.Extensibility.Interfaces;

namespace ServiceSentry.Model
{
    /// <summary>
    ///     Provides a generalized mechanism for exporting an
    ///     <see cref="List{T}" /> as an extension.
    /// </summary>
    public interface IFileListExtension : IExtensionClass
    {
        /// <summary>
        ///     Gets the LogFiles.
        /// </summary>
        List<ExternalFile> Files { get; }
    }
}