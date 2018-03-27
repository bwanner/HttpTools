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
        public abstract int Priority
        {
            get;
        }

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

        protected HttpServerSettings settings
        {
            get; private set;
        }        

        protected Operation()
        {
        }

        /// <summary>
        /// Used to initialize the operation before Executing.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="operationId">The Id of the operation</param>
        public void Initialize(Logger logger, HttpServerSettings settings, string operationId)
        {
            this.logger = logger;
            this.settings = settings.Clone();
            this.ID = operationId;
        }

        /// <summary>
        /// Used to initialize static properties at the beginning of the operation.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="settings">The settings.</param>
        public virtual void InitializeClass(Logger logger, HttpServerSettings settings) { }

        public abstract bool Match(HttpContext context);
        public abstract void Execute(HttpContext context);
    }
}
