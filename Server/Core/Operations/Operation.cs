using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core
{
    public abstract class Operation
    {
        public abstract string Name
        {
            get;
        }

        public string ID
        {
            get; private set;
        }

        protected Logger logger
        {
            get; private set;
        }

        protected Operation()
        {
        }

        /// <summary>
        /// Used to initialize the operation before Executing.
        /// </summary>
        /// <param name="operationId">The Id of the operation</param>
        public void Initialize(Logger logger, string operationId)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ID = operationId;
        }

        /// <summary>
        /// Used to initialize static properties at the beginning of the operation.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="settings">The settings.</param>
        public virtual void InitializeClass(OperationSettings settings) { }

        public abstract bool Match(HttpContext context);
        public abstract void Execute(HttpContext context);
    }
}
