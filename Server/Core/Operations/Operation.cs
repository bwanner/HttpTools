using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Authentication;
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

        protected Operation(Logger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Used to initialize the operation before Executing.
        /// </summary>
        /// <param name="logger">The operation logger</param>
        /// <param name="operationId">The Id of the operation</param>
        public void Initialize(Logger logger, string operationId)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ID = operationId;
        }

        public void Execute(HttpContext context, IAuthenticationManager authManager)
        {
            try
            {
                this.ExecuteInternal(context, authManager);
            }
            catch(Exception ex)
            {
                /* Log exception in operation logs */
                this.logger?.Log(EventType.OperationError, $"Operation failed with exception '{ex}'.");
                throw ex;
            }
        }

        /// <summary>
        /// Used to initialize static properties at the beginning of the operation.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="settings">The settings.</param>
        public virtual void InitializeClass(OperationSettings settings, IAuthenticationManager authManager) { }
        public abstract bool Match(HttpContext context);
        protected abstract void ExecuteInternal(HttpContext context, IAuthenticationManager authManager);
    }
}
