using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Common.Internal
{
    public interface IHttpClient
    {
        Task<Tuple<HttpStatusCode, string>> ExecuteAsync(HttpRequest httpRequest, IProgress<ParseUploadProgressEventArgs> uploadProgress, IProgress<ParseDownloadProgressEventArgs> downloadProgress, CancellationToken cancellationToken);
    }
}
