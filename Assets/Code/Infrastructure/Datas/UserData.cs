using System;

namespace Code.Infrastructure.Datas
{
    [Serializable]
    public sealed class UserData
    {
        public int id;
        public string first_name;
        public string last_name;
        public string email;
        public string gender;
        public string ip_address;
        public string address;
    }
}