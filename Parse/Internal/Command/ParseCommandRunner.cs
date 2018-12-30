using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    public class ParseCommandRunner : IParseCommandRunner
    {
        IHttpClient Client { get; }

        IInstallationIdController InstallationIdController { get; }

        public ParseCommandRunner(IHttpClient httpClient, IInstallationIdController installationIdController)
        {
            Client = httpClient;
            InstallationIdController = installationIdController;
        }

        public Task<Tuple<HttpStatusCode, IDictionary<string, object>>> RunCommandAsync(ParseCommand command, IProgress<ParseUploadProgressEventArgs> uploadProgress = null, IProgress<ParseDownloadProgressEventArgs> downloadProgress = null, CancellationToken cancellationToken = default) => PrepareCommand(command).ContinueWith(commandTask =>
        {
            return Client.ExecuteAsync(commandTask.Result, uploadProgress, downloadProgress, cancellationToken).OnSuccess(t =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                Tuple<HttpStatusCode, string> response = t.Result;
                string contentString = response.Item2;
                int responseCode = (int) response.Item1;
                if (responseCode >= 500)
                {
                    // Server error, return InternalServerError.
                    throw new ParseException(ParseException.ErrorCode.InternalServerError, response.Item2);
                }
                else if (contentString != null)
                {
                    IDictionary<string, object> contentJson = null;
                    // TODO: Newer versions of Parse Server send certain and/or all failure results back as HTML; add case for this.
                    try
                    { contentJson = contentString.StartsWith("[") ? new Dictionary<string, object> { ["results"] = JsonProcessor.Parse(contentString) } : JsonProcessor.Parse(contentString) as IDictionary<string, object>; }
                    catch (Exception e) { throw new ParseException(ParseException.ErrorCode.OtherCause, "Invalid or alternatively-formatted response recieved from server.", e); }

                    if (responseCode < 200 || responseCode > 299)
                        throw new ParseException(contentJson.ContainsKey("code") ? (ParseException.ErrorCode) (int) (long) contentJson["code"] : ParseException.ErrorCode.OtherCause, contentJson.ContainsKey("error") ? contentJson["error"] as string : contentString);

                    return new Tuple<HttpStatusCode, IDictionary<string, object>>(response.Item1, contentJson);
                }
                return new Tuple<HttpStatusCode, IDictionary<string, object>>(response.Item1, null);
            });
        }).Unwrap();

        Task<ParseCommand> PrepareCommand(ParseCommand command)
        {
            ParseCommand newCommand = new ParseCommand(command);

            Task<ParseCommand> installationIdTask = InstallationIdController.GetAsync().ContinueWith(t =>
            {
                newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Installation-Id", t.Result.ToString()));
                return newCommand;
            });

            // TODO: Inject configuration instead of using shared static here.
            ParseClient.Configuration configuration = ParseClient.CurrentConfiguration;
            newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Application-Id", configuration.ApplicationID));
            newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Client-Version", ParseClient.VersionString));

            if (configuration.AuxiliaryHeaders != null)
                foreach (KeyValuePair<string, string> header in configuration.AuxiliaryHeaders)
                    newCommand.Headers.Add(header);

            if (!String.IsNullOrEmpty(configuration.VersionInfo.BuildVersion))
                newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-App-Build-Version", configuration.VersionInfo.BuildVersion));
            if (!String.IsNullOrEmpty(configuration.VersionInfo.DisplayVersion))
                newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-App-Display-Version", configuration.VersionInfo.DisplayVersion));
            if (!String.IsNullOrEmpty(configuration.VersionInfo.OSVersion))
                newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-OS-Version", configuration.VersionInfo.OSVersion));
            if (!String.IsNullOrEmpty(configuration.MasterKey))
                newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Master-Key", configuration.MasterKey));
            else if (!String.IsNullOrEmpty(configuration.Key))
                newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Windows-Key", configuration.Key));

            // TODO: Inject this instead of using static here.
            if (ParseUser.IsRevocableSessionEnabled)
                newCommand.Headers.Add(new KeyValuePair<string, string>("X-Parse-Revocable-Session", "1"));

            return installationIdTask;
        }
    }
}
