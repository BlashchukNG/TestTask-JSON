using Code.Infrastructure.Datas;
using TMPro;
using UnityEngine;

namespace Code.Views.ViewUserData
{
    public sealed class UserDataView :
        MonoBehaviour,
        IUserDataView
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TMP_Text _txtId;
        [SerializeField] private TMP_Text _txtFirstName;
        [SerializeField] private TMP_Text _txtLastName;
        [SerializeField] private TMP_Text _txtEmail;
        [SerializeField] private TMP_Text _txtGender;
        [SerializeField] private TMP_Text _txtIpAddress;
        [SerializeField] private TMP_Text _txtAddress;

        public RectTransform RectTransform => _rectTransform;

        public void Initialize(UserData data)
        {
            _txtId.text = data.id.ToString();
            _txtFirstName.text = data.first_name;
            _txtLastName.text = data.last_name;
            _txtEmail.text = data.email;
            _txtGender.text = data.gender;
            _txtIpAddress.text = data.ip_address;
            _txtAddress.text = data.address;
        }
    }
}