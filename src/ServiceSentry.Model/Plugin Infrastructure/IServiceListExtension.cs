using System.Collections.Generic;
using ServiceSentry.Extensibility.Interfaces;

namespace ServiceSentry.Model
{
    /// <summary>
    ///     Provides a generalized mechanism for exporting an
    ///     <see cref="List{T}" /> as an extension.
    /// </summary>
    public interface IServiceListExtension : IExtensionClass
    {
        /// <summary>
        ///     Gets the Service List.
        /// </summary>
        List<Service> Services { get; }

        List<ExternalFile> OtherFiles { get; }
    }
}