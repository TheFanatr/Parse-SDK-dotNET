using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    public class ParseFileController : IParseFileController
    {
        IParseCommandRunner Runner { get; }

        public ParseFileController(IParseCommandRunner commandRunner) => Runner = commandRunner;

        public Task<FileState> SaveAsync(FileState state, Stream dataStream, string sessionToken, IProgress<ParseUploadProgressEventArgs> progress, CancellationToken cancellationToken = default)
        {
            if (state.Url != null)
                return Task.FromResult(state);

            if (cancellationToken.IsCancellationRequested)
            {
                TaskCompletionSource<FileState> tcs = new TaskCompletionSource<FileState> { };
                tcs.TrySetCanceled();
                return tcs.Task;
            }

            long oldPosition = dataStream.Position;
            ParseCommand command = new ParseCommand("files/" + state.Name, "POST", sessionToken, stream: dataStream, contentType: state.MimeType);

            return Runner.RunCommandAsync(command, progress, cancellationToken: cancellationToken).OnSuccess(uploadTask =>
            {
                IDictionary<string, object> jsonData = uploadTask.Result.Item2;
                cancellationToken.ThrowIfCancellationRequested();

                return new FileState
                {
                    Name = jsonData["name"] as string,
                    Url = new Uri(jsonData["url"] as string, UriKind.Absolute),
                    MimeType = state.MimeType
                };
            }).ContinueWith(t =>
            {
                if ((t.IsFaulted || t.IsCanceled) && dataStream.CanSeek)
                    dataStream.Seek(oldPosition, SeekOrigin.Begin);

                return t;
            }).Unwrap();
        }
    }
}
