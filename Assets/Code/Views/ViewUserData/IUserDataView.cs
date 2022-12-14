using Code.Infrastructure.Datas;
using UnityEngine;

namespace Code.Views.ViewUserData
{
    public interface IUserDataView
    {
        RectTransform RectTransform { get; }
        void Initialize(UserData data);
    }
}