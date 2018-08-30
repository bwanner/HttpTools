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
        private HttpServerSettings settings;
        private SortedSet<Tuple<int, Operation>> operations;

        public AssemblyOperationFactory(Logger logger, HttpServerSettings settings)
        {
            this.logger = logger;
            this.settings = settings;

            this.operations = new SortedSet<Tuple<int, Operation>>(new OperationPriorityComparer());
        }

        public void LoadOperations()
        {
            lock (this.operations)
            {
                this.logger?.Log(EventType.OperationLoading, "Attempting to load all available operations.");

                var operationsTypes = (from operation in Assembly.GetExecutingAssembly().GetTypes()
                                       where operation.IsClass && operation.IsSubclassOf(typeof(Operation))
                                       select operation).ToList();

                this.logger?.Log(EventType.OperationLoading, "Found {0} operations in the assembly .", operationsTypes.Count);

                var requestedOperations = this.settings.Operations.ToDictionary((op) => op.Name, StringComparer.InvariantCultureIgnoreCase);

                this.logger?.Log(EventType.OperationLoading, "Found {0} operations in the settings .", requestedOperations.Count);

                this.logger?.Log(EventType.OperationLoading, "Start instanciating all requested operationTypes.");

                this.operations.Clear();

                int skippedCount = 0;
                foreach (Type operationType in operationsTypes)
                {
                    try
                    {
                        this.logger?.Log(EventType.OperationLoading, "Attempting to instanciate operationType '{0}'.", operationType);

                        Operation operation = this.CreateInstance(operationType);
                        string operationName = operation.Name;

                        this.logger?.Log(EventType.OperationLoading, "OperationType '{0}' is registered as '{1}'.", operationType, operationName);

                        if (!requestedOperations.ContainsKey(operationName))
                        {
                            this.logger?.Log(EventType.OperationLoading, "Skipping '{0}' as it wasn't requested.", operationName);

                            skippedCount++;

                            continue;
                        }

                        var operationSettings = requestedOperations[operationName];

                        this.logger?.Log(EventType.OperationLoading, "Attempting to execute one-time initialization for operation '{0}'.", operation.Name);

                        operation.InitializeClass(operationSettings);

                        this.logger?.Log(EventType.OperationLoading, "Operation loaded. Name: '{0}', Priority: '{1}'.", operation.Name, operationSettings.Priority);

                        this.operations.Add(new Tuple<int, Operation>(operationSettings.Priority, operation));
                    }
                    catch (Exception ex)
                    {
                        this.logger?.Log(EventType.OperationLoadingError, "An error occured while instanciating/initializing operationType '{0}': {1}", operationType, ex);
                    }
                }

                this.logger?.Log(
                    EventType.OperationLoading, 
                    "Finished loading operations: Successful: '{0}', Skipped: '{1}', Failed: '{2}'.",
                    this.operations.Count,
                    skippedCount,
                    requestedOperations.Count - this.operations.Count);
            }
        }

        public Operation CreateMatchingOperation(HttpContext context)
        {
            lock (this.operations)
            {
                Operation result = null;
                foreach (Tuple<int, Operation> operationEntry in this.operations)
                {
                    Operation operation = operationEntry.Item2;
                    int priority = operationEntry.Item1;

                    if (operation.Match(context))
                    {
                        this.logger?.Log(EventType.OperationLoading, "Found matching operation. Name: '{0}', Priority: '{1}'.", operation.Name, priority);
                        
                        result = this.CreateInstance(operation.GetType());

                        break;
                    }
                }

                return result;
            }
        }

        private Operation CreateInstance(Type operationType)
        {
            return (Operation)Activator.CreateInstance(operationType);
        }
    }
}
