using Broilerplate.Bt.Exceptions;
using Broilerplate.Bt.Nodes.Ports;

namespace Broilerplate.Bt.Nodes.Decorator {
    [NodeTint(NodeColors.Orange)]
    [CreateNodeMenu("Decorator/Conditional Branch")]
    public class ConditionalBranchNode : BaseNode {
        [Input(connectionType = ConnectionType.Override)]
        public Port testCase;
        
        [Output(connectionType = ConnectionType.Override)]
        public Port onSuccess;
        [Output(connectionType = ConnectionType.Override)]
        public Port onFailure;

        private BaseNode testNode;
        private BaseNode resultNode;
        private bool resultIsIn;
        
        protected override void InternalSpawn() {
            testNode = GetInput(nameof(testCase));
            testNode.Spawn();
            resultIsIn = false;
        }

        protected override TaskStatus InternalTick() {
            if (!resultIsIn) {
                if (testNode.Status == TaskStatus.Running) {
                    return TaskStatus.Running;
                }
                
                if (testNode.Status == TaskStatus.Success) {
                    resultNode = GetNext(nameof(onSuccess));
                }
                else if (testNode.Status == TaskStatus.Failure) {
                    resultNode = GetNext(nameof(onFailure));
                }
                else {
                    throw new IllegalReturnStatusException($"Cannot deal with status {testNode.Status} at this point! Expected Success or Failure");
                }

                if (resultNode != null) {
                    resultNode.Spawn();
                    resultIsIn = true;
                    return TaskStatus.Running;
                }
                // the output result port is empty. We're done
                return TaskStatus.Success;
                
            }
            return resultNode.Status;
        }

        protected override void InternalTerminate() {
            if (resultNode != null) {
                resultNode.Terminate();
            }
        }
    }
}