using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Logging
{
    public interface ILogWriter
    {
        void WriteLog(Log log);
    }
}
