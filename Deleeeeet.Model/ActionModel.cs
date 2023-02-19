using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deleeeeet.Model.Data;
using Deleeeeet.Model.Event;

namespace Deleeeeet.Model
{
    public class ActionModel
    {
        private TwitterApi? twitterApi;


        private readonly JsonLoader _jsonLoader;
        private readonly ZipLoader _zipLoader;
        public ActionModel()
        {
            _jsonLoader = new JsonLoader();
            _zipLoader = new ZipLoader(_jsonLoader);
        }

        #region Authorization
        public event Action? OnUnAuthorized;
        public event Action<Uri, RequirePinCodeEventArgs>? OnGetAuthUri;
        public event Action<string>? OnFailAuthorize;
        public event Action? OnAuthorized;
        public event Action<QueryUseSavedTokenEventArgs>? OnQueryUseSavedToken;

        public async Task Authorize()
        {
            this.twitterApi = null;
            OnUnAuthorized?.Invoke();

            var authorization = new Authorization();

            var existingTokens = authorization.LoadTokensFromDefault();
            bool useDefault = false;
            if (existingTokens == null)
            {
                useDefault = false;
            }
            else
            {
                var eventArgs = new QueryUseSavedTokenEventArgs();
                OnQueryUseSavedToken?.Invoke(eventArgs);
                useDefault = eventArgs.UseSavedToken;
            }

            if (useDefault)
            {
                await AuthorizeDefault(authorization);
            }
            else
            {
                await AuthorizeOnline(authorization);
            }

        }

        private async Task AuthorizeOnline(Authorization authorization)
        {

            (var uri, var method) = authorization.AuthorizeOnline();

            var eventArgs = new RequirePinCodeEventArgs();
            OnGetAuthUri?.Invoke(uri, eventArgs);

            if (eventArgs.PinCode == null)
            {
                OnFailAuthorize?.Invoke("pinCode is null");
                return;

            }
            CoreTweet.Tokens token = null;
            try
            {
                token = await method(eventArgs.PinCode);
            }
            catch (Exception ex)
            {
                OnFailAuthorize?.Invoke(ex.Message);
                return;
            }

            if (token == null)
            {
                OnFailAuthorize?.Invoke("token is null");
                return;
            }

            authorization.SaveTokensToDefault(token);

            this.twitterApi = new TwitterApi(token);
            OnAuthorized?.Invoke();
        }

        private async Task AuthorizeDefault(Authorization authorization)
        {
            await Task.Run(() =>
            {
                this.twitterApi = null;
                OnUnAuthorized?.Invoke();

                var token = authorization.LoadTokensFromDefault();

                if (token == null)
                {
                    OnFailAuthorize?.Invoke("offline token is null");
                    return;
                }

                this.twitterApi = new TwitterApi(token);
                OnAuthorized?.Invoke();
            });
        }

        public async Task AuthorizeOffline(string filePath)
        {
            await Task.Run(() =>
            {
                this.twitterApi = null;
                OnUnAuthorized?.Invoke();

                var authorization = new Authorization();
                var token = authorization.LoadTokens(filePath);

                if (token == null)
                {
                    OnFailAuthorize?.Invoke("offline token is null");
                    return;
                }

                this.twitterApi = new TwitterApi(token);
                OnAuthorized?.Invoke();
            });
        }
        #endregion

        #region Load
        public event Action? OnLoadTweetJsStarted;
        public event Action? OnVerifyTweetJsFailed;
        public event Action? OnLoadTweetJsFailed;
        public event Action<ITweet[]>? OnTweetJsLoaded;
        public async Task LoadTweetJs(string filePath)
        {
            await Task.Run(() =>
            {
                ILoader loader = Path.GetExtension(filePath).ToUpper() == ".JS" ? _jsonLoader : _zipLoader;
                OnLoadTweetJsStarted?.Invoke();

                if (!loader.VerifyFormat(filePath))
                {
                    OnVerifyTweetJsFailed?.Invoke();
                    return;
                }
                try
                {
                    var tweets = loader.LoadTweets(filePath);
                    OnTweetJsLoaded?.Invoke(tweets.Select(x => new Data.Tweet(x)).ToArray());
                }
                catch (Exception ex)
                {
                    OnLoadTweetJsFailed?.Invoke();
                    return;
                }
            });
        }

        #endregion


        #region delete action
        public event Action? OnDeleteStarted;
        public event Action<long, bool>? OnDeleted;
        public event Action<int, int, int, long[]>? OnDeleteEnded;
        public event Action<Exception>? OnException;
        public event Action<int, int>? OnProgress;

        private CancellationTokenSource? _deleteCancelToken;
        public void CancelDelete()
        {
            _deleteCancelToken?.Cancel();
        }

        delegate Task<DeleteStatus> DeleteAction(long id);
        public async Task DeleteTweets(long[] targetIds, bool isActual)
        {
            DeleteAction method = isActual ? DeleteTweetActual : DeleteTweetsDryRun;
            await DeleteTweets(targetIds, method);
        }

        private Task<DeleteStatus> DeleteTweetActual(long targetId)
        {
            return this.twitterApi.Delete(targetId);
        }

        private async Task<DeleteStatus> DeleteTweetsDryRun(long targetId)
        {
            await Task.Delay(30);
            return targetId % 100 == 0 ? DeleteStatus.Failed : DeleteStatus.Deleted;
        }

        private Task DeleteTweets(long[] targetIds, DeleteAction action)
        {
            _deleteCancelToken = new CancellationTokenSource();
            var token = _deleteCancelToken.Token;

            return Task.Run(async () =>
            {
                OnDeleteStarted?.Invoke();
                int deleted = 0;
                int notFound = 0;
                var failed = new List<long>();

                try
                {
                    int total = targetIds.Length;
                    for (int i = 0 ; i < total; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        long id = targetIds[i];

                        var result = await action(id);

                        switch (result)
                        {
                            case DeleteStatus.Deleted:
                                deleted++; break;
                            case DeleteStatus.NotFound:
                                notFound++; break;
                            case DeleteStatus.Failed:
                                failed.Add(id); break;
                        }
                        OnDeleted?.Invoke(id, result != DeleteStatus.Failed);
                        OnProgress?.Invoke(i + 1, total);
                    }
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(ex);
                }
                finally
                {
                    OnDeleteEnded?.Invoke(deleted, failed.Count, notFound, failed.ToArray());
                }
                return;
            });

        }
        #endregion
    }
}
