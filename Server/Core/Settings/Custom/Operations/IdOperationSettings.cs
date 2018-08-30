namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class IdOperationSettings : OperationSettings
    {
        private string id = "UNKNOWN";
        public string ServerId
        {
            get => this.id;
            set
            {
                this.id = value;
            }
        }
    }
}
