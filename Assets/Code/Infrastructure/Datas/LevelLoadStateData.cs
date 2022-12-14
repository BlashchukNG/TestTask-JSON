namespace Code.Infrastructure.Datas
{
    public struct LoadLevelStateData
    {
        public string sceneName;
        public UserData[] userData;


        public LoadLevelStateData(string sceneName, UserData[] userData)
        {
            this.sceneName = sceneName;
            this.userData = userData;
        }
    }
}