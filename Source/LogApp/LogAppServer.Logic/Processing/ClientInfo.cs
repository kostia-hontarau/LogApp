using LogApp.Common.Model;

namespace LogAppServer.Logic.Processing
{
    public class ClientInfo
    {
        #region Data Members
        private readonly ApplicationInfoCollection currentState = new ApplicationInfoCollection();

        #region Constructors
        internal ClientInfo(string name)
        {
            Name = name;
        } 
        #endregion
        #endregion

        #region Properties
        public string Name { get; private set; }
        public ApplicationInfoCollection CurrentState
        {
            get { return this.currentState; }
        }
        #endregion
    }
}
