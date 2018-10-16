using Batzill.Server.Core.ObjectModel;

namespace Batzill.Server.Core
{
    public interface IOperationFactory
    {
        Operation CreateMatchingOperation(HttpContext context);
        void LoadOperations();
    }
}
