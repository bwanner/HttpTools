using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Logging
{
    public interface ILogWriter : ISettingsChangable
    {
        void WriteLog(Log log);
    }
}
