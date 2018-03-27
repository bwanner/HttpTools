using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Operations;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core
{
    public class AssemblyOperationFactory : IOperationFactory
    {
        private Logger logger;
        private SortedSet<Operation> operations;

        public AssemblyOperationFactory(Logger logger, HttpServerSettings settings = null)
        {
            this.logger = logger;
            this.operations = new SortedSet<Operation>(new OperationPriorityComparer());

            this.ApplySettings(settings);
        }

        public void LoadOperations(HttpServerSettings settings)
        {
            lock (this.operations)
            {
                this.logger.Log(EventType.OperationLoading, "Attempting to load all available operations.");

                var operationsTypes = (from operation in Assembly.GetExecutingAssembly().GetTypes()
                                       where operation.IsClass && operation.IsSubclassOf(typeof(Operation))
                                       select operation).ToList();

                this.logger.Log(EventType.OperationLoading, "Found {0} operations in the assembly .", operationsTypes.Count);
                this.logger.Log(EventType.OperationLoading, "Start instanciating all operationTypes.");

                this.operations.Clear();

                foreach (Type operationType in operationsTypes)
                {
                    try
                    {
                        this.logger.Log(EventType.OperationLoading, "Attempting to instanciate operationType '{0}'.", operationType);

                        Operation operation = this.CreateInstance(operationType);

                        this.logger.Log(EventType.OperationLoading, "Attempting to initialize operation '{0}'.", operation.Name);

                        operation.InitializeClass(this.logger, settings);

                        this.logger.Log(EventType.OperationLoading, "Operation loaded. Name: '{0}', Priority: '{1}'.", operation.Name, operation.Priority);

                        this.operations.Add(operation);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log(EventType.OperationLoadingError, "An error occured while instanciating/initializing operationType '{0}': {1}", operationType, ex.ToString());
                    }
                }

                this.logger.Log(EventType.OperationLoading, "Finished loading operations: Successful: '{0}', Failed: '{1}'.", this.operations.Count, (operationsTypes.Count - this.operations.Count));
            }
        }

        public Operation CreateMatchingOperation(HttpContext context)
        {
            lock (this.operations)
            {
                Operation result = null;
                foreach (Operation operation in this.operations)
                {
                    if (operation.Match(context))
                    {
                        this.logger.Log(EventType.OperationLoading, "Found matching operation. Name: '{0}', Priority: '{1}'.", operation.Name, operation.Priority);
                        
                        result = this.CreateInstance(operation.GetType());

                        break;
                    }
                }

                // return default operation if no operatin was found
                if (result == null)
                {
                    this.logger.Log(
                        EventType.OperationLoading,
                        "No matching operation found , defaulting to 'EchoOperation'.");
                    
                    result = this.CreateInstance(typeof(EchoOperation));
                }

                return result;
            }
        }

        private Operation CreateInstance(Type operationType)
        {
            if (!operationType.IsSubclassOf(typeof(Operation)))
            {
                this.logger.Log(EventType.OperationLoadingError, "Invalid type passed, can't convert '{0}' to an operation", operationType);
                throw new ArgumentException("operationType");
            }
            
            return (Operation)Activator.CreateInstance(operationType);
        }

        public bool ApplySettings(HttpServerSettings settings)
        {
            return true;
        }
    }
}
