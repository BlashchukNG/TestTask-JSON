using System;
using System.Collections;
using Code.Helpers;
using Code.Infrastructure.Datas;
using UnityEngine.Networking;

namespace Code.Infrastructure.Services.JsonLoad
{
    public sealed class JsonLoadService :
        IJsonLoadService
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public JsonLoadService(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void Load(string url, Action<UserData[]> success, Action failed)
        {
            _coroutineRunner.StartCoroutine(LoadFromServer(url, success, failed));
        }

        IEnumerator LoadFromServer(string url, Action<UserData[]> success, Action failed)
        {
            var request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (!request.isHttpError && !request.isNetworkError)
            {
                var data = JsonHelper.GetArray<UserData>(request.downloadHandler.text);
                success?.Invoke(data);
            }
            else
            {
                failed.Invoke();
            }

            request.Dispose();
        }
    }
}